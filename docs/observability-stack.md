# Observability Stack - Family Hub

**Version:** 1.0
**Date:** 2025-12-19
**Status:** Implementation Ready
**Author:** SRE Team (Claude Code)

---

## Executive Summary

This document defines the comprehensive observability stack for Family Hub, including metrics collection (Prometheus), visualization (Grafana), logging (Loki), and distributed tracing (OpenTelemetry). The stack is designed for cloud-agnostic deployment with low resource overhead.

**Key Components:**

- **Prometheus**: Metrics collection and alerting
- **Grafana**: Unified dashboards for metrics and logs
- **Loki**: Lightweight log aggregation
- **OpenTelemetry**: Distributed tracing
- **AlertManager**: Alert routing and management

---

## Table of Contents

1. [Monitoring Architecture](#1-monitoring-architecture)
2. [Prometheus Configuration](#2-prometheus-configuration)
3. [Grafana Dashboards](#3-grafana-dashboards)
4. [Logging with Loki](#4-logging-with-loki)
5. [Distributed Tracing](#5-distributed-tracing)
6. [Alerting Rules](#6-alerting-rules)

---

## 1. Monitoring Architecture

### 1.1 Architecture Diagram

```
┌────────────────────────────────────────────────────────────────┐
│                   Observability Stack                           │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │              Application Services                        │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐              │  │
│  │  │ Calendar │  │   Task   │  │ Shopping │  (+ 5 more)  │  │
│  │  │ Service  │  │ Service  │  │ Service  │              │  │
│  │  └────┬─────┘  └────┬─────┘  └────┬─────┘              │  │
│  │       │             │             │                      │  │
│  │       │ /metrics    │ /metrics    │ /metrics            │  │
│  │       │ logs        │ logs        │ logs                │  │
│  │       │ traces      │ traces      │ traces              │  │
│  └───────┼─────────────┼─────────────┼──────────────────────┘  │
│          │             │             │                         │
│          ▼             ▼             ▼                         │
│  ┌───────────────────────────────────────────────┐            │
│  │         Prometheus (Metrics Collection)       │            │
│  │  - Scrapes /metrics endpoints every 30s       │            │
│  │  - Stores time-series data (15 days)          │            │
│  │  - Evaluates alert rules                      │            │
│  │  - Resource: 500m CPU, 2Gi RAM, 50Gi storage  │            │
│  └───────────────────┬───────────────────────────┘            │
│                      │                                         │
│                      │ Alerts                                  │
│                      ▼                                         │
│  ┌───────────────────────────────────────────────┐            │
│  │         AlertManager (Alert Routing)          │            │
│  │  - Groups and deduplicates alerts             │            │
│  │  - Routes to Slack, PagerDuty, Email          │            │
│  │  - Resource: 100m CPU, 128Mi RAM              │            │
│  └───────────────────────────────────────────────┘            │
│                                                                │
│  ┌───────────────────────────────────────────────┐            │
│  │             Loki (Log Aggregation)            │            │
│  │  - Collects logs from all pods                │            │
│  │  - Stores in object storage (S3/Minio)        │            │
│  │  - Indexed by labels, not content              │            │
│  │  - Resource: 300m CPU, 1Gi RAM                │            │
│  └───────────────────────────────────────────────┘            │
│                                                                │
│  ┌───────────────────────────────────────────────┐            │
│  │     OpenTelemetry Collector (Traces)         │            │
│  │  - Receives traces from services              │            │
│  │  - Exports to Jaeger/Tempo                    │            │
│  │  - Resource: 200m CPU, 512Mi RAM              │            │
│  └───────────────────────────────────────────────┘            │
│                                                                │
│  ┌───────────────────────────────────────────────┐            │
│  │          Grafana (Visualization)              │            │
│  │  - Dashboards for metrics, logs, traces       │            │
│  │  - Unified view across all data sources       │            │
│  │  - Alerting and annotations                   │            │
│  │  - Resource: 200m CPU, 512Mi RAM              │            │
│  └───────────────────────────────────────────────┘            │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

### 1.2 Deployment Strategy

```bash
# Deploy monitoring stack using Helm
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo add grafana https://grafana.github.io/helm-charts
helm repo update

# Install Prometheus Stack (includes Grafana, AlertManager)
helm install prometheus prometheus-community/kube-prometheus-stack \
  --namespace monitoring \
  --create-namespace \
  --values prometheus-values.yaml

# Install Loki
helm install loki grafana/loki-stack \
  --namespace monitoring \
  --values loki-values.yaml

# Install OpenTelemetry Collector
helm install opentelemetry-collector open-telemetry/opentelemetry-collector \
  --namespace monitoring \
  --values otel-values.yaml
```

---

## 2. Prometheus Configuration

### 2.1 Prometheus Values (prometheus-values.yaml)

```yaml
prometheus:
  prometheusSpec:
    retention: 15d
    storageSpec:
      volumeClaimTemplate:
        spec:
          accessModes: ["ReadWriteOnce"]
          resources:
            requests:
              storage: 50Gi
    resources:
      requests:
        cpu: 500m
        memory: 2Gi
      limits:
        cpu: 2000m
        memory: 4Gi

    # Service discovery for Family Hub services
    additionalScrapeConfigs:
      - job_name: "family-hub-services"
        kubernetes_sd_configs:
          - role: pod
            namespaces:
              names:
                - family-hub
        relabel_configs:
          - source_labels:
              [__meta_kubernetes_pod_annotation_prometheus_io_scrape]
            action: keep
            regex: true
          - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_path]
            action: replace
            target_label: __metrics_path__
            regex: (.+)
          - source_labels:
              [__address__, __meta_kubernetes_pod_annotation_prometheus_io_port]
            action: replace
            regex: ([^:]+)(?::\d+)?;(\d+)
            replacement: $1:$2
            target_label: __address__
          - source_labels: [__meta_kubernetes_pod_label_app_kubernetes_io_name]
            action: replace
            target_label: service
          - source_labels: [__meta_kubernetes_namespace]
            action: replace
            target_label: namespace

alertmanager:
  alertmanagerSpec:
    resources:
      requests:
        cpu: 100m
        memory: 128Mi
      limits:
        cpu: 200m
        memory: 256Mi

grafana:
  enabled: true
  adminPassword: CHANGE_ME_STRONG_PASSWORD
  persistence:
    enabled: true
    size: 10Gi
  resources:
    requests:
      cpu: 200m
      memory: 512Mi
    limits:
      cpu: 500m
      memory: 1Gi

  # Datasources
  datasources:
    datasources.yaml:
      apiVersion: 1
      datasources:
        - name: Prometheus
          type: prometheus
          url: http://prometheus-kube-prometheus-prometheus:9090
          access: proxy
          isDefault: true
        - name: Loki
          type: loki
          url: http://loki:3100
          access: proxy
        - name: Tempo
          type: tempo
          url: http://tempo:3100
          access: proxy

  # Ingress for Grafana
  ingress:
    enabled: true
    ingressClassName: nginx
    annotations:
      cert-manager.io/cluster-issuer: letsencrypt-prod
    hosts:
      - grafana.familyhub.yourdomain.com
    tls:
      - secretName: grafana-tls
        hosts:
          - grafana.familyhub.yourdomain.com
```

### 2.2 Key Metrics to Collect

```yaml
# Application Metrics (exposed by .NET services)
# HTTP Requests
http_requests_total{service="calendar-service", method="GET", status="200"}
http_request_duration_seconds{service="calendar-service", endpoint="/api/events"}

# Database Metrics
database_connections_active{service="calendar-service"}
database_query_duration_seconds{service="calendar-service", query_type="SELECT"}

# Event Bus Metrics
event_bus_messages_published_total{service="calendar-service", event_type="CalendarEventCreated"}
event_bus_messages_consumed_total{service="task-service", event_type="CalendarEventCreated"}
event_bus_processing_duration_seconds{service="task-service"}

# Cache Metrics
cache_hits_total{service="calendar-service", cache_type="redis"}
cache_misses_total{service="calendar-service", cache_type="redis"}

# Business Metrics
calendar_events_created_total{family_group_id="uuid"}
tasks_completed_total{family_group_id="uuid"}
shopping_lists_completed_total{family_group_id="uuid"}

# Infrastructure Metrics (automatically collected)
node_cpu_seconds_total
node_memory_MemAvailable_bytes
kubelet_running_pods
kube_pod_container_resource_requests{resource="cpu"}
```

---

## 3. Grafana Dashboards

### 3.1 Family Hub Overview Dashboard

```json
{
  "dashboard": {
    "title": "Family Hub - Overview",
    "panels": [
      {
        "title": "Request Rate (req/sec)",
        "targets": [
          {
            "expr": "sum(rate(http_requests_total{namespace=\"family-hub\"}[5m])) by (service)"
          }
        ],
        "type": "graph"
      },
      {
        "title": "Error Rate (%)",
        "targets": [
          {
            "expr": "sum(rate(http_requests_total{namespace=\"family-hub\",status=~\"5..\"}[5m])) / sum(rate(http_requests_total{namespace=\"family-hub\"}[5m])) * 100"
          }
        ],
        "type": "stat"
      },
      {
        "title": "P95 Response Time (ms)",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, sum(rate(http_request_duration_seconds_bucket{namespace=\"family-hub\"}[5m])) by (le, service)) * 1000"
          }
        ],
        "type": "graph"
      },
      {
        "title": "Active Pods",
        "targets": [
          {
            "expr": "count(kube_pod_status_phase{namespace=\"family-hub\",phase=\"Running\"}) by (pod)"
          }
        ],
        "type": "stat"
      },
      {
        "title": "Database Connections",
        "targets": [
          {
            "expr": "sum(database_connections_active) by (service)"
          }
        ],
        "type": "graph"
      },
      {
        "title": "Event Bus Messages/sec",
        "targets": [
          {
            "expr": "sum(rate(event_bus_messages_published_total[5m])) by (service, event_type)"
          }
        ],
        "type": "graph"
      }
    ]
  }
}
```

### 3.2 Service-Specific Dashboard (Calendar Service)

```yaml
# Key panels:
- Request rate by endpoint
- P50, P95, P99 latency
- Error rate by status code
- Database query performance
- Cache hit/miss ratio
- Event chain latency (HealthAppointmentScheduled → CalendarEventCreated)
- Pod resource usage (CPU, memory)
- HPA scaling events
```

### 3.3 Business Metrics Dashboard

```yaml
# Key panels:
- Total families registered
- Daily active users (DAU)
- Monthly active users (MAU)
- Event chains executed per day
- Most used features (events, tasks, shopping lists)
- Conversion funnel (registration → first event → retention)
- Premium subscriptions
```

---

## 4. Logging with Loki

### 4.1 Loki Configuration (loki-values.yaml)

```yaml
loki:
  enabled: true
  persistence:
    enabled: true
    size: 50Gi
  resources:
    requests:
      cpu: 300m
      memory: 1Gi
    limits:
      cpu: 1000m
      memory: 2Gi

  config:
    auth_enabled: false
    server:
      http_listen_port: 3100

    ingester:
      chunk_idle_period: 15m
      chunk_retain_period: 30s
      max_chunk_age: 1h
      lifecycler:
        ring:
          kvstore:
            store: inmemory
          replication_factor: 1

    limits_config:
      enforce_metric_name: false
      reject_old_samples: true
      reject_old_samples_max_age: 168h # 1 week

    schema_config:
      configs:
        - from: 2024-01-01
          store: boltdb-shipper
          object_store: filesystem
          schema: v11
          index:
            prefix: index_
            period: 24h

    storage_config:
      boltdb_shipper:
        active_index_directory: /loki/index
        cache_location: /loki/cache
        shared_store: filesystem
      filesystem:
        directory: /loki/chunks

promtail:
  enabled: true
  config:
    clients:
      - url: http://loki:3100/loki/api/v1/push

    scrape_configs:
      - job_name: kubernetes-pods
        kubernetes_sd_configs:
          - role: pod
        pipeline_stages:
          - docker: {}
        relabel_configs:
          - source_labels: [__meta_kubernetes_pod_label_app_kubernetes_io_name]
            target_label: app
          - source_labels: [__meta_kubernetes_namespace]
            target_label: namespace
          - source_labels: [__meta_kubernetes_pod_name]
            target_label: pod
```

### 4.2 Log Query Examples (LogQL)

```logql
# All logs from calendar service
{namespace="family-hub", app="calendar-service"}

# Errors in last hour
{namespace="family-hub"} |= "ERROR" | logfmt | line_format "{{.timestamp}} {{.level}} {{.message}}"

# Slow database queries
{namespace="family-hub", app="calendar-service"} |= "query" | json | duration > 1s

# Event chain execution
{namespace="family-hub"} |= "HealthAppointmentScheduled" or "CalendarEventCreated"

# Failed authentication attempts
{namespace="family-hub", app="auth-service"} |= "authentication failed"

# Rate of errors per service
sum(rate({namespace="family-hub"} |= "ERROR" [5m])) by (app)
```

### 4.3 Structured Logging Format

```csharp
// .NET Serilog configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new JsonFormatter())
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
    .Enrich.WithProperty("Service", "calendar-service")
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateLogger();

// Example log output:
{
  "timestamp": "2025-12-19T10:30:00.123Z",
  "level": "Information",
  "message": "Calendar event created successfully",
  "service": "calendar-service",
  "environment": "production",
  "event_id": "uuid-here",
  "family_group_id": "uuid-here",
  "user_id": "uuid-here",
  "duration_ms": 45,
  "span_id": "abc123",
  "trace_id": "def456"
}
```

---

## 5. Distributed Tracing

### 5.1 OpenTelemetry Collector Configuration

```yaml
# otel-values.yaml
mode: deployment

config:
  receivers:
    otlp:
      protocols:
        grpc:
          endpoint: 0.0.0.0:4317
        http:
          endpoint: 0.0.0.0:4318

  processors:
    batch:
      timeout: 10s
      send_batch_size: 1024

    resource:
      attributes:
        - key: cluster
          value: family-hub-prod
          action: insert

  exporters:
    logging:
      loglevel: info

    jaeger:
      endpoint: jaeger-collector.monitoring.svc.cluster.local:14250
      tls:
        insecure: true

    prometheus:
      endpoint: 0.0.0.0:8889

  service:
    pipelines:
      traces:
        receivers: [otlp]
        processors: [batch, resource]
        exporters: [jaeger, logging]

      metrics:
        receivers: [otlp]
        processors: [batch, resource]
        exporters: [prometheus, logging]

resources:
  requests:
    cpu: 200m
    memory: 512Mi
  limits:
    cpu: 500m
    memory: 1Gi
```

### 5.2 Application Instrumentation (.NET)

```csharp
// Program.cs
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("CalendarService")
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("calendar-service", serviceVersion: "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = builder.Environment.EnvironmentName
                }))
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.Filter = (httpContext) =>
                {
                    // Don't trace health checks
                    return !httpContext.Request.Path.StartsWithSegments("/health");
                };
            })
            .AddHttpClientInstrumentation()
            .AddNpgsql()  // PostgreSQL instrumentation
            .AddRedisInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://otel-collector.monitoring.svc.cluster.local:4317");
            });
    });

