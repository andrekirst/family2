# Kubernetes Deployment Guide - Family Hub

**Version:** 1.0
**Date:** 2025-12-19
**Status:** Production Ready
**Author:** DevOps Engineer (Claude Code)

---

## Executive Summary

This guide provides step-by-step deployment procedures for Family Hub on Kubernetes. It covers local development, staging, and production environments with cloud-agnostic instructions.

**Target Audience:** DevOps engineers, SREs, developers deploying Family Hub

**Prerequisites:**

- Kubernetes 1.27+ cluster access
- kubectl 1.27+ installed
- Helm 3.12+ installed
- Git access to Family Hub repositories

---

## Table of Contents

1. [Environment Setup](#1-environment-setup)
2. [Initial Cluster Configuration](#2-initial-cluster-configuration)
3. [Core Infrastructure Deployment](#3-core-infrastructure-deployment)
4. [Application Deployment](#4-application-deployment)
5. [Verification and Testing](#5-verification-and-testing)
6. [Scaling Procedures](#6-scaling-procedures)
7. [Disaster Recovery](#7-disaster-recovery)
8. [Troubleshooting Guide](#8-troubleshooting-guide)

---

## 1. Environment Setup

### 1.1 Local Development Environment

**Option A: Docker Desktop with Kubernetes**

```bash
# 1. Install Docker Desktop
# Download from: https://www.docker.com/products/docker-desktop

# 2. Enable Kubernetes in Docker Desktop
# Settings → Kubernetes → Enable Kubernetes → Apply & Restart

# 3. Verify installation
kubectl version --client
kubectl cluster-info

# Expected output:
# Kubernetes control plane is running at https://kubernetes.docker.internal:6443
```

**Option B: Minikube**

```bash
# 1. Install Minikube
# macOS:
brew install minikube

# Linux:
curl -LO https://storage.googleapis.com/minikube/releases/latest/minikube-linux-amd64
sudo install minikube-linux-amd64 /usr/local/bin/minikube

# Windows:
# Download from: https://github.com/kubernetes/minikube/releases

# 2. Start Minikube cluster
minikube start --cpus=4 --memory=8192 --disk-size=50g

# 3. Enable addons
minikube addons enable ingress
minikube addons enable metrics-server

# 4. Verify installation
kubectl get nodes
```

**Option C: k3d (Lightweight Kubernetes in Docker)**

```bash
# 1. Install k3d
curl -s https://raw.githubusercontent.com/k3d-io/k3d/main/install.sh | bash

# 2. Create cluster
k3d cluster create familyhub \
  --agents 2 \
  --port "8080:80@loadbalancer" \
  --port "8443:443@loadbalancer"

# 3. Verify installation
kubectl get nodes
```

### 1.2 Cloud Provider Setup

**DigitalOcean Kubernetes (Recommended for Cost)**

```bash
# 1. Install doctl CLI
# macOS:
brew install doctl

# Linux:
cd ~
wget https://github.com/digitalocean/doctl/releases/download/v1.98.1/doctl-1.98.1-linux-amd64.tar.gz
tar xf doctl-1.98.1-linux-amd64.tar.gz
sudo mv doctl /usr/local/bin

# 2. Authenticate
doctl auth init
# Enter your DigitalOcean API token

# 3. Create Kubernetes cluster
doctl kubernetes cluster create familyhub-prod \
  --region nyc3 \
  --version 1.28.2-do.0 \
  --node-pool "name=workers;size=s-4vcpu-8gb;count=3;auto-scale=true;min-nodes=2;max-nodes=5"

# 4. Configure kubectl
doctl kubernetes cluster kubeconfig save familyhub-prod

# 5. Verify connection
kubectl get nodes
```

**AWS EKS**

```bash
# 1. Install eksctl
# macOS:
brew tap weaveworks/tap
brew install weaveworks/tap/eksctl

# Linux:
curl --silent --location "https://github.com/weaveworks/eksctl/releases/latest/download/eksctl_$(uname -s)_amd64.tar.gz" | tar xz -C /tmp
sudo mv /tmp/eksctl /usr/local/bin

# 2. Create EKS cluster
eksctl create cluster \
  --name familyhub-prod \
  --region us-east-1 \
  --nodegroup-name workers \
  --node-type t3.medium \
  --nodes 3 \
  --nodes-min 2 \
  --nodes-max 5 \
  --managed

# 3. Update kubeconfig
aws eks update-kubeconfig --region us-east-1 --name familyhub-prod

# 4. Verify
kubectl get nodes
```

**Azure AKS**

```bash
# 1. Install Azure CLI
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# 2. Login
az login

# 3. Create resource group
az group create --name familyhub-rg --location eastus

# 4. Create AKS cluster
az aks create \
  --resource-group familyhub-rg \
  --name familyhub-prod \
  --node-count 3 \
  --node-vm-size Standard_DS2_v2 \
  --enable-cluster-autoscaler \
  --min-count 2 \
  --max-count 5 \
  --generate-ssh-keys

# 5. Get credentials
az aks get-credentials --resource-group familyhub-rg --name familyhub-prod

# 6. Verify
kubectl get nodes
```

**GCP GKE**

```bash
# 1. Install gcloud CLI
# macOS:
brew install --cask google-cloud-sdk

# Linux:
curl https://sdk.cloud.google.com | bash
exec -l $SHELL

# 2. Initialize and authenticate
gcloud init
gcloud auth login

# 3. Create GKE cluster
gcloud container clusters create familyhub-prod \
  --zone us-central1-a \
  --num-nodes 3 \
  --machine-type n1-standard-2 \
  --enable-autoscaling \
  --min-nodes 2 \
  --max-nodes 5

# 4. Get credentials
gcloud container clusters get-credentials familyhub-prod --zone us-central1-a

# 5. Verify
kubectl get nodes
```

### 1.3 Tool Installation

**Required Tools:**

```bash
# Helm (Package Manager)
curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash

# ArgoCD CLI
curl -sSL -o argocd-linux-amd64 https://github.com/argoproj/argo-cd/releases/latest/download/argocd-linux-amd64
sudo install -m 555 argocd-linux-amd64 /usr/local/bin/argocd
rm argocd-linux-amd64

# Kubectx and Kubens (Context switching)
sudo git clone https://github.com/ahmetb/kubectx /opt/kubectx
sudo ln -s /opt/kubectx/kubectx /usr/local/bin/kubectx
sudo ln -s /opt/kubectx/kubens /usr/local/bin/kubens

# k9s (Terminal UI for Kubernetes)
# macOS:
brew install k9s

# Linux:
curl -sS https://webinstall.dev/k9s | bash

# Verify installations
helm version
argocd version --client
kubectx
k9s version
```

---

## 2. Initial Cluster Configuration

### 2.1 Create Namespaces

```bash
# Create all required namespaces
kubectl create namespace family-hub
kubectl create namespace family-hub-data
kubectl create namespace monitoring
kubectl create namespace ingress-nginx
kubectl create namespace cert-manager
kubectl create namespace argocd

# Label namespaces for network policies
kubectl label namespace family-hub name=family-hub
kubectl label namespace family-hub-data name=family-hub-data
kubectl label namespace monitoring name=monitoring
kubectl label namespace ingress-nginx name=ingress-nginx

# Set default namespace (optional, for convenience)
kubens family-hub

# Verify namespaces
kubectl get namespaces
```

### 2.2 Install NGINX Ingress Controller

```bash
# Add Helm repository
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

# Install NGINX Ingress Controller
helm install ingress-nginx ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --set controller.replicaCount=2 \
  --set controller.service.type=LoadBalancer \
  --set controller.metrics.enabled=true \
  --set controller.metrics.serviceMonitor.enabled=true \
  --set controller.podAnnotations."prometheus\.io/scrape"=true \
  --set controller.podAnnotations."prometheus\.io/port"=10254

# Wait for Load Balancer IP
kubectl wait --namespace ingress-nginx \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/component=controller \
  --timeout=300s

# Get external IP
kubectl get service ingress-nginx-controller -n ingress-nginx

# Expected output:
# NAME                       TYPE           CLUSTER-IP      EXTERNAL-IP     PORT(S)
# ingress-nginx-controller   LoadBalancer   10.245.123.45   167.99.123.45   80:31234/TCP,443:31567/TCP
```

### 2.3 Install Cert-Manager (TLS Certificates)

```bash
# Install cert-manager CRDs
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.crds.yaml

# Add Helm repository
helm repo add jetstack https://charts.jetstack.io
helm repo update

# Install cert-manager
helm install cert-manager jetstack/cert-manager \
  --namespace cert-manager \
  --version v1.13.0 \
  --set installCRDs=false \
  --set prometheus.enabled=true

# Wait for cert-manager to be ready
kubectl wait --namespace cert-manager \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/instance=cert-manager \
  --timeout=300s

# Verify installation
kubectl get pods -n cert-manager
```

**Create Let's Encrypt ClusterIssuer:**

```bash
cat <<EOF | kubectl apply -f -
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-staging
spec:
  acme:
    server: https://acme-staging-v02.api.letsencrypt.org/directory
    email: admin@familyhub.yourdomain.com
    privateKeySecretRef:
      name: letsencrypt-staging
    solvers:
    - http01:
        ingress:
          class: nginx
---
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@familyhub.yourdomain.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
EOF

# Verify ClusterIssuers
kubectl get clusterissuer
```

### 2.4 Install Sealed Secrets (Secret Management)

```bash
# Install Sealed Secrets controller
kubectl apply -f https://github.com/bitnami-labs/sealed-secrets/releases/download/v0.24.0/controller.yaml

# Install kubeseal CLI
# macOS:
brew install kubeseal

# Linux:
wget https://github.com/bitnami-labs/sealed-secrets/releases/download/v0.24.0/kubeseal-0.24.0-linux-amd64.tar.gz
tar xfz kubeseal-0.24.0-linux-amd64.tar.gz
sudo install -m 755 kubeseal /usr/local/bin/kubeseal

# Verify installation
kubectl get pods -n kube-system | grep sealed-secrets
kubeseal --version
```

### 2.5 Configure Storage Classes

**Cloud Provider Storage Class (Auto-Configured):**

Most cloud providers automatically create a default storage class. Verify:

```bash
kubectl get storageclass

# Expected output (example for DigitalOcean):
# NAME                         PROVISIONER                 RECLAIMPOLICY
# do-block-storage (default)   dobs.csi.digitalocean.com   Delete
```

**Create Custom Storage Class (if needed):**

```bash
cat <<EOF | kubectl apply -f -
apiVersion: storage.k8s.io/v1
kind: StorageClass
metadata:
  name: family-hub-fast
provisioner: kubernetes.io/gce-pd  # Change based on cloud provider
parameters:
  type: pd-ssd  # Fast SSD storage
  replication-type: regional-pd  # Regional replication
allowVolumeExpansion: true
volumeBindingMode: WaitForFirstConsumer
reclaimPolicy: Retain  # Don't delete volume when PVC is deleted
EOF

# Verify
kubectl get storageclass family-hub-fast
```

---

## 3. Core Infrastructure Deployment

### 3.1 Deploy PostgreSQL

**Option A: Using Bitnami Helm Chart (Recommended for Production)**

```bash
# Add Bitnami repository
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update

# Create PostgreSQL values file
cat > postgres-values.yaml <<EOF
global:
  postgresql:
    auth:
      postgresPassword: "CHANGE_ME_STRONG_PASSWORD"
      database: "familyhub"

architecture: replication  # Primary + read replicas
replication:
  numSynchronousReplicas: 1

primary:
  persistence:
    enabled: true
    storageClass: ""  # Use default storage class
    size: 50Gi
  resources:
    requests:
      cpu: 500m
      memory: 2Gi
    limits:
      cpu: 2000m
      memory: 4Gi

readReplicas:
  replicaCount: 1
  persistence:
    enabled: true
    size: 50Gi
  resources:
    requests:
      cpu: 250m
      memory: 1Gi
    limits:
      cpu: 1000m
      memory: 2Gi

metrics:
  enabled: true
  serviceMonitor:
    enabled: true

volumePermissions:
  enabled: true
EOF

# Install PostgreSQL
helm install postgresql bitnami/postgresql \
  --namespace family-hub-data \
  --values postgres-values.yaml

# Wait for PostgreSQL to be ready
kubectl wait --namespace family-hub-data \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/name=postgresql \
  --timeout=300s

# Get PostgreSQL connection details
export POSTGRES_PASSWORD=$(kubectl get secret --namespace family-hub-data postgresql -o jsonpath="{.data.postgres-password}" | base64 -d)
echo "PostgreSQL password: $POSTGRES_PASSWORD"

# Test connection
kubectl run postgresql-client --rm --tty -i --restart='Never' \
  --namespace family-hub-data \
  --image docker.io/bitnami/postgresql:16 \
  --env="PGPASSWORD=$POSTGRES_PASSWORD" \
  --command -- psql --host postgresql.family-hub-data.svc.cluster.local -U postgres -d familyhub -p 5432
```

**Initialize Database Schemas:**

```bash
# Create initialization SQL script
cat > init-db.sql <<EOF
-- Create schemas for each service
CREATE SCHEMA IF NOT EXISTS auth;
CREATE SCHEMA IF NOT EXISTS calendar;
CREATE SCHEMA IF NOT EXISTS tasks;
CREATE SCHEMA IF NOT EXISTS shopping;
CREATE SCHEMA IF NOT EXISTS health;
CREATE SCHEMA IF NOT EXISTS meal_planning;
CREATE SCHEMA IF NOT EXISTS finance;
CREATE SCHEMA IF NOT EXISTS communication;

-- Create service users
CREATE USER auth_service WITH PASSWORD 'CHANGE_ME_AUTH_PASSWORD';
CREATE USER calendar_service WITH PASSWORD 'CHANGE_ME_CALENDAR_PASSWORD';
CREATE USER task_service WITH PASSWORD 'CHANGE_ME_TASK_PASSWORD';
CREATE USER shopping_service WITH PASSWORD 'CHANGE_ME_SHOPPING_PASSWORD';
CREATE USER health_service WITH PASSWORD 'CHANGE_ME_HEALTH_PASSWORD';
CREATE USER meal_planning_service WITH PASSWORD 'CHANGE_ME_MEAL_PLANNING_PASSWORD';
CREATE USER finance_service WITH PASSWORD 'CHANGE_ME_FINANCE_PASSWORD';
CREATE USER communication_service WITH PASSWORD 'CHANGE_ME_COMMUNICATION_PASSWORD';

-- Grant permissions
GRANT ALL PRIVILEGES ON SCHEMA auth TO auth_service;
GRANT ALL PRIVILEGES ON SCHEMA calendar TO calendar_service;
GRANT ALL PRIVILEGES ON SCHEMA tasks TO task_service;
GRANT ALL PRIVILEGES ON SCHEMA shopping TO shopping_service;
GRANT ALL PRIVILEGES ON SCHEMA health TO health_service;
GRANT ALL PRIVILEGES ON SCHEMA meal_planning TO meal_planning_service;
GRANT ALL PRIVILEGES ON SCHEMA finance TO finance_service;
GRANT ALL PRIVILEGES ON SCHEMA communication TO communication_service;

-- Enable Row-Level Security extension
CREATE EXTENSION IF NOT EXISTS pgcrypto;  -- For UUID generation
EOF

# Apply initialization script
kubectl exec -it postgresql-0 -n family-hub-data -- \
  psql -U postgres -d familyhub -f /tmp/init-db.sql
```

### 3.2 Deploy Redis

```bash
# Create Redis values file
cat > redis-values.yaml <<EOF
architecture: standalone  # Change to "replication" for HA

auth:
  enabled: true
  password: "CHANGE_ME_REDIS_PASSWORD"

master:
  persistence:
    enabled: true
    storageClass: ""  # Use default storage class
    size: 10Gi
  resources:
    requests:
      cpu: 200m
      memory: 512Mi
    limits:
      cpu: 500m
      memory: 2Gi

metrics:
  enabled: true
  serviceMonitor:
    enabled: true
EOF

# Install Redis
helm install redis bitnami/redis \
  --namespace family-hub-data \
  --values redis-values.yaml

# Wait for Redis to be ready
kubectl wait --namespace family-hub-data \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/name=redis \
  --timeout=300s

# Test connection
export REDIS_PASSWORD=$(kubectl get secret --namespace family-hub-data redis -o jsonpath="{.data.redis-password}" | base64 -d)

kubectl run redis-client --rm --tty -i --restart='Never' \
  --namespace family-hub-data \
  --env REDIS_PASSWORD=$REDIS_PASSWORD \
  --image docker.io/bitnami/redis:7.0 -- bash

# Inside the container:
redis-cli -h redis-master.family-hub-data.svc.cluster.local -a $REDIS_PASSWORD
# Test: SET test "Hello"; GET test; exit
```

### 3.3 Create Sealed Secrets for Database Credentials

```bash
# Create PostgreSQL secret (local)
kubectl create secret generic postgres-credentials \
  --namespace family-hub-data \
  --from-literal=postgres-password='YOUR_POSTGRES_PASSWORD' \
  --from-literal=auth-service-password='YOUR_AUTH_SERVICE_PASSWORD' \
  --from-literal=calendar-service-password='YOUR_CALENDAR_SERVICE_PASSWORD' \
  --from-literal=task-service-password='YOUR_TASK_SERVICE_PASSWORD' \
  --from-literal=shopping-service-password='YOUR_SHOPPING_SERVICE_PASSWORD' \
  --from-literal=health-service-password='YOUR_HEALTH_SERVICE_PASSWORD' \
  --from-literal=meal-planning-service-password='YOUR_MEAL_PLANNING_SERVICE_PASSWORD' \
  --from-literal=finance-service-password='YOUR_FINANCE_SERVICE_PASSWORD' \
  --from-literal=communication-service-password='YOUR_COMMUNICATION_SERVICE_PASSWORD' \
  --dry-run=client -o yaml > postgres-secret.yaml

# Seal the secret
kubeseal --format=yaml --cert=sealed-secrets-pub.pem < postgres-secret.yaml > postgres-sealed-secret.yaml

# Apply sealed secret
kubectl apply -f postgres-sealed-secret.yaml

# Create Redis secret
kubectl create secret generic redis-credentials \
  --namespace family-hub-data \
  --from-literal=redis-password='YOUR_REDIS_PASSWORD' \
  --dry-run=client -o yaml | \
kubeseal --format=yaml --cert=sealed-secrets-pub.pem > redis-sealed-secret.yaml

# Apply sealed secret
kubectl apply -f redis-sealed-secret.yaml

# Verify secrets
kubectl get secrets -n family-hub-data
```

---

## 4. Application Deployment

### 4.1 Install ArgoCD

```bash
# Install ArgoCD
kubectl create namespace argocd
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml

# Wait for ArgoCD to be ready
kubectl wait --namespace argocd \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/name=argocd-server \
  --timeout=300s

# Expose ArgoCD UI (Option 1: Port Forward)
kubectl port-forward svc/argocd-server -n argocd 8080:443 &

# Get initial admin password
ARGOCD_PASSWORD=$(kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d)
echo "ArgoCD admin password: $ARGOCD_PASSWORD"

# Login to ArgoCD CLI
argocd login localhost:8080 --username admin --password "$ARGOCD_PASSWORD" --insecure

# Change admin password
argocd account update-password

# Expose ArgoCD UI (Option 2: Ingress for Production)
cat <<EOF | kubectl apply -f -
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: argocd-server-ingress
  namespace: argocd
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
    nginx.ingress.kubernetes.io/ssl-passthrough: "true"
    nginx.ingress.kubernetes.io/backend-protocol: "HTTPS"
spec:
  ingressClassName: nginx
  tls:
  - hosts:
    - argocd.familyhub.yourdomain.com
    secretName: argocd-tls
  rules:
  - host: argocd.familyhub.yourdomain.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: argocd-server
            port:
              name: https
EOF
```

### 4.2 Configure ArgoCD with Git Repository

```bash
# Add Git repository to ArgoCD
argocd repo add https://github.com/yourorg/familyhub-k8s.git \
  --username your-github-username \
  --password your-github-token

# Or for private repositories with SSH:
argocd repo add git@github.com:yourorg/familyhub-k8s.git \
  --ssh-private-key-path ~/.ssh/id_rsa

# Verify repository
argocd repo list
```

### 4.3 Create ArgoCD Application for Family Hub

```bash
# Create ArgoCD application manifest
cat <<EOF | kubectl apply -f -
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: family-hub
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://github.com/yourorg/familyhub-k8s.git
    targetRevision: main
    path: manifests/production
    helm:
      valueFiles:
      - values.yaml
  destination:
    server: https://kubernetes.default.svc
    namespace: family-hub
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
EOF

# Sync application
argocd app sync family-hub

# Watch deployment progress
argocd app get family-hub --watch

# Or use kubectl
kubectl get pods -n family-hub --watch
```

### 4.4 Manual Deployment (Without ArgoCD)

If not using ArgoCD, deploy manually with Helm:

```bash
# Clone Helm charts repository
git clone https://github.com/yourorg/familyhub-helm-charts.git
cd familyhub-helm-charts

# Create values file for production
cat > values-production.yaml <<EOF
global:
  environment: production
  domain: familyhub.yourdomain.com

  postgresql:
    host: postgresql.family-hub-data.svc.cluster.local
    port: 5432
    database: familyhub

  redis:
    host: redis-master.family-hub-data.svc.cluster.local
    port: 6379

ingress:
  enabled: true
  className: nginx
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
  tls:
    enabled: true

# Service-specific configuration
auth-service:
  replicaCount: 2
  resources:
    requests:
      cpu: 100m
      memory: 256Mi
    limits:
      cpu: 500m
      memory: 512Mi

calendar-service:
  replicaCount: 2
  resources:
    requests:
      cpu: 150m
      memory: 256Mi
    limits:
      cpu: 500m
      memory: 512Mi

# ... other services
EOF

# Install Family Hub umbrella chart
helm install family-hub ./charts/family-hub \
  --namespace family-hub \
  --create-namespace \
  --values values-production.yaml

# Wait for all pods to be ready
kubectl wait --namespace family-hub \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/part-of=family-hub \
  --timeout=600s
```

---

## 5. Verification and Testing

### 5.1 Health Checks

```bash
# Check all pods are running
kubectl get pods -n family-hub
kubectl get pods -n family-hub-data

# Check pod health
kubectl describe pod <pod-name> -n family-hub

# Check logs
kubectl logs -f <pod-name> -n family-hub

# Check all logs for a service
kubectl logs -l app=calendar-service -n family-hub --all-containers=true --tail=100

# Check services
kubectl get svc -n family-hub

# Check ingress
kubectl get ingress -n family-hub
kubectl describe ingress family-hub-ingress -n family-hub
```

### 5.2 Smoke Tests

**Test Database Connectivity:**

```bash
# Test PostgreSQL connection from a pod
kubectl run psql-test --rm -it --restart=Never \
  --namespace family-hub \
  --image postgres:16 \
  --env="PGPASSWORD=YOUR_PASSWORD" \
  --command -- psql -h postgresql.family-hub-data.svc.cluster.local -U postgres -d familyhub -c "SELECT version();"

# Test Redis connection
kubectl run redis-test --rm -it --restart=Never \
  --namespace family-hub \
  --image redis:7 \
  --command -- redis-cli -h redis-master.family-hub-data.svc.cluster.local -a YOUR_REDIS_PASSWORD ping
```

**Test API Endpoints:**

```bash
# Get external IP of ingress
EXTERNAL_IP=$(kubectl get svc ingress-nginx-controller -n ingress-nginx -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
echo "External IP: $EXTERNAL_IP"

# Test API Gateway health endpoint
curl http://$EXTERNAL_IP/health \
  -H "Host: api.familyhub.yourdomain.com"

# Expected output: {"status":"healthy","timestamp":"2025-12-19T10:30:00Z"}

# Test GraphQL endpoint
curl http://$EXTERNAL_IP/graphql \
  -H "Host: api.familyhub.yourdomain.com" \
  -H "Content-Type: application/json" \
  -d '{"query":"{ __schema { queryType { name } } }"}'

# Expected output: {"data":{"__schema":{"queryType":{"name":"Query"}}}}
```

**Test TLS Certificates:**

```bash
# Check certificate status
kubectl get certificate -n family-hub

# Describe certificate
kubectl describe certificate familyhub-tls -n family-hub

# Test HTTPS endpoint
curl https://familyhub.yourdomain.com/health -v

# Expected: HTTP/2 200, certificate issued by Let's Encrypt
```

### 5.3 End-to-End Test

```bash
# Run end-to-end test suite (if available)
kubectl apply -f tests/e2e-test-job.yaml

# Watch test progress
kubectl logs -f job/e2e-tests -n family-hub

# Check test results
kubectl get job e2e-tests -n family-hub -o jsonpath='{.status.succeeded}'
# Expected: 1 (success)
```

---

## 6. Scaling Procedures

### 6.1 Manual Scaling

**Scale Deployment Replicas:**

```bash
# Scale calendar service to 5 replicas
kubectl scale deployment calendar-service --replicas=5 -n family-hub

# Verify scaling
kubectl get deployment calendar-service -n family-hub
kubectl get pods -l app=calendar-service -n family-hub
```

**Scale StatefulSet (Database):**

```bash
# Scale PostgreSQL read replicas
kubectl scale statefulset postgresql-read --replicas=3 -n family-hub-data

# Verify scaling
kubectl get statefulset -n family-hub-data
kubectl get pods -l app.kubernetes.io/component=read -n family-hub-data
```

### 6.2 Horizontal Pod Autoscaler (HPA)

**Create HPA for Services:**

```bash
# Calendar service HPA
cat <<EOF | kubectl apply -f -
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: calendar-service-hpa
  namespace: family-hub
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: calendar-service
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
  behavior:
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 50
        periodSeconds: 60
    scaleUp:
      stabilizationWindowSeconds: 0
      policies:
      - type: Percent
        value: 100
        periodSeconds: 15
EOF

# Verify HPA
kubectl get hpa -n family-hub
kubectl describe hpa calendar-service-hpa -n family-hub

# Watch HPA scaling
kubectl get hpa -n family-hub --watch
```

**Create HPA for All Services:**

```bash
# Apply HPA to all backend services
for SERVICE in auth calendar task shopping health meal-planning finance communication; do
  cat <<EOF | kubectl apply -f -
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: ${SERVICE}-service-hpa
  namespace: family-hub
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: ${SERVICE}-service
  minReplicas: 1
  maxReplicas: 5
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
EOF
done

# Verify all HPAs
kubectl get hpa -n family-hub
```

### 6.3 Cluster Autoscaling

**Enable Cluster Autoscaler (Cloud-Specific):**

**DigitalOcean:**

```bash
# Cluster autoscaling enabled during cluster creation
# Update existing cluster:
doctl kubernetes cluster update familyhub-prod --auto-upgrade=true

# Update node pool
doctl kubernetes cluster node-pool update familyhub-prod <pool-id> \
  --auto-scale --min-nodes 2 --max-nodes 10
```

**AWS EKS:**

```bash
# Deploy cluster autoscaler
kubectl apply -f https://raw.githubusercontent.com/kubernetes/autoscaler/master/cluster-autoscaler/cloudprovider/aws/examples/cluster-autoscaler-autodiscover.yaml

# Annotate deployment
kubectl -n kube-system annotate deployment.apps/cluster-autoscaler \
  cluster-autoscaler.kubernetes.io/safe-to-evict="false"

# Edit deployment to add cluster name
kubectl -n kube-system edit deployment.apps/cluster-autoscaler
# Add: --node-group-auto-discovery=asg:tag=k8s.io/cluster-autoscaler/enabled,k8s.io/cluster-autoscaler/familyhub-prod
# Add: --balance-similar-node-groups
# Add: --skip-nodes-with-system-pods=false
```

---

## 7. Disaster Recovery

### 7.1 Backup PostgreSQL

**Automated Backup CronJob:**

```bash
cat <<EOF | kubectl apply -f -
apiVersion: batch/v1
kind: CronJob
metadata:
  name: postgres-backup
  namespace: family-hub-data
spec:
  schedule: "0 2 * * *"  # Daily at 2 AM UTC
  successfulJobsHistoryLimit: 7
  failedJobsHistoryLimit: 3
  jobTemplate:
    spec:
      template:
        spec:
          restartPolicy: OnFailure
          containers:
          - name: backup
            image: postgres:16-alpine
            env:
            - name: PGHOST
              value: postgresql.family-hub-data.svc.cluster.local
            - name: PGUSER
              value: postgres
            - name: PGPASSWORD
              valueFrom:
                secretKeyRef:
                  name: postgres-credentials
                  key: postgres-password
            - name: PGDATABASE
              value: familyhub
            - name: AWS_ACCESS_KEY_ID
              valueFrom:
                secretKeyRef:
                  name: backup-credentials
                  key: access-key-id
            - name: AWS_SECRET_ACCESS_KEY
              valueFrom:
                secretKeyRef:
                  name: backup-credentials
                  key: secret-access-key
            - name: S3_BUCKET
              value: familyhub-backups
            - name: S3_ENDPOINT
              value: https://s3.amazonaws.com  # Or Backblaze, Minio, etc.
            command:
            - /bin/sh
            - -c
            - |
              set -e
              BACKUP_FILE="/tmp/backup-$(date +%Y%m%d-%H%M%S).dump"
              echo "Starting backup to $BACKUP_FILE"

              # Create backup
              pg_dump -Fc -f "$BACKUP_FILE"

              # Upload to S3
              apk add --no-cache aws-cli
              aws s3 cp "$BACKUP_FILE" "s3://$S3_BUCKET/postgres/" --endpoint-url "$S3_ENDPOINT"

              echo "Backup completed successfully"

              # Cleanup old backups (keep last 30 days)
              aws s3 ls "s3://$S3_BUCKET/postgres/" --endpoint-url "$S3_ENDPOINT" | \
                awk '{print $4}' | \
                head -n -30 | \
                xargs -I {} aws s3 rm "s3://$S3_BUCKET/postgres/{}" --endpoint-url "$S3_ENDPOINT"
EOF

# Create backup credentials secret
kubectl create secret generic backup-credentials \
  --namespace family-hub-data \
  --from-literal=access-key-id='YOUR_ACCESS_KEY' \
  --from-literal=secret-access-key='YOUR_SECRET_KEY' \
  --dry-run=client -o yaml | \
kubeseal --format=yaml > backup-credentials-sealed.yaml

kubectl apply -f backup-credentials-sealed.yaml

# Trigger manual backup
kubectl create job --from=cronjob/postgres-backup postgres-backup-manual -n family-hub-data

# Check backup status
kubectl logs -f job/postgres-backup-manual -n family-hub-data
```

### 7.2 Restore PostgreSQL from Backup

```bash
# 1. List available backups
aws s3 ls s3://familyhub-backups/postgres/

# 2. Download backup
aws s3 cp s3://familyhub-backups/postgres/backup-20251219-020000.dump /tmp/restore.dump

# 3. Scale down all services (to prevent write conflicts)
kubectl scale deployment --all --replicas=0 -n family-hub

# 4. Drop and recreate database (DANGER!)
kubectl exec -it postgresql-0 -n family-hub-data -- psql -U postgres -c "DROP DATABASE familyhub;"
kubectl exec -it postgresql-0 -n family-hub-data -- psql -U postgres -c "CREATE DATABASE familyhub;"

# 5. Restore data
kubectl cp /tmp/restore.dump family-hub-data/postgresql-0:/tmp/restore.dump
kubectl exec -it postgresql-0 -n family-hub-data -- pg_restore -U postgres -d familyhub -v /tmp/restore.dump

# 6. Verify data
kubectl exec -it postgresql-0 -n family-hub-data -- psql -U postgres -d familyhub -c "SELECT COUNT(*) FROM calendar.events;"

# 7. Scale services back up
kubectl scale deployment --all --replicas=2 -n family-hub

# 8. Verify application health
kubectl get pods -n family-hub
curl https://familyhub.yourdomain.com/health
```

### 7.3 Complete Cluster Disaster Recovery

**Scenario: Complete cluster failure, restore from backups.**

```bash
# 1. Provision new Kubernetes cluster (see Section 1.2)

# 2. Install core infrastructure (see Section 2)
# - Namespaces
# - NGINX Ingress
# - Cert-Manager
# - Sealed Secrets

# 3. Deploy PostgreSQL and Redis (see Section 3)

# 4. Restore PostgreSQL database (see Section 7.2)

# 5. Deploy applications with ArgoCD (see Section 4)
# ArgoCD will sync from Git and deploy all services

# 6. Update DNS to point to new cluster
# Get new Load Balancer IP
EXTERNAL_IP=$(kubectl get svc ingress-nginx-controller -n ingress-nginx -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
echo "Update DNS A record for familyhub.yourdomain.com to $EXTERNAL_IP"

# 7. Wait for DNS propagation (5-60 minutes)
# Test: dig familyhub.yourdomain.com

# 8. Verify all services are healthy
kubectl get pods --all-namespaces
curl https://familyhub.yourdomain.com/health

# 9. Run end-to-end tests
kubectl apply -f tests/e2e-test-job.yaml
kubectl logs -f job/e2e-tests -n family-hub
```

**Recovery Time Objective (RTO): 2-4 hours**

- Cluster provisioning: 30 minutes
- Infrastructure setup: 30 minutes
- Database restore: 30-60 minutes (depends on size)
- Application deployment: 15 minutes
- DNS propagation: 30-60 minutes
- Testing and verification: 15 minutes

**Recovery Point Objective (RPO): <24 hours (daily backups)**

---

## 8. Troubleshooting Guide

### 8.1 Common Issues

**Issue: Pods stuck in Pending state**

```bash
# Check pod events
kubectl describe pod <pod-name> -n family-hub

# Common causes and solutions:

# 1. Insufficient resources
kubectl describe nodes | grep -A 5 "Allocated resources"
# Solution: Scale cluster or reduce resource requests

# 2. PVC not bound
kubectl get pvc -n family-hub-data
# Solution: Check storage class, provisioner logs

# 3. Image pull error
kubectl get events -n family-hub | grep "Failed to pull image"
# Solution: Check image name, registry credentials
```

**Issue: CrashLoopBackOff**

```bash
# Check pod logs
kubectl logs <pod-name> -n family-hub --previous

# Check current logs
kubectl logs -f <pod-name> -n family-hub

# Common causes:
# 1. Database connection failure
#    - Verify PostgreSQL is running
#    - Check connection string in ConfigMap/Secret
#    - Test connection: kubectl exec -it <pod-name> -- curl postgres-host:5432

# 2. Missing environment variables
#    - Check ConfigMap: kubectl get configmap <name> -o yaml
#    - Check Secret: kubectl get secret <name> -o yaml

# 3. Application bug
#    - Check application logs for stack traces
#    - Verify configuration is correct
```

**Issue: Service Unavailable (503 errors)**

```bash
# Check ingress
kubectl get ingress -n family-hub
kubectl describe ingress family-hub-ingress -n family-hub

# Check service endpoints
kubectl get endpoints <service-name> -n family-hub
# If empty, pods are not ready

# Check pod readiness
kubectl get pods -n family-hub
# Look for "0/1" or "0/2" ready status

# Check readiness probe
kubectl describe pod <pod-name> -n family-hub | grep -A 10 Readiness

# Test service directly
kubectl port-forward svc/<service-name> 8080:80 -n family-hub
curl http://localhost:8080/health
```

**Issue: High CPU/Memory Usage**

```bash
# Check resource usage
kubectl top pods -n family-hub
kubectl top nodes

# Check for resource limits
kubectl describe pod <pod-name> -n family-hub | grep -A 5 Limits

# Check for memory leaks
kubectl exec -it <pod-name> -n family-hub -- top

# If memory leak detected:
# 1. Restart pod: kubectl delete pod <pod-name> -n family-hub
# 2. Investigate application code
# 3. Increase memory limits temporarily
```

**Issue: Database connection pool exhausted**

```bash
# Check PostgreSQL connections
kubectl exec -it postgresql-0 -n family-hub-data -- psql -U postgres -c "SELECT count(*) FROM pg_stat_activity;"

# Check max connections
kubectl exec -it postgresql-0 -n family-hub-data -- psql -U postgres -c "SHOW max_connections;"

# Kill idle connections
kubectl exec -it postgresql-0 -n family-hub-data -- psql -U postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE state = 'idle' AND state_change < now() - interval '5 minutes';"

# Increase max_connections (requires restart)
kubectl exec -it postgresql-0 -n family-hub-data -- psql -U postgres -c "ALTER SYSTEM SET max_connections = 200;"
kubectl rollout restart statefulset/postgresql -n family-hub-data
```

### 8.2 Debugging Commands

```bash
# Interactive shell in pod
kubectl exec -it <pod-name> -n family-hub -- /bin/sh

# Run command in pod
kubectl exec <pod-name> -n family-hub -- env

# Copy files from/to pod
kubectl cp <pod-name>:/path/to/file /local/path -n family-hub
kubectl cp /local/path <pod-name>:/path/to/file -n family-hub

# Port forward to service
kubectl port-forward svc/<service-name> 8080:80 -n family-hub

# Watch pod status
kubectl get pods -n family-hub --watch

# Get all events
kubectl get events -n family-hub --sort-by='.lastTimestamp'

# Check resource quotas
kubectl describe resourcequota -n family-hub

# Check network policies
kubectl get networkpolicy -n family-hub
kubectl describe networkpolicy <policy-name> -n family-hub
```

### 8.3 Performance Troubleshooting

**Slow API Responses:**

```bash
# Check Prometheus metrics
kubectl port-forward -n monitoring svc/prometheus-server 9090:80
# Open http://localhost:9090
# Query: histogram_quantile(0.95, sum(rate(http_request_duration_seconds_bucket[5m])) by (le, service))

# Check database query performance
kubectl exec -it postgresql-0 -n family-hub-data -- psql -U postgres -d familyhub -c "SELECT query, calls, mean_exec_time, max_exec_time FROM pg_stat_statements ORDER BY mean_exec_time DESC LIMIT 10;"

# Enable slow query log
kubectl exec -it postgresql-0 -n family-hub-data -- psql -U postgres -c "ALTER SYSTEM SET log_min_duration_statement = 1000;"  # Log queries >1s
kubectl rollout restart statefulset/postgresql -n family-hub-data

# Check slow queries
kubectl logs postgresql-0 -n family-hub-data | grep "duration:"
```

**Database Connection Issues:**

```bash
# Test connection from pod
kubectl run psql-debug --rm -it --restart=Never \
  --image postgres:16 \
  --env="PGPASSWORD=YOUR_PASSWORD" \
  --command -- psql -h postgresql.family-hub-data.svc.cluster.local -U postgres -d familyhub

# Check DNS resolution
kubectl run busybox --rm -it --restart=Never --image=busybox -- nslookup postgresql.family-hub-data.svc.cluster.local

# Check network connectivity
kubectl run busybox --rm -it --restart=Never --image=busybox -- telnet postgresql.family-hub-data.svc.cluster.local 5432
```

### 8.4 Getting Help

**Gather Debug Information:**

```bash
# Create debug bundle
mkdir -p debug-bundle
kubectl get all -n family-hub -o yaml > debug-bundle/all-resources.yaml
kubectl get events -n family-hub --sort-by='.lastTimestamp' > debug-bundle/events.txt
kubectl logs -l app.kubernetes.io/part-of=family-hub -n family-hub --all-containers=true --tail=1000 > debug-bundle/logs.txt
kubectl describe pods -n family-hub > debug-bundle/pod-descriptions.txt
kubectl top pods -n family-hub > debug-bundle/resource-usage.txt
kubectl get pvc -n family-hub-data > debug-bundle/pvc.txt

# Compress and share
tar -czf debug-bundle.tar.gz debug-bundle/
```

**Contact Support:**

- GitHub Issues: <https://github.com/yourorg/family-hub/issues>
- Slack/Discord: Link your community channel
- Email: <support@familyhub.yourdomain.com>

---

## 9. Operations Runbook

### 9.1 Daily Operations

**Morning Health Check:**

```bash
#!/bin/bash
# daily-health-check.sh

echo "=== Family Hub Daily Health Check ==="
echo "Date: $(date)"
echo ""

echo "1. Checking cluster nodes..."
kubectl get nodes
echo ""

echo "2. Checking application pods..."
kubectl get pods -n family-hub
echo ""

echo "3. Checking database pods..."
kubectl get pods -n family-hub-data
echo ""

echo "4. Checking HPA status..."
kubectl get hpa -n family-hub
echo ""

echo "5. Checking certificate expiry..."
kubectl get certificate -n family-hub
echo ""

echo "6. Checking recent errors in logs..."
kubectl logs -l app.kubernetes.io/part-of=family-hub -n family-hub --since=24h | grep -i "error\|exception" | tail -20
echo ""

echo "7. Testing API endpoint..."
curl -s https://familyhub.yourdomain.com/health
echo ""

echo "=== Health Check Complete ==="
```

### 9.2 Weekly Operations

- Review Prometheus alerts
- Check backup logs
- Review resource usage trends
- Update security patches (if available)
- Review HPA metrics, adjust thresholds if needed

### 9.3 Monthly Operations

- Review and rotate logs
- Update Kubernetes cluster version (minor versions)
- Security audit
- Cost optimization review
- Disaster recovery drill
- Database performance tuning

---

## 10. Maintenance Windows

### 10.1 Planned Maintenance Procedure

**Pre-Maintenance:**

```bash
# 1. Notify users (24-48 hours in advance)
# 2. Create pre-maintenance backup
kubectl create job --from=cronjob/postgres-backup postgres-backup-pre-maintenance -n family-hub-data

# 3. Scale up replicas for redundancy
kubectl scale deployment --all --replicas=3 -n family-hub
```

**During Maintenance:**

```bash
# 1. Set maintenance mode (if supported)
kubectl set env deployment -n family-hub --all MAINTENANCE_MODE=true

# 2. Perform maintenance (e.g., upgrade)
helm upgrade family-hub ./charts/family-hub --values values-production.yaml

# 3. Rolling restart if needed
kubectl rollout restart deployment -n family-hub

# 4. Monitor rollout
kubectl rollout status deployment -n family-hub
```

**Post-Maintenance:**

```bash
# 1. Disable maintenance mode
kubectl set env deployment -n family-hub --all MAINTENANCE_MODE-

# 2. Run smoke tests
curl https://familyhub.yourdomain.com/health

# 3. Monitor logs for errors
kubectl logs -l app.kubernetes.io/part-of=family-hub -n family-hub --tail=100

# 4. Scale back to normal
kubectl scale deployment --all --replicas=2 -n family-hub

# 5. Notify users (maintenance complete)
```

---

## Appendix A: Environment Variables Reference

**Common Environment Variables:**

```yaml
# Database
DATABASE_HOST: postgresql.family-hub-data.svc.cluster.local
DATABASE_PORT: 5432
DATABASE_NAME: familyhub
DATABASE_USER: <service>_service
DATABASE_PASSWORD: <from-secret>
DATABASE_SSL_MODE: require

# Redis
REDIS_HOST: redis-master.family-hub-data.svc.cluster.local
REDIS_PORT: 6379
REDIS_PASSWORD: <from-secret>

# Application
ASPNETCORE_ENVIRONMENT: Production
ASPNETCORE_URLS: http://+:5000
LOG_LEVEL: Information

# Authentication
AUTH_SERVICE_URL: http://auth-service.family-hub.svc.cluster.local:5001
ZITADEL_URL: https://your-zitadel-instance.com
ZITADEL_CLIENT_ID: <from-secret>
ZITADEL_CLIENT_SECRET: <from-secret>

# Event Bus
EVENT_BUS_TYPE: Redis
EVENT_BUS_CONNECTION: redis-master.family-hub-data.svc.cluster.local:6379

# Monitoring
OTEL_EXPORTER_OTLP_ENDPOINT: http://otel-collector.monitoring.svc.cluster.local:4317
```

---

## Appendix B: Useful kubectl Aliases

```bash
# Add to ~/.bashrc or ~/.zshrc

# Namespaces
alias kns='kubens'
alias kgns='kubectl get namespaces'

# Pods
alias kgp='kubectl get pods'
alias kdp='kubectl describe pod'
alias klf='kubectl logs -f'

# Deployments
alias kgd='kubectl get deployments'
alias kdd='kubectl describe deployment'
alias krd='kubectl rollout restart deployment'

# Services
alias kgs='kubectl get services'
alias kds='kubectl describe service'

# All resources
alias kga='kubectl get all'

# Quick access to Family Hub namespace
alias kf='kubectl -n family-hub'
alias kfd='kubectl -n family-hub-data'
alias kfm='kubectl -n monitoring'

# Examples:
# kf get pods
# kfd logs postgresql-0
# kfm port-forward svc/grafana 3000:80
```

---

**Document Status:** Production Ready
**Last Updated:** 2025-12-19
**Next Review:** After Phase 2 deployment

**Maintained By:** DevOps Team
