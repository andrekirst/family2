# Helm Charts Structure - Family Hub

**Version:** 1.0
**Date:** 2025-12-19
**Status:** Implementation Ready
**Author:** Platform Engineer (Claude Code)

---

## Executive Summary

This document defines the Helm chart structure for Family Hub, including the umbrella chart design, per-service charts, and configuration management strategy. The design supports cloud-agnostic deployment, environment-specific overrides, and GitOps workflows.

---

## Table of Contents

1. [Directory Structure](#1-directory-structure)
2. [Umbrella Chart Design](#2-umbrella-chart-design)
3. [Service Chart Templates](#3-service-chart-templates)
4. [Values Configuration](#4-values-configuration)
5. [Configuration Management](#5-configuration-management)

---

## 1. Directory Structure

```
family-hub-helm/
├── Chart.yaml                      # Umbrella chart metadata
├── values.yaml                     # Global default values
├── values-dev.yaml                 # Development overrides
├── values-staging.yaml             # Staging overrides
├── values-production.yaml          # Production overrides
├── templates/                      # Umbrella chart templates
│   ├── namespace.yaml
│   ├── network-policies.yaml
│   └── _helpers.tpl
├── charts/                         # Subcharts (dependencies)
│   ├── auth-service/
│   │   ├── Chart.yaml
│   │   ├── values.yaml
│   │   └── templates/
│   │       ├── deployment.yaml
│   │       ├── service.yaml
│   │       ├── configmap.yaml
│   │       ├── secret.yaml (sealed)
│   │       ├── hpa.yaml
│   │       ├── servicemonitor.yaml
│   │       └── _helpers.tpl
│   ├── calendar-service/
│   ├── task-service/
│   ├── shopping-service/
│   ├── health-service/
│   ├── meal-planning-service/
│   ├── finance-service/
│   ├── communication-service/
│   ├── api-gateway/
│   └── frontend/
├── infrastructure/                 # Infrastructure charts (optional)
│   ├── postgresql/
│   ├── redis/
│   ├── ingress-nginx/
│   └── cert-manager/
└── docs/
    ├── README.md
    └── deployment-guide.md
```

---

## 2. Umbrella Chart Design

### 2.1 Umbrella Chart.yaml

```yaml
# Chart.yaml
apiVersion: v2
name: family-hub
description: Family Hub - Privacy-first family organization platform
type: application
version: 1.0.0
appVersion: "1.0.0"
keywords:
  - family
  - organization
  - privacy
  - microservices
home: https://github.com/yourorg/family-hub
sources:
  - https://github.com/yourorg/family-hub
maintainers:
  - name: Family Hub Team
    email: team@familyhub.com

dependencies:
  - name: auth-service
    version: "1.x.x"
    repository: "file://./charts/auth-service"
    condition: auth-service.enabled
  - name: calendar-service
    version: "1.x.x"
    repository: "file://./charts/calendar-service"
    condition: calendar-service.enabled
  - name: task-service
    version: "1.x.x"
    repository: "file://./charts/task-service"
    condition: task-service.enabled
  - name: shopping-service
    version: "1.x.x"
    repository: "file://./charts/shopping-service"
    condition: shopping-service.enabled
  - name: health-service
    version: "1.x.x"
    repository: "file://./charts/health-service"
    condition: health-service.enabled
  - name: meal-planning-service
    version: "1.x.x"
    repository: "file://./charts/meal-planning-service"
    condition: meal-planning-service.enabled
  - name: finance-service
    version: "1.x.x"
    repository: "file://./charts/finance-service"
    condition: finance-service.enabled
  - name: communication-service
    version: "1.x.x"
    repository: "file://./charts/communication-service"
    condition: communication-service.enabled
  - name: api-gateway
    version: "1.x.x"
    repository: "file://./charts/api-gateway"
    condition: api-gateway.enabled
  - name: frontend
    version: "1.x.x"
    repository: "file://./charts/frontend"
    condition: frontend.enabled
```

### 2.2 Global Values (values.yaml)

```yaml
# values.yaml - Global defaults

global:
  # Environment: dev, staging, production
  environment: production

  # Domain configuration
  domain: familyhub.yourdomain.com
  apiDomain: api.familyhub.yourdomain.com

  # Image registry
  imageRegistry: docker.io/familyhub
  imagePullPolicy: IfNotPresent
  imagePullSecrets: []

  # PostgreSQL configuration
  postgresql:
    enabled: true
    host: postgresql.family-hub-data.svc.cluster.local
    port: 5432
    database: familyhub
    sslMode: require
    # Credentials from secrets
    existingSecret: postgres-credentials

  # Redis configuration
  redis:
    enabled: true
    host: redis-master.family-hub-data.svc.cluster.local
    port: 6379
    # Password from secret
    existingSecret: redis-credentials

  # Authentication (Zitadel)
  auth:
    zitadelUrl: https://your-zitadel-instance.com
    existingSecret: zitadel-credentials

  # Monitoring
  monitoring:
    enabled: true
    prometheus:
      enabled: true
    grafana:
      enabled: true

  # Security
  podSecurityStandards:
    enforce: restricted
  networkPolicies:
    enabled: true

# Ingress configuration
ingress:
  enabled: true
  className: nginx
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
    nginx.ingress.kubernetes.io/rate-limit: "100"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
  tls:
    enabled: true
    secretName: familyhub-tls

# Service defaults (inherited by all services)
serviceDefaults:
  replicaCount: 2
  resources:
    requests:
      cpu: 100m
      memory: 128Mi
    limits:
      cpu: 500m
      memory: 512Mi
  autoscaling:
    enabled: true
    minReplicas: 1
    maxReplicas: 5
    targetCPUUtilizationPercentage: 70
    targetMemoryUtilizationPercentage: 80

  # Health checks
  livenessProbe:
    httpGet:
      path: /health/live
      port: http
    initialDelaySeconds: 30
    periodSeconds: 10
    timeoutSeconds: 5
    failureThreshold: 3

  readinessProbe:
    httpGet:
      path: /health/ready
      port: http
    initialDelaySeconds: 10
    periodSeconds: 5
    timeoutSeconds: 3
    failureThreshold: 3

# Individual service overrides
auth-service:
  enabled: true
  replicaCount: 2
  image:
    repository: familyhub/auth-service
    tag: "1.0.0"
  service:
    port: 5001

calendar-service:
  enabled: true
  replicaCount: 2
  image:
    repository: familyhub/calendar-service
    tag: "1.0.0"
  service:
    port: 5002

task-service:
  enabled: true
  replicaCount: 2
  image:
    repository: familyhub/task-service
    tag: "1.0.0"
  service:
    port: 5003

shopping-service:
  enabled: true
  replicaCount: 1
  image:
    repository: familyhub/shopping-service
    tag: "1.0.0"
  service:
    port: 5004

health-service:
  enabled: true
  replicaCount: 1
  image:
    repository: familyhub/health-service
    tag: "1.0.0"
  service:
    port: 5005

meal-planning-service:
  enabled: true
  replicaCount: 1
  image:
    repository: familyhub/meal-planning-service
    tag: "1.0.0"
  service:
    port: 5007

finance-service:
  enabled: true
  replicaCount: 1
  image:
    repository: familyhub/finance-service
    tag: "1.0.0"
  service:
    port: 5006

communication-service:
  enabled: true
  replicaCount: 1
  image:
    repository: familyhub/communication-service
    tag: "1.0.0"
  service:
    port: 5008

api-gateway:
  enabled: true
  replicaCount: 2
  image:
    repository: familyhub/api-gateway
    tag: "1.0.0"
  service:
    port: 8080

frontend:
  enabled: true
  replicaCount: 2
  image:
    repository: familyhub/frontend
    tag: "1.0.0"
  service:
    port: 80
```

---

## 3. Service Chart Templates

### 3.1 Service Chart.yaml Template

```yaml
# charts/calendar-service/Chart.yaml
apiVersion: v2
name: calendar-service
description: Calendar Service for Family Hub
type: application
version: 1.0.0
appVersion: "1.0.0"
```

### 3.2 Deployment Template

```yaml
# charts/calendar-service/templates/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ include "calendar-service.fullname" . }}
  namespace: {{ .Release.Namespace }}
  labels:
    {{- include "calendar-service.labels" . | nindent 4 }}
spec:
  {{- if not .Values.autoscaling.enabled }}
  replicas: {{ .Values.replicaCount }}
  {{- end }}
  selector:
    matchLabels:
      {{- include "calendar-service.selectorLabels" . | nindent 6 }}
  template:
    metadata:
      annotations:
        checksum/config: {{ include (print $.Template.BasePath "/configmap.yaml") . | sha256sum }}
        prometheus.io/scrape: "true"
        prometheus.io/port: "{{ .Values.service.port }}"
        prometheus.io/path: "/metrics"
      labels:
        {{- include "calendar-service.selectorLabels" . | nindent 8 }}
    spec:
      {{- with .Values.imagePullSecrets }}
      imagePullSecrets:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      serviceAccountName: {{ include "calendar-service.serviceAccountName" . }}
      securityContext:
        runAsNonRoot: true
        runAsUser: 1000
        fsGroup: 1000
        seccompProfile:
          type: RuntimeDefault
      containers:
      - name: calendar
        image: "{{ .Values.global.imageRegistry }}/{{ .Values.image.repository }}:{{ .Values.image.tag | default .Chart.AppVersion }}"
        imagePullPolicy: {{ .Values.global.imagePullPolicy }}
        securityContext:
          allowPrivilegeEscalation: false
          capabilities:
            drop:
            - ALL
          readOnlyRootFilesystem: true
        ports:
        - name: http
          containerPort: {{ .Values.service.port }}
          protocol: TCP
        livenessProbe:
          {{- toYaml .Values.livenessProbe | nindent 10 }}
        readinessProbe:
          {{- toYaml .Values.readinessProbe | nindent 10 }}
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: {{ .Values.global.environment | title }}
        - name: ASPNETCORE_URLS
          value: "http://+:{{ .Values.service.port }}"
        - name: DATABASE_HOST
          value: {{ .Values.global.postgresql.host }}
        - name: DATABASE_PORT
          value: "{{ .Values.global.postgresql.port }}"
        - name: DATABASE_NAME
          value: {{ .Values.global.postgresql.database }}
        - name: DATABASE_USER
          value: calendar_service
        - name: DATABASE_PASSWORD
          valueFrom:
            secretKeyRef:
              name: {{ .Values.global.postgresql.existingSecret }}
              key: calendar-service-password
        - name: REDIS_HOST
          value: {{ .Values.global.redis.host }}
        - name: REDIS_PORT
          value: "{{ .Values.global.redis.port }}"
        - name: REDIS_PASSWORD
          valueFrom:
            secretKeyRef:
              name: {{ .Values.global.redis.existingSecret }}
              key: redis-password
        - name: EVENT_BUS_TYPE
          value: Redis
        - name: EVENT_BUS_CONNECTION
          value: "$(REDIS_HOST):$(REDIS_PORT),password=$(REDIS_PASSWORD)"
        - name: OTEL_EXPORTER_OTLP_ENDPOINT
          value: "http://otel-collector.monitoring.svc.cluster.local:4317"
        - name: OTEL_SERVICE_NAME
          value: calendar-service
        envFrom:
        - configMapRef:
            name: {{ include "calendar-service.fullname" . }}
        resources:
          {{- toYaml .Values.resources | nindent 10 }}
        volumeMounts:
        - name: tmp
          mountPath: /tmp
      volumes:
      - name: tmp
        emptyDir: {}
      {{- with .Values.nodeSelector }}
      nodeSelector:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      {{- with .Values.affinity }}
      affinity:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      {{- with .Values.tolerations }}
      tolerations:
        {{- toYaml . | nindent 8 }}
      {{- end }}
```

### 3.3 Service Template

```yaml
# charts/calendar-service/templates/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: {{ include "calendar-service.fullname" . }}
  namespace: {{ .Release.Namespace }}
  labels:
    {{- include "calendar-service.labels" . | nindent 4 }}
spec:
  type: ClusterIP
  ports:
  - port: {{ .Values.service.port }}
    targetPort: http
    protocol: TCP
    name: http
  selector:
    {{- include "calendar-service.selectorLabels" . | nindent 4 }}
```

### 3.4 ConfigMap Template

```yaml
# charts/calendar-service/templates/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: {{ include "calendar-service.fullname" . }}
  namespace: {{ .Release.Namespace }}
  labels:
    {{- include "calendar-service.labels" . | nindent 4 }}
data:
  LOG_LEVEL: {{ .Values.logLevel | default "Information" }}
  ENABLE_SWAGGER: {{ .Values.enableSwagger | default "false" | quote }}
  AUTH_SERVICE_URL: "http://auth-service.{{ .Release.Namespace }}.svc.cluster.local:5001"
  TASK_SERVICE_URL: "http://task-service.{{ .Release.Namespace }}.svc.cluster.local:5003"
  COMMUNICATION_SERVICE_URL: "http://communication-service.{{ .Release.Namespace }}.svc.cluster.local:5008"
```

### 3.5 HPA Template

```yaml
# charts/calendar-service/templates/hpa.yaml
{{- if .Values.autoscaling.enabled }}
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: {{ include "calendar-service.fullname" . }}
  namespace: {{ .Release.Namespace }}
  labels:
    {{- include "calendar-service.labels" . | nindent 4 }}
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: {{ include "calendar-service.fullname" . }}
  minReplicas: {{ .Values.autoscaling.minReplicas }}
  maxReplicas: {{ .Values.autoscaling.maxReplicas }}
  metrics:
  {{- if .Values.autoscaling.targetCPUUtilizationPercentage }}
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: {{ .Values.autoscaling.targetCPUUtilizationPercentage }}
  {{- end }}
  {{- if .Values.autoscaling.targetMemoryUtilizationPercentage }}
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: {{ .Values.autoscaling.targetMemoryUtilizationPercentage }}
  {{- end }}
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
{{- end }}
```

### 3.6 ServiceMonitor Template (Prometheus)

```yaml
# charts/calendar-service/templates/servicemonitor.yaml
{{- if .Values.global.monitoring.enabled }}
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: {{ include "calendar-service.fullname" . }}
  namespace: {{ .Release.Namespace }}
  labels:
    {{- include "calendar-service.labels" . | nindent 4 }}
spec:
  selector:
    matchLabels:
      {{- include "calendar-service.selectorLabels" . | nindent 6 }}
  endpoints:
  - port: http
    path: /metrics
    interval: 30s
    scrapeTimeout: 10s
{{- end }}
```

### 3.7 Helpers Template

```yaml
# charts/calendar-service/templates/_helpers.tpl
{{/*
Expand the name of the chart.
*/}}
{{- define "calendar-service.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "calendar-service.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "calendar-service.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "calendar-service.labels" -}}
helm.sh/chart: {{ include "calendar-service.chart" . }}
{{ include "calendar-service.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/part-of: family-hub
{{- end }}

{{/*
Selector labels
*/}}
{{- define "calendar-service.selectorLabels" -}}
app.kubernetes.io/name: {{ include "calendar-service.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "calendar-service.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "calendar-service.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}
```

---

## 4. Values Configuration

### 4.1 Environment-Specific Values

**values-dev.yaml:**
```yaml
# Development overrides
global:
  environment: dev
  domain: dev.familyhub.local
  apiDomain: api.dev.familyhub.local
  imagePullPolicy: Always

  postgresql:
    host: postgresql.family-hub-data.svc.cluster.local

  monitoring:
    enabled: false

ingress:
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-staging

serviceDefaults:
  replicaCount: 1
  resources:
    requests:
      cpu: 50m
      memory: 64Mi
    limits:
      cpu: 200m
      memory: 256Mi
  autoscaling:
    enabled: false

# Enable debug features
calendar-service:
  replicaCount: 1
  logLevel: Debug
  enableSwagger: true

task-service:
  replicaCount: 1
  logLevel: Debug
  enableSwagger: true
```

**values-staging.yaml:**
```yaml
# Staging overrides
global:
  environment: staging
  domain: staging.familyhub.yourdomain.com
  apiDomain: api.staging.familyhub.yourdomain.com

serviceDefaults:
  replicaCount: 2
  resources:
    requests:
      cpu: 100m
      memory: 128Mi
    limits:
      cpu: 500m
      memory: 512Mi
  autoscaling:
    enabled: true
    minReplicas: 1
    maxReplicas: 3
```

**values-production.yaml:**
```yaml
# Production overrides
global:
  environment: production
  domain: familyhub.yourdomain.com
  apiDomain: api.familyhub.yourdomain.com

  postgresql:
    host: postgresql.family-hub-data.svc.cluster.local

  monitoring:
    enabled: true

serviceDefaults:
  replicaCount: 3
  resources:
    requests:
      cpu: 150m
      memory: 256Mi
    limits:
      cpu: 1000m
      memory: 1Gi
  autoscaling:
    enabled: true
    minReplicas: 2
    maxReplicas: 10

# Critical services get more resources
calendar-service:
  replicaCount: 3
  resources:
    requests:
      cpu: 200m
      memory: 512Mi
    limits:
      cpu: 1000m
      memory: 1Gi

task-service:
  replicaCount: 3
  resources:
    requests:
      cpu: 200m
      memory: 512Mi
    limits:
      cpu: 1000m
      memory: 1Gi

api-gateway:
  replicaCount: 3
  resources:
    requests:
      cpu: 300m
      memory: 512Mi
    limits:
      cpu: 1500m
      memory: 1Gi
```

---

## 5. Configuration Management

### 5.1 Deployment Commands

**Development:**
```bash
helm install family-hub ./family-hub-helm \
  --namespace family-hub \
  --create-namespace \
  --values values-dev.yaml
```

**Staging:**
```bash
helm upgrade --install family-hub ./family-hub-helm \
  --namespace family-hub \
  --create-namespace \
  --values values-staging.yaml
```

**Production:**
```bash
helm upgrade --install family-hub ./family-hub-helm \
  --namespace family-hub \
  --create-namespace \
  --values values-production.yaml \
  --atomic \
  --timeout 10m
```

### 5.2 Secret Management

**Using Sealed Secrets:**
```bash
# Create secret locally
kubectl create secret generic calendar-service-db \
  --from-literal=password='super-secret' \
  --dry-run=client -o yaml > calendar-db-secret.yaml

# Seal the secret
kubeseal --format=yaml < calendar-db-secret.yaml > calendar-db-sealed-secret.yaml

# Store sealed secret in Git
git add calendar-db-sealed-secret.yaml
git commit -m "Add calendar database credentials (sealed)"
git push

# Reference in values.yaml:
# global:
#   postgresql:
#     existingSecret: calendar-service-db
```

### 5.3 ArgoCD Integration

**Application Manifest:**
```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: family-hub
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
      prune: true
      selfHeal: true
    syncOptions:
    - CreateNamespace=true
```

### 5.4 Version Management

**Helm Chart Versioning:**
```yaml
# Follow SemVer
# MAJOR.MINOR.PATCH
# 1.0.0 - Initial release
# 1.1.0 - New feature (backward compatible)
# 1.0.1 - Bug fix
# 2.0.0 - Breaking change

# Chart.yaml
version: 1.2.3  # Chart version
appVersion: "1.2.3"  # Application version
```

**Release Process:**
```bash
# 1. Update Chart.yaml version
# 2. Update appVersion if application changed
# 3. Package chart
helm package family-hub-helm

# 4. Update chart repository index
helm repo index .

# 5. Push to chart repository
git add family-hub-1.2.3.tgz index.yaml
git commit -m "Release family-hub chart v1.2.3"
git push

# 6. Tag release
git tag -a v1.2.3 -m "Release v1.2.3"
git push origin v1.2.3
```

---

## Appendix A: Chart Testing

```bash
# Lint chart
helm lint family-hub-helm

# Dry run
helm install family-hub family-hub-helm \
  --namespace family-hub \
  --dry-run --debug

# Template and inspect
helm template family-hub family-hub-helm \
  --values values-production.yaml > rendered.yaml

# Test installation
helm test family-hub --namespace family-hub
```

## Appendix B: Chart Publishing

**ChartMuseum (Self-Hosted):**
```bash
# Install ChartMuseum
helm repo add chartmuseum https://chartmuseum.github.io/charts
helm install chartmuseum chartmuseum/chartmuseum

# Push chart
curl --data-binary "@family-hub-1.0.0.tgz" \
  http://chartmuseum.yourdomain.com/api/charts

# Add repository
helm repo add family-hub http://chartmuseum.yourdomain.com
helm repo update
```

---

**Document Status:** Implementation Ready
**Last Updated:** 2025-12-19
**Maintained By:** Platform Engineering Team