// Custom activity source for manual spans
var activitySource = new ActivitySource("CalendarService");

// Example: Trace event chain
using var activity = activitySource.StartActivity("ProcessHealthAppointment", ActivityKind.Consumer);
activity?.SetTag("event.type", "HealthAppointmentScheduled");
activity?.SetTag("family.group.id", familyGroupId);
// ... business logic
activity?.SetTag("calendar.event.id", calendarEventId);
```

### 5.3 Trace Query Examples

```
# Find slow event chains
duration > 5s AND service.name = "calendar-service"

# Find failed event processing
error = true AND span.name = "ProcessHealthAppointment"

# Trace specific family group
family.group.id = "uuid-here"

# Database query traces
span.kind = "client" AND db.system = "postgresql"
```

---

## 6. Alerting Rules

### 6.1 Critical Alerts

```yaml
# prometheus-alerts.yaml
apiVersion: monitoring.coreos.com/v1
kind: PrometheusRule
metadata:
  name: family-hub-alerts
  namespace: monitoring
spec:
  groups:
    - name: family-hub-critical
      interval: 30s
      rules:
        # Service Down
        - alert: ServiceDown
          expr: up{namespace="family-hub"} == 0
          for: 2m
          labels:
            severity: critical
          annotations:
            summary: "Service {{ $labels.job }} is down"
            description: "Service {{ $labels.job }} in namespace {{ $labels.namespace }} has been down for more than 2 minutes."

        # High Error Rate
        - alert: HighErrorRate
          expr: |
            sum(rate(http_requests_total{namespace="family-hub",status=~"5.."}[5m])) by (service)
            /
            sum(rate(http_requests_total{namespace="family-hub"}[5m])) by (service)
            > 0.05
          for: 5m
          labels:
            severity: critical
          annotations:
            summary: "High error rate for {{ $labels.service }}"
            description: "Service {{ $labels.service }} has error rate > 5% (current: {{ $value | humanizePercentage }})"

        # Database Down
        - alert: DatabaseDown
          expr: up{job="postgresql"} == 0
          for: 1m
          labels:
            severity: critical
          annotations:
            summary: "PostgreSQL database is down"
            description: "PostgreSQL has been down for more than 1 minute. All services will fail."

        # Redis Down
        - alert: RedisDown
          expr: up{job="redis"} == 0
          for: 2m
          labels:
            severity: critical
          annotations:
            summary: "Redis is down"
            description: "Redis has been down for more than 2 minutes. Event bus and cache unavailable."

        # Disk Space Low
        - alert: DiskSpaceLow
          expr: |
            (node_filesystem_avail_bytes{mountpoint="/"} / node_filesystem_size_bytes{mountpoint="/"}) * 100 < 10
          for: 5m
          labels:
            severity: critical
          annotations:
            summary: "Disk space critically low on {{ $labels.instance }}"
            description: "Only {{ $value | humanize }}% disk space remaining on {{ $labels.instance }}."

    - name: family-hub-warning
      interval: 1m
      rules:
        # High Response Time
        - alert: HighResponseTime
          expr: |
            histogram_quantile(0.95,
              sum(rate(http_request_duration_seconds_bucket{namespace="family-hub"}[5m])) by (le, service)
            ) > 2
          for: 10m
          labels:
            severity: warning
          annotations:
            summary: "High response time for {{ $labels.service }}"
            description: "P95 response time is {{ $value }}s (threshold: 2s)"

        # High CPU Usage
        - alert: HighCPUUsage
          expr: |
            sum(rate(container_cpu_usage_seconds_total{namespace="family-hub"}[5m])) by (pod)
            /
            sum(kube_pod_container_resource_limits{namespace="family-hub",resource="cpu"}) by (pod)
            > 0.8
          for: 10m
          labels:
            severity: warning
          annotations:
            summary: "High CPU usage for {{ $labels.pod }}"
            description: "CPU usage is {{ $value | humanizePercentage }} of limit"

        # High Memory Usage
        - alert: HighMemoryUsage
          expr: |
            sum(container_memory_working_set_bytes{namespace="family-hub"}) by (pod)
            /
            sum(kube_pod_container_resource_limits{namespace="family-hub",resource="memory"}) by (pod)
            > 0.8
          for: 10m
          labels:
            severity: warning
          annotations:
            summary: "High memory usage for {{ $labels.pod }}"
            description: "Memory usage is {{ $value | humanizePercentage }} of limit"

        # Event Chain Latency
        - alert: EventChainSlowProcessing
          expr: |
            histogram_quantile(0.95,
              sum(rate(event_bus_processing_duration_seconds_bucket[5m])) by (le, service, event_type)
            ) > 5
          for: 10m
          labels:
            severity: warning
          annotations:
            summary: "Slow event chain processing in {{ $labels.service }}"
            description: "Event {{ $labels.event_type }} processing P95 latency is {{ $value }}s (threshold: 5s)"

        # Certificate Expiry
        - alert: CertificateExpiringSoon
          expr: |
            (certmanager_certificate_expiration_timestamp_seconds - time()) / 86400 < 7
          for: 1h
          labels:
            severity: warning
          annotations:
            summary: "Certificate {{ $labels.name }} expiring soon"
            description: "Certificate will expire in {{ $value }} days"
