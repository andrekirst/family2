# CI/CD Pipeline - Family Hub

**Version:** 1.0
**Date:** 2025-12-19
**Status:** Implementation Ready
**Author:** DevOps Team (Claude Code)

---

## Executive Summary

This document defines the complete CI/CD pipeline for Family Hub using GitHub Actions for CI and ArgoCD for GitOps-based continuous deployment. The pipeline supports automated testing, security scanning, Docker image builds, and environment promotion strategies.

**Pipeline Flow:**

```
Git Push → GitHub Actions (Build/Test) → Docker Registry → ArgoCD (Deploy) → Kubernetes
```

---

## Table of Contents

1. [GitHub Actions Workflows](#1-github-actions-workflows)
2. [GitOps with ArgoCD](#2-gitops-with-argocd)
3. [Environment Promotion](#3-environment-promotion)
4. [Rollback Procedures](#4-rollback-procedures)
5. [Security Scanning](#5-security-scanning)

---

## 1. GitHub Actions Workflows

### 1.1 Main CI Workflow (.github/workflows/ci.yml)

```yaml
name: CI Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_PREFIX: ghcr.io/${{ github.repository }}

jobs:
  # Job 1: Build and Test Backend Services
  build-backend:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        service:
          [
            auth,
            calendar,
            task,
            shopping,
            health,
            meal-planning,
            finance,
            communication,
          ]

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "10.0.x"

      - name: Restore dependencies
        run: dotnet restore src/Services/${{ matrix.service }}-service

      - name: Build
        run: dotnet build src/Services/${{ matrix.service }}-service --no-restore --configuration Release

      - name: Run unit tests
        run: dotnet test src/Services/${{ matrix.service }}-service/tests --no-build --configuration Release --verbosity normal --logger "trx;LogFileName=test-results.trx"

      - name: Publish test results
        uses: dorny/test-reporter@v1
        if: always()
        with:
          name: Test Results - ${{ matrix.service }}
          path: "**/test-results.trx"
          reporter: dotnet-trx

      - name: Code coverage
        run: dotnet test src/Services/${{ matrix.service }}-service/tests --no-build --configuration Release --collect:"XPlat Code Coverage"

      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: "**/coverage.cobertura.xml"
          flags: ${{ matrix.service }}-service

  # Job 2: Build Frontend
  build-frontend:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: "21"
          cache: "npm"
          cache-dependency-path: src/Frontend/package-lock.json

      - name: Install dependencies
        run: npm ci
        working-directory: src/Frontend

      - name: Lint
        run: npm run lint
        working-directory: src/Frontend

      - name: Run tests
        run: npm run test:ci
        working-directory: src/Frontend

      - name: Build
        run: npm run build:prod
        working-directory: src/Frontend

      - name: Upload build artifacts
        uses: actions/upload-artifact@v4
        with:
          name: frontend-build
          path: src/Frontend/dist

  # Job 3: Security Scanning
  security-scan:
    runs-on: ubuntu-latest
    needs: [build-backend, build-frontend]

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Run Trivy vulnerability scanner (filesystem)
        uses: aquasecurity/trivy-action@master
        with:
          scan-type: "fs"
          scan-ref: "."
          format: "sarif"
          output: "trivy-results.sarif"

      - name: Upload Trivy results to GitHub Security
        uses: github/codeql-action/upload-sarif@v3
        if: always()
        with:
          sarif_file: "trivy-results.sarif"

      - name: OWASP Dependency Check
        uses: dependency-check/Dependency-Check_Action@main
        with:
          project: "family-hub"
          path: "."
          format: "HTML"

  # Job 4: Build and Push Docker Images
  build-push-images:
    runs-on: ubuntu-latest
    needs: [build-backend, build-frontend, security-scan]
    if: github.ref == 'refs/heads/main'
    strategy:
      matrix:
        service:
          [
            auth,
            calendar,
            task,
            shopping,
            health,
            meal-planning,
            finance,
            communication,
            api-gateway,
            frontend,
          ]

    permissions:
      contents: read
      packages: write

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Log in to GitHub Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.IMAGE_PREFIX }}/${{ matrix.service }}-service
          tags: |
            type=sha,prefix=
            type=ref,event=branch
            type=semver,pattern={{version}}
            type=semver,pattern={{major}}.{{minor}}

      - name: Build and push Docker image
        uses: docker/build-push-action@v5
        with:
          context: .
          file: src/Services/${{ matrix.service }}-service/Dockerfile
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max

      - name: Run Trivy vulnerability scanner (image)
        uses: aquasecurity/trivy-action@master
        with:
          image-ref: ${{ env.IMAGE_PREFIX }}/${{ matrix.service }}-service:${{ github.sha }}
          format: "sarif"
          output: "trivy-image-results.sarif"

      - name: Upload Trivy image results
        uses: github/codeql-action/upload-sarif@v3
        if: always()
        with:
          sarif_file: "trivy-image-results.sarif"

  # Job 5: Update Helm values for ArgoCD
  update-helm-values:
    runs-on: ubuntu-latest
    needs: build-push-images
    if: github.ref == 'refs/heads/main'

    steps:
      - name: Checkout helm charts repo
        uses: actions/checkout@v4
        with:
          repository: yourorg/familyhub-helm-charts
          token: ${{ secrets.HELM_REPO_TOKEN }}
          path: helm-charts

      - name: Update image tags in values.yaml
        run: |
          cd helm-charts
          # Update each service image tag to new SHA
          for service in auth calendar task shopping health meal-planning finance communication api-gateway frontend; do
            yq eval ".${service}-service.image.tag = \"${{ github.sha }}\"" -i values-dev.yaml
          done
          git config user.name "GitHub Actions"
          git config user.email "actions@github.com"
          git add values-dev.yaml
          git commit -m "Update image tags to ${{ github.sha }}"
          git push
```

### 1.2 Release Workflow (.github/workflows/release.yml)

```yaml
name: Release

on:
  push:
    tags:
      - "v*.*.*"

jobs:
  release:
    runs-on: ubuntu-latest
    permissions:
      contents: write

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Create GitHub Release
        uses: softprops/action-gh-release@v1
        with:
          generate_release_notes: true
          draft: false
          prerelease: false

      - name: Build and push release images
        # Similar to build-push-images but with release tags
        run: |
          echo "Building release images for ${{ github.ref_name }}"
```

---

## 2. GitOps with ArgoCD

### 2.1 ArgoCD Application Setup

```yaml
# argocd/family-hub-dev.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: family-hub-dev
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://github.com/yourorg/familyhub-helm-charts.git
    targetRevision: main
    path: family-hub-helm
    helm:
      valueFiles:
        - values-dev.yaml
  destination:
    server: https://kubernetes.default.svc
    namespace: family-hub-dev
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
      allowEmpty: false
    syncOptions:
      - CreateNamespace=true
    retry:
      limit: 5
      backoff:
        duration: 5s
        factor: 2
        maxDuration: 3m

---
# argocd/family-hub-staging.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: family-hub-staging
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://github.com/yourorg/familyhub-helm-charts.git
    targetRevision: main
    path: family-hub-helm
    helm:
      valueFiles:
        - values-staging.yaml
  destination:
    server: https://kubernetes.default.svc
    namespace: family-hub-staging
  syncPolicy:
    automated:
      prune: false
      selfHeal: true
    syncOptions:
      - CreateNamespace=true

---
# argocd/family-hub-production.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: family-hub-production
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://github.com/yourorg/familyhub-helm-charts.git
    targetRevision: main
    path: family-hub-helm
    helm:
      valueFiles:
        - values-production.yaml
  destination:
    server: https://kubernetes.default.svc
    namespace: family-hub
  syncPolicy:
    automated:
      prune: false
      selfHeal: false # Manual sync for production
    syncOptions:
      - CreateNamespace=true
  ignoreDifferences:
    - group: apps
      kind: Deployment
      jsonPointers:
        - /spec/replicas # Ignore HPA-managed replicas
```

### 2.2 ArgoCD CLI Commands

```bash
# Login to ArgoCD
argocd login argocd.familyhub.yourdomain.com --username admin

# List applications
argocd app list

# Get application status
argocd app get family-hub-production

# Sync application (manual)
argocd app sync family-hub-production

# Watch sync progress
argocd app sync family-hub-production --watch

# Rollback to previous version
argocd app rollback family-hub-production

# View application history
argocd app history family-hub-production

# View diff before sync
argocd app diff family-hub-production
```

---

## 3. Environment Promotion

### 3.1 Promotion Strategy

```
Development (Auto-deploy from main)
         ↓
    Git Tag (v1.2.3)
         ↓
Staging (Auto-deploy from tag)
         ↓
   Manual Approval
         ↓
Production (Manual sync)
```

### 3.2 Promotion Workflow

**Step 1: Development → Staging**

```bash
# After successful dev testing, create release tag
git tag -a v1.2.3 -m "Release v1.2.3"
git push origin v1.2.3

# GitHub Actions builds release images with v1.2.3 tag
# ArgoCD automatically deploys to staging

# Monitor staging deployment
argocd app get family-hub-staging --watch

# Run smoke tests in staging
kubectl run smoke-tests -n family-hub-staging \
  --image=familyhub/smoke-tests:latest \
  --restart=Never \
  --rm -it

# Check staging logs for errors
kubectl logs -l app.kubernetes.io/part-of=family-hub \
  -n family-hub-staging --since=15m | grep -i "error"
```

**Step 2: Staging → Production**

```bash
# Update production values with verified image tags
cd familyhub-helm-charts
vi values-production.yaml

# Change:
# calendar-service:
#   image:
#     tag: "sha256:abc123"  # Old
# To:
#     tag: "v1.2.3"  # New verified tag

# Commit and push
git add values-production.yaml
git commit -m "Promote v1.2.3 to production"
git push

# Manual sync in ArgoCD (with approval)
argocd app sync family-hub-production

# Monitor deployment
argocd app get family-hub-production --watch
kubectl rollout status deployment -n family-hub

# Verify health
curl https://familyhub.yourdomain.com/health
```

---

## 4. Rollback Procedures

### 4.1 ArgoCD Rollback

```bash
# View deployment history
argocd app history family-hub-production

# Rollback to previous version
argocd app rollback family-hub-production

# Rollback to specific revision
argocd app rollback family-hub-production 5

# Force sync if needed
argocd app sync family-hub-production --force
```

### 4.2 Kubernetes Rollback

```bash
# View deployment history
kubectl rollout history deployment calendar-service -n family-hub

# Rollback deployment
kubectl rollout undo deployment calendar-service -n family-hub

# Rollback to specific revision
kubectl rollout undo deployment calendar-service --to-revision=3 -n family-hub

# Check rollback status
kubectl rollout status deployment calendar-service -n family-hub
```

### 4.3 Helm Rollback

```bash
# List Helm releases
helm list -n family-hub

# View release history
helm history family-hub -n family-hub

# Rollback to previous release
helm rollback family-hub -n family-hub

# Rollback to specific revision
helm rollback family-hub 3 -n family-hub
```

---

## 5. Security Scanning

### 5.1 SAST (Static Application Security Testing)

```yaml
# .github/workflows/sast.yml
name: SAST

on:
  push:
    branches: [main]
  pull_request:

jobs:
  codeql:
    runs-on: ubuntu-latest
    permissions:
      security-events: write

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Initialize CodeQL
        uses: github/codeql-action/init@v3
        with:
          languages: csharp, javascript

      - name: Autobuild
        uses: github/codeql-action/autobuild@v3

      - name: Perform CodeQL Analysis
        uses: github/codeql-action/analyze@v3
```

### 5.2 Dependency Scanning

```yaml
# Dependabot config (.github/dependabot.yml)
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
    open-pull-requests-limit: 10

  - package-ecosystem: "npm"
    directory: "/src/Frontend"
    schedule:
      interval: "weekly"
    open-pull-requests-limit: 10

  - package-ecosystem: "docker"
    directory: "/"
    schedule:
      interval: "weekly"

  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"
```

### 5.3 Container Scanning

```bash
# Trivy scans in CI pipeline (already in main workflow)
# Local scanning:
trivy image familyhub/calendar-service:latest

# Scan for HIGH and CRITICAL vulnerabilities only
trivy image --severity HIGH,CRITICAL familyhub/calendar-service:latest

# Scan and fail on CRITICAL
trivy image --exit-code 1 --severity CRITICAL familyhub/calendar-service:latest
```

---

## Appendix: Pipeline Metrics

### Key Performance Indicators

```yaml
# Target metrics:
- Build time: < 5 minutes
- Test time: < 3 minutes
- Total pipeline time: < 10 minutes
- Deploy time (ArgoCD): < 2 minutes
- Rollback time: < 1 minute
- Pipeline success rate: > 95%
- Security scan findings: 0 critical, < 5 high
```

---

**Document Status:** Implementation Ready
**Last Updated:** 2025-12-19
**Maintained By:** DevOps Team