```

### 6.2 AlertManager Configuration

```yaml
# alertmanager-config.yaml
apiVersion: v1
kind: Secret
metadata:
  name: alertmanager-config
  namespace: monitoring
stringData:
  alertmanager.yaml: |
    global:
      resolve_timeout: 5m
      slack_api_url: 'https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK'

    route:
      group_by: ['alertname', 'cluster', 'service']
      group_wait: 10s
      group_interval: 10s
      repeat_interval: 12h
      receiver: 'default'
      routes:
      - match:
          severity: critical
        receiver: 'pagerduty'
        continue: true
      - match:
          severity: critical
        receiver: 'slack-critical'
      - match:
          severity: warning
        receiver: 'slack-warning'

    receivers:
    - name: 'default'
      slack_configs:
      - channel: '#family-hub-alerts'
        title: 'Alert: {{ .GroupLabels.alertname }}'
        text: '{{ range .Alerts }}{{ .Annotations.description }}{{ end }}'

    - name: 'slack-critical'
      slack_configs:
      - channel: '#family-hub-critical'
        title: 'CRITICAL: {{ .GroupLabels.alertname }}'
        text: '{{ range .Alerts }}{{ .Annotations.description }}{{ end }}'
        color: danger

    - name: 'slack-warning'
      slack_configs:
      - channel: '#family-hub-alerts'
        title: 'Warning: {{ .GroupLabels.alertname }}'
        text: '{{ range .Alerts }}{{ .Annotations.description }}{{ end }}'
        color: warning

    - name: 'pagerduty'
      pagerduty_configs:
      - service_key: YOUR_PAGERDUTY_SERVICE_KEY
        description: '{{ .GroupLabels.alertname }}'
```

---

## Appendix A: Quick Reference Commands

```bash
# Access Grafana
kubectl port-forward -n monitoring svc/prometheus-grafana 3000:80
# Open: http://localhost:3000 (admin / CHANGE_ME_STRONG_PASSWORD)

# Access Prometheus
kubectl port-forward -n monitoring svc/prometheus-kube-prometheus-prometheus 9090:9090
# Open: http://localhost:9090

# Query Loki logs
kubectl port-forward -n monitoring svc/loki 3100:3100
# Query: curl -G -s "http://localhost:3100/loki/api/v1/query" --data-urlencode 'query={namespace="family-hub"}'

# Check alerting rules
kubectl get prometheusrule -n monitoring

# View active alerts
kubectl port-forward -n monitoring svc/prometheus-kube-prometheus-alertmanager 9093:9093
# Open: http://localhost:9093
```

---

**Document Status:** Implementation Ready
**Last Updated:** 2025-12-19
**Maintained By:** SRE Team
