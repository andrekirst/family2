# Cloud Architecture - Family Hub

**Version:** 1.0
**Date:** 2025-12-19
**Status:** Architecture Approved
**Author:** Cloud Architect (Claude Code)

---

## Executive Summary

This document defines the cloud-agnostic Kubernetes architecture for Family Hub, a privacy-first family organization platform. The architecture is designed to:

- Run on ANY Kubernetes cluster (cloud or on-premise)
- Scale from 10 families to 10,000+ families
- Support multi-tenancy with strong data isolation
- Cost-optimize for startup budget ($200-500/month for 100 families)
- Maintain 99.9%+ availability
- Enable GitOps-based deployments

**Key Decisions:**

- **No Service Mesh** (initial phase) - NGINX Ingress only for simplicity
- **Shared Database with RLS** - PostgreSQL with Row-Level Security per tenant
- **ArgoCD for GitOps** - Automated deployments with rollback capabilities
- **Loki + Grafana for Logging** - Lightweight, cloud-agnostic observability
- **Horizontal Scaling** - All services can scale independently

---

## Table of Contents

1. [High-Level Architecture](#1-high-level-architecture)
2. [Network Architecture](#2-network-architecture)
3. [Multi-Tenancy Design](#3-multi-tenancy-design)
4. [Service Mesh Decision](#4-service-mesh-decision)
5. [Storage Strategy](#5-storage-strategy)
6. [Security Architecture](#6-security-architecture)
7. [Cost Model and Estimates](#7-cost-model-and-estimates)

---

## 1. High-Level Architecture

### 1.1 System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Internet / Users                                     │
└──────────────────────────────────┬──────────────────────────────────────────┘
                                   │
                        ┌──────────▼──────────┐
                        │   DNS / Route53     │
                        │  (Cloud Provider)   │
                        └──────────┬──────────┘
                                   │
                        ┌──────────▼──────────┐
                        │  Load Balancer      │
                        │  (Cloud Provider    │
                        │   or MetalLB)       │
                        └──────────┬──────────┘
                                   │
┌──────────────────────────────────┼──────────────────────────────────────────┐
│                    Kubernetes Cluster (Any Provider)                         │
│                                  │                                           │
│                       ┌──────────▼──────────┐                                │
│                       │  NGINX Ingress      │                                │
│                       │   Controller        │                                │
│                       │  (TLS Termination)  │                                │
│                       └──────────┬──────────┘                                │
│                                  │                                           │
│              ┌───────────────────┼───────────────────┐                       │
│              │                   │                   │                       │
│     ┌────────▼────────┐  ┌──────▼──────┐   ┌───────▼────────┐                │
│     │  Angular Web    │  │  API Gateway│   │  Monitoring     │               │
│     │  App (NGINX)    │  │   (YARP)    │   │  Services       │               │
│     │                 │  │             │   │  (Prometheus/   │               │
│     │  Pod: 2 replicas│  │ Pod: 2 reps │   │   Grafana)      │               │
│     └─────────────────┘  └──────┬──────┘   └─────────────────┘               │
│                                  │                                           │
│              ┌───────────────────┼───────────────────────────┐               │
│              │                   │                           │               │
│     ┌────────▼────────┐  ┌──────▼──────┐   ┌──────────────▼──┐               │
│     │  Auth Service   │  │  Calendar   │   │  Task Service   │               │
│     │  (.NET Core)    │  │  Service    │   │  (.NET Core)    │               │
│     │                 │  │ (.NET Core) │   │                 │               │
│     │  Pod: 1-3 reps  │  │ Pod: 1-3    │   │  Pod: 1-3 reps  │               │
│     └────────┬────────┘  └──────┬──────┘   └────────┬────────┘               │
│              │                   │                   │                       │
│     ┌────────▼────────┐  ┌──────▼──────┐   ┌───────▼────────┐                │
│     │  Shopping       │  │  Health     │   │  Meal Planning │                │
│     │  Service        │  │  Service    │   │  Service       │                │
│     │  (.NET Core)    │  │ (.NET Core) │   │  (.NET Core)   │                │
│     │  Pod: 1-3 reps  │  │ Pod: 1-3    │   │  Pod: 1-3 reps │                │
│     └────────┬────────┘  └──────┬──────┘   └────────┬───────┘                │
│              │                   │                   │                       │
│     ┌────────▼────────┐  ┌──────▼──────────────────▼───────┐                 │
│     │  Finance        │  │  Communication Service          │                 │
│     │  Service        │  │  (.NET Core)                    │                 │
│     │  (.NET Core)    │  │  Pod: 1-2 replicas              │                 │
│     │  Pod: 1-3 reps  │  └──────────────────────────────────┘                │
│     └────────┬────────┘                                                      │
│              │                                                               │
│              └──────────────────┬─────────────────────────┐                  │
│                                 │                         │                  │
│                      ┌──────────▼──────────┐   ┌─────────▼────────┐          │
│                      │   PostgreSQL        │   │   Redis          │          │
│                      │   (StatefulSet)     │   │   (StatefulSet)  │          │
│                      │   Primary + Replica │   │   Master + Slave │          │
│                      │   PVC: 50Gi-500Gi   │   │   PVC: 10Gi-50Gi │          │
│                      └─────────────────────┘   └──────────────────┘          │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐     │
│  │                    Persistent Storage (PVs)                         │     │
│  │  - Cloud Provider Storage Class (AWS EBS, Azure Disk, GCP PD)       │     │
│  │  - Or: Local storage, NFS, Ceph, Longhorn                           │     │
│  └─────────────────────────────────────────────────────────────────────┘     │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────────┐
│                        External Services                                     │
│                                                                              │
│  ┌───────────────┐     ┌──────────────┐     ┌────────────────┐               │
│  │  Zitadel      │     │  Let's       │     │  Backup        │               │
│  │  (Auth IdP)   │     │  Encrypt     │     │  Storage       │               │
│  │  External     │     │  (Cert Mgmt) │     │  (S3/Minio)    │               │
│  └───────────────┘     └──────────────┘     └────────────────┘               │
└──────────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Component Overview

| Component                 | Purpose                          | Replicas | Resources                    |
| ------------------------- | -------------------------------- | -------- | ---------------------------- |
| **NGINX Ingress**         | Traffic routing, TLS termination | 2+       | 200m CPU, 256Mi RAM          |
| **Angular Web App**       | Frontend SPA                     | 2-3      | 100m CPU, 128Mi RAM          |
| **API Gateway**           | GraphQL federation, routing      | 2-3      | 200m CPU, 256Mi RAM          |
| **Auth Service**          | Zitadel integration, family mgmt | 1-3      | 100m CPU, 256Mi RAM          |
| **Calendar Service**      | Events, appointments             | 1-3      | 150m CPU, 256Mi RAM          |
| **Task Service**          | Tasks, to-dos                    | 1-3      | 150m CPU, 256Mi RAM          |
| **Shopping Service**      | Shopping lists                   | 1-2      | 100m CPU, 128Mi RAM          |
| **Health Service**        | Appointments, prescriptions      | 1-2      | 100m CPU, 128Mi RAM          |
| **Meal Planning Service** | Recipes, meal plans              | 1-2      | 100m CPU, 128Mi RAM          |
| **Finance Service**       | Budgets, expenses                | 1-2      | 100m CPU, 128Mi RAM          |
| **Communication Service** | Notifications, messaging         | 1-2      | 150m CPU, 256Mi RAM          |
| **PostgreSQL**            | Primary database                 | 1-3      | 500m-2 CPU, 2-4Gi RAM        |
| **Redis**                 | Cache, event bus                 | 1-2      | 200m-500m CPU, 512Mi-2Gi RAM |
| **Prometheus**            | Metrics collection               | 1        | 500m CPU, 2Gi RAM            |
| **Grafana**               | Dashboards                       | 1        | 200m CPU, 512Mi RAM          |
| **Loki**                  | Log aggregation                  | 1        | 300m CPU, 1Gi RAM            |

### 1.3 Service Communication Patterns

**Synchronous (HTTP/GraphQL):**

- Frontend → API Gateway → Services
- API Gateway → Auth Service (token validation)
- Services → PostgreSQL (queries)
- Services → Redis (cache)

**Asynchronous (Events):**

- Service → Redis Pub/Sub → Subscribed Services
- Event pattern: HealthAppointmentScheduled → Calendar Service → Task Service

**Storage:**

- All services → PostgreSQL (own schema)
- Read-heavy queries → Redis cache
- Static assets → CDN (future)

---

## 2. Network Architecture

### 2.1 Network Topology

```
┌──────────────────────────────────────────────────────────────────┐
│                      Public Internet                             │
└───────────────────────────┬──────────────────────────────────────┘
                            │
                  ┌─────────▼─────────┐
                  │  External Load    │
                  │  Balancer (L4/L7) │
                  │  (Cloud Provider) │
                  └─────────┬─────────┘
                            │
┌───────────────────────────┼──────────────────────────────────────┐
│ Kubernetes Cluster        │                                      │
│                           │                                      │
│  ┌────────────────────────▼───────────────────────────────────┐  │
│  │            Ingress Namespace                               │  │
│  │  ┌────────────────────────────────────────────────────┐    │  │
│  │  │  NGINX Ingress Controller                          │    │  │
│  │  │  - TLS Termination (Let's Encrypt certs)           │    │  │
│  │  │  - Rate Limiting                                   │    │  │
│  │  │  - IP Whitelisting (optional)                      │    │  │
│  │  └────────────────────┬───────────────────────────────┘    │  │
│  └────────────────────────┼───────────────────────────────────┘  │
│                           │                                      │
│  ┌────────────────────────▼───────────────────────────────────┐  │
│  │         Application Namespace (family-hub)                 │  │
│  │                                                            │  │
│  │  ┌──────────────┐   ┌──────────────┐   ┌──────────────┐    │  │
│  │  │  Frontend    │   │  API Gateway │   │  Services    │    │  │
│  │  │  (Public)    │   │  (Internal)  │   │  (Internal)  │    │  │
│  │  │              │   │              │   │              │    │  │
│  │  │  Port: 80    │   │  Port: 8080  │   │  Port: 5xxx  │    │  │
│  │  └──────────────┘   └──────┬───────┘   └───┬──────────┘    │  │
│  │                             │               │              │  │
│  │                             └───────┬───────┘              │  │
│  │                                     │                      │  │
│  │  ┌──────────────────────────────────▼───────────────────┐  │  │
│  │  │           Data Namespace (family-hub-data)           │  │  │
│  │  │                                                      │  │  │
│  │  │  ┌──────────────┐          ┌──────────────┐          │  │  │
│  │  │  │ PostgreSQL   │          │   Redis      │          │  │  │
│  │  │  │ (Internal)   │          │  (Internal)  │          │  │  │
│  │  │  │ Port: 5432   │          │  Port: 6379  │          │  │  │
│  │  │  └──────────────┘          └──────────────┘          │  │  │
│  │  └──────────────────────────────────────────────────────┘  │  │
│  │                                                            │  │
│  │  ┌──────────────────────────────────────────────────────┐  │  │
│  │  │      Monitoring Namespace (monitoring)               │  │  │
│  │  │                                                      │  │  │
│  │  │  ┌──────────┐  ┌──────────┐  ┌──────────┐            │  │  │
│  │  │  │Prometheus│  │ Grafana  │  │  Loki    │            │  │  │
│  │  │  │(Internal)│  │(Public)  │  │(Internal)│            │  │  │
│  │  │  └──────────┘  └──────────┘  └──────────┘            │  │  │
│  │  └──────────────────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

### 2.2 Network Policies

**Ingress Rules:**

```yaml
# Allow frontend to be accessed from ingress
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: allow-frontend-ingress
  namespace: family-hub
spec:
  podSelector:
    matchLabels:
      app: frontend
  ingress:
    - from:
        - namespaceSelector:
            matchLabels:
              name: ingress-nginx
      ports:
        - protocol: TCP
          port: 80
```

**Service-to-Service Rules:**

```yaml
# Allow API Gateway to call microservices
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: allow-api-gateway-to-services
  namespace: family-hub
spec:
  podSelector:
    matchLabels:
      tier: backend
  ingress:
    - from:
        - podSelector:
            matchLabels:
              app: api-gateway
      ports:
        - protocol: TCP
          port: 5000-5010
```

**Database Access Rules:**

```yaml
# Only backend services can access PostgreSQL
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: allow-db-access
  namespace: family-hub-data
spec:
  podSelector:
    matchLabels:
      app: postgresql
  ingress:
    - from:
        - namespaceSelector:
            matchLabels:
              name: family-hub
          podSelector:
            matchLabels:
              tier: backend
      ports:
        - protocol: TCP
          port: 5432
```

### 2.3 DNS and Service Discovery

**Internal Service Discovery:**

- Kubernetes DNS: `<service-name>.<namespace>.svc.cluster.local`
- Example: `calendar-service.family-hub.svc.cluster.local`

**External Access:**

- Primary domain: `familyhub.yourdomain.com`
- API: `api.familyhub.yourdomain.com`
- Monitoring: `grafana.familyhub.yourdomain.com`

**TLS Certificates:**

- Cert-manager with Let's Encrypt
- Auto-renewal every 60 days
- Wildcard certificate for `*.familyhub.yourdomain.com`

---

## 3. Multi-Tenancy Design

### 3.1 Tenant Isolation Strategy

**Decision: Shared Database with Row-Level Security (RLS)**

**Rationale:**

- Simpler operations (single database to manage)
- Lower infrastructure costs ($100/month vs $1000+/month for 100 tenants)
- Sufficient isolation for family data (not regulated financial data)
- PostgreSQL RLS provides strong guarantees
- Easier to start, can shard later if needed

**Alternative Considered:**

- Dedicated DB per family: Too expensive and complex for 10-10,000 families
- Namespace per family: Massive Kubernetes overhead, not practical

### 3.2 Database Isolation Architecture

```sql
-- Family Hub Multi-Tenancy Implementation

-- 1. Each table has family_group_id column
CREATE TABLE calendar.events (
    id UUID PRIMARY KEY,
    family_group_id UUID NOT NULL REFERENCES auth.family_groups(id),
    title TEXT NOT NULL,
    -- ... other columns
);

-- 2. Row-Level Security policy
ALTER TABLE calendar.events ENABLE ROW LEVEL SECURITY;

CREATE POLICY family_isolation_policy ON calendar.events
    USING (
        family_group_id IN (
            SELECT fm.family_group_id
            FROM auth.family_members fm
            WHERE fm.user_id = current_setting('app.current_user_id')::UUID
              AND fm.is_active = true
        )
    );

-- 3. Application sets user context
-- In C# service:
await connection.ExecuteAsync(
    "SET app.current_user_id = @userId",
    new { userId = currentUser.Id }
);

-- 4. All queries automatically filtered
SELECT * FROM calendar.events WHERE start_time > NOW();
-- PostgreSQL automatically adds: AND family_group_id IN (...)
```

### 3.3 Tenant Onboarding Flow

```
1. User Registration
   ↓
2. Create Family Group (family_groups table)
   ↓
3. Assign User as Owner (family_members table)
   ↓
4. Initialize Default Data (templates, settings)
   ↓
5. Send Welcome Email
   ↓
6. Ready to Use (all RLS policies active)
```

**Onboarding Automation:**

```yaml
# Kubernetes Job for tenant initialization
apiVersion: batch/v1
kind: Job
metadata:
  name: tenant-onboarding
spec:
  template:
    spec:
      containers:
        - name: onboarding
          image: familyhub/tenant-onboarding:latest
          env:
            - name: FAMILY_GROUP_ID
              value: "uuid-here"
            - name: OWNER_EMAIL
              value: "user@example.com"
```

### 3.4 Resource Quotas per Tenant

**Soft Limits (Application-Enforced):**

```yaml
# Configurable per subscription tier
limits:
  free_tier:
    max_family_members: 6
    max_calendar_events: 1000
    max_tasks: 500
    max_shopping_lists: 50
    max_recipes: 100
    storage_mb: 100

  premium_tier:
    max_family_members: 10
    max_calendar_events: 10000
    max_tasks: 5000
    max_shopping_lists: 500
    max_recipes: 1000
    storage_mb: 1000
```

**Hard Limits (Database-Enforced):**

```sql
-- Prevent runaway data growth
CREATE OR REPLACE FUNCTION check_family_limits()
RETURNS TRIGGER AS $$
BEGIN
    IF (SELECT COUNT(*) FROM calendar.events
        WHERE family_group_id = NEW.family_group_id) > 10000 THEN
        RAISE EXCEPTION 'Calendar event limit exceeded';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER enforce_calendar_limit
    BEFORE INSERT ON calendar.events
    FOR EACH ROW EXECUTE FUNCTION check_family_limits();
```

### 3.5 Cost Allocation per Tenant

**Metrics to Track:**

```yaml
# Prometheus metrics per tenant
- database_queries_total{family_group_id="uuid"}
- api_requests_total{family_group_id="uuid"}
- storage_bytes{family_group_id="uuid"}
- event_bus_messages{family_group_id="uuid"}
```

**Cost Calculation (monthly):**

```
Per-Family Cost =
  (Database CPU time × $0.05/CPU-hour) +
  (Storage GB × $0.10/GB) +
  (API requests × $0.0001) +
  (Fixed overhead / total families)

Estimated: $0.50 - $2.00 per family/month
Target: 100 families = $50-200/month variable + $200 fixed = $250-400 total
```

---

## 4. Service Mesh Decision

### 4.1 Decision: NO Service Mesh (Initial Deployment)

**Rationale:**

1. **Simplicity First**: Single developer, limited ops experience
2. **Cost**: Service mesh adds 20-30% resource overhead
3. **Complexity**: Istio/Linkerd add significant learning curve
4. **Not Needed Yet**: <1000 users, simple traffic patterns
5. **NGINX Ingress Sufficient**: Handles 90% of needs

**What We Lose:**

- Automatic mTLS between services
- Advanced traffic routing (canary, blue-green)
- Distributed tracing (but we have OpenTelemetry)
- Circuit breaking (but we have Polly in .NET)

**What We Gain:**

- 30% lower infrastructure costs
- Faster iteration and debugging
- Easier troubleshooting
- Simpler operations

### 4.2 Alternatives Considered

| Service Mesh           | Pros                              | Cons                                          | Decision            |
| ---------------------- | --------------------------------- | --------------------------------------------- | ------------------- |
| **Istio**              | Feature-rich, industry standard   | Complex, resource-heavy, steep learning curve | NO (too complex)    |
| **Linkerd**            | Lightweight, easier than Istio    | Still adds complexity                         | NO (not needed yet) |
| **NGINX Ingress Only** | Simple, battle-tested, sufficient | Less observability                            | YES (chosen)        |

### 4.3 Migration Path (Future)

**When to Add Service Mesh:**

- 1000+ active families
- Complex multi-region deployments
- Need for advanced traffic management
- Security compliance requires mTLS

**Recommended Path:**

1. Start with NGINX Ingress (Phase 0-5)
2. Add OpenTelemetry for tracing (Phase 5)
3. Evaluate Linkerd if needed (Phase 6+)
4. Only add Istio if enterprise features required (Phase 7+)

### 4.4 Implemented Features Without Service Mesh

**Security:**

- TLS termination at ingress (NGINX)
- Network policies (Kubernetes native)
- Pod Security Standards (Kubernetes native)

**Observability:**

- Prometheus metrics (service instrumentation)
- Loki logs (centralized logging)
- OpenTelemetry tracing (distributed tracing)

**Resilience:**

- Polly for retry/circuit breaker (.NET)
- Kubernetes health checks
- HPA for auto-scaling

**Traffic Management:**

- NGINX Ingress for routing
- Kubernetes Services for load balancing
- Manual canary deployments (ArgoCD)

---

## 5. Storage Strategy

### 5.1 Persistent Storage Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Storage Tiers                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Tier 1: Hot Data (Redis)                                       │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - Active user sessions                                  │    │
│  │ - Calendar events (7-day window)                        │    │
│  │ - Task lists (current)                                  │    │
│  │ - Cache (GraphQL responses)                             │    │
│  │ Storage: 10-50 GB                                       │    │
│  │ Access: <10ms latency                                   │    │
│  └────────────────────────────────────────────────────────┘    │
│                           │                                      │
│                           ▼                                      │
│  Tier 2: Warm Data (PostgreSQL - Primary)                       │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - All operational data                                  │    │
│  │ - Events, tasks, recipes, expenses (90 days)           │    │
│  │ - User profiles, family groups                          │    │
│  │ Storage: 50-500 GB                                      │    │
│  │ Access: <100ms latency                                  │    │
│  └────────────────────────────────────────────────────────┘    │
│                           │                                      │
│                           ▼                                      │
│  Tier 3: Cold Data (PostgreSQL - Archived)                      │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - Historical data (>90 days)                            │    │
│  │ - Archived events, old expenses                         │    │
│  │ - Partitioned tables                                    │    │
│  │ Storage: 100-1000 GB                                    │    │
│  │ Access: <500ms latency                                  │    │
│  └────────────────────────────────────────────────────────┘    │
│                           │                                      │
│                           ▼                                      │
│  Tier 4: Backup Data (Object Storage)                           │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - Daily database backups                                │    │
│  │ - Document storage (future)                             │    │
│  │ - Disaster recovery                                     │    │
│  │ Storage: S3/Minio/Backblaze B2                          │    │
│  │ Retention: 30 days (rolling)                            │    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 5.2 PostgreSQL Configuration

**Storage Class Selection (Cloud-Agnostic):**

```yaml
# Generic storage class - maps to cloud provider
apiVersion: storage.k8s.io/v1
kind: StorageClass
metadata:
  name: family-hub-db-storage
provisioner: kubernetes.io/cloud-provider # Auto-detected
parameters:
  type: ssd # Fast storage for database
  replication-type: regional # High availability
allowVolumeExpansion: true
volumeBindingMode: WaitForFirstConsumer
```

**PersistentVolumeClaim:**

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: postgresql-pvc
  namespace: family-hub-data
spec:
  accessModes:
    - ReadWriteOnce
  storageClassName: family-hub-db-storage
  resources:
    requests:
      storage: 50Gi # Start small, expand as needed
```

**Scaling Strategy:**

```yaml
# Initial (10-100 families): 50GB
# Growth (100-1000 families): 200GB
# Scale (1000-10000 families): 500GB-1TB

# Automatic expansion when 80% full
# Manual intervention for >1TB
```

### 5.3 Redis Configuration

**Deployment Mode:**

```yaml
# Phase 0-4: Single Redis instance
# Phase 5+: Redis Sentinel (HA) or Redis Cluster

# StatefulSet for Redis
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: redis
  namespace: family-hub-data
spec:
  serviceName: redis
  replicas: 1 # Start with 1, scale to 3 with Sentinel
  selector:
    matchLabels:
      app: redis
  template:
    metadata:
      labels:
        app: redis
    spec:
      containers:
        - name: redis
          image: redis:7-alpine
          ports:
            - containerPort: 6379
          volumeMounts:
            - name: data
              mountPath: /data
          resources:
            requests:
              cpu: 200m
              memory: 512Mi
            limits:
              cpu: 500m
              memory: 2Gi
  volumeClaimTemplates:
    - metadata:
        name: data
      spec:
        accessModes: ["ReadWriteOnce"]
        storageClassName: family-hub-db-storage
        resources:
          requests:
            storage: 10Gi
```

### 5.4 Backup Strategy

**Automated Backups:**

```yaml
# CronJob for PostgreSQL backups
apiVersion: batch/v1
kind: CronJob
metadata:
  name: postgres-backup
  namespace: family-hub-data
spec:
  schedule: "0 2 * * *" # Daily at 2 AM UTC
  jobTemplate:
    spec:
      template:
        spec:
          containers:
            - name: backup
              image: postgres:16-alpine
              env:
                - name: PGHOST
                  value: postgresql.family-hub-data.svc.cluster.local
                - name: PGUSER
                  valueFrom:
                    secretKeyRef:
                      name: postgres-credentials
                      key: username
                - name: PGPASSWORD
                  valueFrom:
                    secretKeyRef:
                      name: postgres-credentials
                      key: password
                - name: S3_BUCKET
                  value: familyhub-backups
              command:
                - /bin/sh
                - -c
                - |
                  pg_dump -Fc > /tmp/backup.dump
                  aws s3 cp /tmp/backup.dump s3://$S3_BUCKET/$(date +%Y%m%d).dump
          restartPolicy: OnFailure
```

**Backup Retention:**

- Daily backups: 7 days
- Weekly backups: 4 weeks
- Monthly backups: 12 months

**Recovery Time Objective (RTO):**

- Database restore: < 1 hour
- Full system restore: < 4 hours

**Recovery Point Objective (RPO):**

- Data loss tolerance: < 24 hours (daily backups)
- Critical data: < 1 hour (WAL archiving for Phase 5+)

---

## 6. Security Architecture

### 6.1 Zero-Trust Security Model

```
┌─────────────────────────────────────────────────────────────────┐
│                     Security Layers                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Layer 1: Edge Security (Ingress)                               │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - TLS 1.3 only (no older protocols)                     │    │
│  │ - Rate limiting (100 req/min per IP)                    │    │
│  │ - DDoS protection (cloud provider)                      │    │
│  │ - IP whitelisting (optional)                            │    │
│  │ - Web Application Firewall (WAF) - future               │    │
│  └────────────────────────────────────────────────────────┘    │
│                           │                                      │
│                           ▼                                      │
│  Layer 2: Authentication & Authorization                         │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - Zitadel OAuth 2.0 / OIDC                              │    │
│  │ - JWT token validation (every request)                  │    │
│  │ - Role-based access control (RBAC)                      │    │
│  │ - Family-level permissions                              │    │
│  │ - Token expiry: 1 hour (refresh: 7 days)               │    │
│  └────────────────────────────────────────────────────────┘    │
│                           │                                      │
│                           ▼                                      │
│  Layer 3: Network Isolation                                      │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - Kubernetes Network Policies                           │    │
│  │ - Namespace isolation                                   │    │
│  │ - Pod-to-pod restrictions                               │    │
│  │ - Database accessible only from backend pods            │    │
│  └────────────────────────────────────────────────────────┘    │
│                           │                                      │
│                           ▼                                      │
│  Layer 4: Data Protection                                        │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - Encryption at rest (PostgreSQL, Redis)                │    │
│  │ - Encryption in transit (TLS)                           │    │
│  │ - Row-Level Security (RLS) in database                  │    │
│  │ - Sensitive data masking in logs                        │    │
│  │ - PII field encryption (Health, Finance services)       │    │
│  └────────────────────────────────────────────────────────┘    │
│                           │                                      │
│                           ▼                                      │
│  Layer 5: Application Security                                   │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - Input validation (all endpoints)                      │    │
│  │ - SQL injection protection (parameterized queries)      │    │
│  │ - XSS protection (CSP headers)                          │    │
│  │ - CSRF tokens (state-changing operations)               │    │
│  │ - Dependency scanning (Snyk/Dependabot)                 │    │
│  └────────────────────────────────────────────────────────┘    │
│                           │                                      │
│                           ▼                                      │
│  Layer 6: Monitoring & Incident Response                         │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ - Audit logging (all sensitive operations)              │    │
│  │ - Security alerts (failed logins, anomalies)            │    │
│  │ - Intrusion detection (future)                          │    │
│  │ - Incident response playbooks                           │    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 6.2 TLS and Certificate Management

**Cert-Manager Configuration:**

```yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@familyhub.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
      - http01:
          ingress:
            class: nginx
```

**Certificate Request:**

```yaml
apiVersion: cert-manager.io/v1
kind: Certificate
metadata:
  name: familyhub-tls
  namespace: family-hub
spec:
  secretName: familyhub-tls-secret
  issuerRef:
    name: letsencrypt-prod
    kind: ClusterIssuer
  dnsNames:
    - familyhub.yourdomain.com
    - api.familyhub.yourdomain.com
    - "*.familyhub.yourdomain.com" # Wildcard
```

**Ingress TLS Configuration:**

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: familyhub-ingress
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/force-ssl-redirect: "true"
spec:
  ingressClassName: nginx
  tls:
    - hosts:
        - familyhub.yourdomain.com
        - api.familyhub.yourdomain.com
      secretName: familyhub-tls-secret
  rules:
    - host: familyhub.yourdomain.com
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: frontend
                port:
                  number: 80
```

### 6.3 Secrets Management

**Strategy: Kubernetes Secrets + Sealed Secrets**

**Why This Approach:**

- Kubernetes Secrets: Native, simple, sufficient for Phase 0-5
- Sealed Secrets: Enables GitOps (encrypted secrets in Git)
- Alternative (HashiCorp Vault): Too complex for single developer

**Sealed Secrets Configuration:**

```yaml
# Install Sealed Secrets controller
kubectl apply -f https://github.com/bitnami-labs/sealed-secrets/releases/download/v0.24.0/controller.yaml

# Create secret locally
kubectl create secret generic postgres-credentials \
  --from-literal=username=familyhub_user \
  --from-literal=password='super-secret-password' \
  --dry-run=client -o yaml > postgres-secret.yaml

# Seal the secret (encrypted)
kubeseal --format=yaml < postgres-secret.yaml > postgres-sealed-secret.yaml

# Commit sealed secret to Git (safe)
git add postgres-sealed-secret.yaml
git commit -m "Add PostgreSQL credentials (sealed)"
```

**Sealed Secret Example:**

```yaml
apiVersion: bitnami.com/v1alpha1
kind: SealedSecret
metadata:
  name: postgres-credentials
  namespace: family-hub-data
spec:
  encryptedData:
    username: AgBhY2RlZjEyMzQ1Njc4OTAhISE= # Encrypted
    password: AgBhY2RlZjEyMzQ1Njc4OTAhISE= # Encrypted
  template:
    metadata:
      name: postgres-credentials
      namespace: family-hub-data
```

**Migration Path to Vault (Future):**

- Phase 7+: If team grows or enterprise features needed
- External Secrets Operator: Bridge to any secret backend
- Timeline: Post-MVP, based on security audit recommendations

### 6.4 RBAC Configuration

**Namespace-Level RBAC:**

```yaml
# Developer role (read-only in prod)
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: developer
  namespace: family-hub
rules:
- apiGroups: [""]
  resources: ["pods", "pods/log", "services"]
  verbs: ["get", "list", "watch"]
- apiGroups: ["apps"]
  resources: ["deployments", "statefulsets"]
  verbs: ["get", "list", "watch"]

# CI/CD role (full access for ArgoCD)
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: cicd
  namespace: family-hub
rules:
- apiGroups: ["*"]
  resources: ["*"]
  verbs: ["*"]
```

**Service Account for Services:**

```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: calendar-service
  namespace: family-hub
---
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: calendar-service-role
  namespace: family-hub
rules:
  - apiGroups: [""]
    resources: ["secrets"]
    resourceNames: ["postgres-credentials"]
    verbs: ["get"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: calendar-service-binding
  namespace: family-hub
subjects:
  - kind: ServiceAccount
    name: calendar-service
    namespace: family-hub
roleRef:
  kind: Role
  name: calendar-service-role
  apiGroup: rbac.authorization.k8s.io
```

### 6.5 Pod Security Standards

**Pod Security Admission:**

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: family-hub
  labels:
    pod-security.kubernetes.io/enforce: restricted
    pod-security.kubernetes.io/audit: restricted
    pod-security.kubernetes.io/warn: restricted
```

**Secure Pod Specification:**

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: calendar-service
  namespace: family-hub
spec:
  serviceAccountName: calendar-service
  securityContext:
    runAsNonRoot: true
    runAsUser: 1000
    fsGroup: 1000
    seccompProfile:
      type: RuntimeDefault
  containers:
    - name: calendar
      image: familyhub/calendar-service:latest
      securityContext:
        allowPrivilegeEscalation: false
        capabilities:
          drop:
            - ALL
        readOnlyRootFilesystem: true
      resources:
        requests:
          cpu: 150m
          memory: 256Mi
        limits:
          cpu: 500m
          memory: 512Mi
```

---

## 7. Cost Model and Estimates

### 7.1 Infrastructure Cost Breakdown

**Phase 1: Development (10 families)**

| Component              | Specification             | Monthly Cost      | Provider Notes                     |
| ---------------------- | ------------------------- | ----------------- | ---------------------------------- |
| **Kubernetes Cluster** | 2 nodes × 2 vCPU, 4GB RAM | $40-80            | DO: $40, Linode: $40, Hetzner: $30 |
| **PostgreSQL**         | 2 vCPU, 4GB RAM, 50GB SSD | $30-60            | Managed or self-hosted             |
| **Redis**              | 1 vCPU, 1GB RAM           | $10-20            | Self-hosted on cluster             |
| **Load Balancer**      | Cloud LB                  | $10-20            | Provider-managed                   |
| **DNS & Domain**       | Domain + DNS hosting      | $1-5              | Cloudflare (free) or provider      |
| **Backups (S3/Minio)** | 50GB storage              | $1-5              | Backblaze B2: $0.005/GB            |
| **Monitoring**         | Self-hosted on cluster    | $0                | Prometheus/Grafana/Loki            |
| **TLS Certificates**   | Let's Encrypt             | $0                | Free                               |
| **TOTAL**              |                           | **$92-190/month** | **Target: ~$150/month**            |

**Phase 2: Growth (100 families)**

| Component              | Specification              | Monthly Cost       | Notes                      |
| ---------------------- | -------------------------- | ------------------ | -------------------------- |
| **Kubernetes Cluster** | 3 nodes × 4 vCPU, 8GB RAM  | $120-240           | Auto-scaling enabled       |
| **PostgreSQL**         | 4 vCPU, 8GB RAM, 200GB SSD | $80-150            | Read replica added         |
| **Redis**              | 2 vCPU, 2GB RAM (Sentinel) | $30-50             | HA setup                   |
| **Load Balancer**      | Cloud LB                   | $20-30             | Higher bandwidth           |
| **Backups (S3)**       | 200GB storage              | $1-10              | Daily backups              |
| **CDN**                | 100GB transfer             | $5-15              | Optional for static assets |
| **Monitoring**         | Dedicated node             | $40-80             | Larger Prometheus/Loki     |
| **TOTAL**              |                            | **$296-575/month** | **Target: ~$400/month**    |

**Phase 3: Scale (1,000 families)**

| Component              | Specification                   | Monthly Cost         | Notes                     |
| ---------------------- | ------------------------------- | -------------------- | ------------------------- |
| **Kubernetes Cluster** | 6 nodes × 4 vCPU, 8GB RAM       | $240-480             | Multi-AZ                  |
| **PostgreSQL**         | 8 vCPU, 16GB RAM, 500GB SSD     | $200-400             | Primary + 2 replicas      |
| **Redis Cluster**      | 6 nodes (3 masters, 3 replicas) | $120-200             | High availability         |
| **Load Balancer**      | Cloud LB (HA)                   | $40-60               | Multi-region              |
| **Backups (S3)**       | 500GB storage                   | $2.50-25             | Daily + weekly            |
| **CDN**                | 1TB transfer                    | $20-50               | Mandatory for performance |
| **Monitoring**         | 2 dedicated nodes               | $80-160              | Separate namespace        |
| **TOTAL**              |                                 | **$702-1,375/month** | **Target: ~$1,000/month** |

**Phase 4: Enterprise (10,000 families)**

| Component              | Specification                    | Monthly Cost           | Notes                     |
| ---------------------- | -------------------------------- | ---------------------- | ------------------------- |
| **Kubernetes Cluster** | 20 nodes × 8 vCPU, 16GB RAM      | $1,600-3,200           | Multi-region              |
| **PostgreSQL**         | 16 vCPU, 32GB RAM, 2TB SSD       | $800-1,600             | Sharded or Citus          |
| **Redis Cluster**      | 12 nodes (6 masters, 6 replicas) | $480-800               | Multi-region              |
| **Load Balancer**      | Global LB                        | $100-200               | Multi-region failover     |
| **Backups (S3)**       | 2TB storage                      | $10-100                | Incremental backups       |
| **CDN**                | 10TB transfer                    | $100-300               | Essential                 |
| **Monitoring**         | Dedicated cluster                | $200-400               | Prometheus federation     |
| **Security**           | WAF, DDoS protection             | $100-300               | Cloud provider services   |
| **TOTAL**              |                                  | **$3,390-6,900/month** | **Target: ~$5,000/month** |

### 7.2 Cost per Family Analysis

```
Phase 1 (10 families): $150/month ÷ 10 = $15.00/family/month
Phase 2 (100 families): $400/month ÷ 100 = $4.00/family/month
Phase 3 (1,000 families): $1,000/month ÷ 1,000 = $1.00/family/month
Phase 4 (10,000 families): $5,000/month ÷ 10,000 = $0.50/family/month
```

**Cost Reduction Through Scale:**

- 10x user growth = 10x cost reduction per user
- Economies of scale for shared infrastructure
- Fixed costs (monitoring, LB) amortized across more users

**Monetization Target:**

- Free tier: Sustainable at $0.50-1.00/family/month cost
- Premium tier: $9.99/month (10-20x cost)
- Enterprise tier: $14.99/month (15-30x cost)

### 7.3 Cloud Provider Comparison (100 families scenario)

| Provider             | K8s Cluster | PostgreSQL | Redis    | Load Balancer | Total/Month | Notes                     |
| -------------------- | ----------- | ---------- | -------- | ------------- | ----------- | ------------------------- |
| **DigitalOcean**     | $120        | $60        | Included | $10           | **$190**    | Simple pricing, good docs |
| **Linode**           | $120        | $50        | Included | $10           | **$180**    | Competitive pricing       |
| **Hetzner Cloud**    | $90         | $40\*      | Included | $5\*          | **$135**    | Cheapest EU option        |
| **AWS EKS**          | $216        | $120       | $50      | $20           | **$406**    | Enterprise features       |
| **Azure AKS**        | $200        | $110       | $45      | $20           | **$375**    | Good MSFT integration     |
| **GCP GKE**          | $194        | $105       | $40      | $20           | **$359**    | Best AI/ML features       |
| **On-Premise (k3s)** | $0\*\*      | $0\*\*     | $0\*\*   | $0\*\*        | **$50**     | Hardware + electricity    |

\*Hetzner requires self-managed PostgreSQL (not recommended for production)
\*\*On-premise requires upfront hardware investment (~$2,000-5,000)

**Recommended Providers by Phase:**

- **Phase 0-2 (Dev/Test)**: DigitalOcean or Linode - Best balance of simplicity and cost
- **Phase 3-4 (Production)**: DigitalOcean, Linode, or Hetzner - Proven scalability
- **Phase 5+ (Enterprise)**: AWS/Azure/GCP - If enterprise features needed
- **Self-Hosting**: k3s on own hardware - For privacy-focused users

### 7.4 Cost Optimization Strategies

**1. Right-Sizing Resources**

```yaml
# Start conservative, scale based on metrics
resources:
  requests: # Guaranteed resources
    cpu: 100m
    memory: 128Mi
  limits: # Maximum allowed
    cpu: 500m
    memory: 512Mi
# Monitor actual usage, adjust every 2 weeks
# Goal: requests = p95 usage, limits = p99 usage
```

**2. Horizontal Pod Autoscaler (HPA)**

```yaml
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
  minReplicas: 1
  maxReplicas: 5
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
```

**3. Database Query Optimization**

- Implement GraphQL DataLoader (N+1 prevention)
- Add appropriate indexes (monitor slow queries)
- Use connection pooling (PgBouncer)
- Cache frequently accessed data in Redis

**4. Scheduled Scaling**

```yaml
# Scale down during low-usage hours (2 AM - 6 AM UTC)
apiVersion: batch/v1
kind: CronJob
metadata:
  name: scale-down-nightly
spec:
  schedule: "0 2 * * *"
  jobTemplate:
    spec:
      template:
        spec:
          containers:
            - name: scale
              image: bitnami/kubectl:latest
              command:
                - kubectl
                - scale
                - deployment/calendar-service
                - --replicas=1
                - -n family-hub
```

**5. Spot Instances for Non-Critical Workloads**

```yaml
# Use spot instances for batch jobs, dev environments
nodeSelector:
  cloud.google.com/gke-preemptible: "true"
  # or
  eks.amazonaws.com/capacityType: "SPOT"
```

**6. Storage Optimization**

- Delete old partitioned data (>1 year)
- Compress backup files
- Use lifecycle policies for S3 backups (Glacier after 90 days)

**Expected Savings:**

- Right-sizing: 20-30% cost reduction
- HPA: 15-25% cost reduction
- Spot instances: 50-70% cost reduction (non-prod)
- Database optimization: 10-20% performance improvement
- **Total potential savings: 30-50% of baseline costs**

### 7.5 ROI Projections

**Scenario: 100 Paying Families**

```
Revenue (Premium @ $9.99/month):
  100 families × $9.99 = $999/month = $11,988/year

Costs:
  Infrastructure: $400/month = $4,800/year
  Domain/Services: $50/year
  Development (part-time): $0 (sweat equity)
  Total: $4,850/year

Gross Margin: $11,988 - $4,850 = $7,138/year (60% margin)
```

**Scenario: 1,000 Paying Families**

```
Revenue (50% Premium, 50% Free):
  500 families × $9.99 = $4,995/month = $59,940/year

Costs:
  Infrastructure: $1,000/month = $12,000/year
  Services: $500/year
  Support (part-time): $12,000/year
  Total: $24,500/year

Gross Margin: $59,940 - $24,500 = $35,440/year (59% margin)
```

**Scenario: 10,000 Families (30% conversion)**

```
Revenue:
  3,000 Premium × $9.99 = $29,970/month = $359,640/year

Costs:
  Infrastructure: $5,000/month = $60,000/year
  Services: $2,000/year
  Support (full-time): $50,000/year
  Marketing: $20,000/year
  Total: $132,000/year

Gross Margin: $359,640 - $132,000 = $227,640/year (63% margin)
```

**Break-Even Analysis:**

- Free tier (cost $0.50/family/month): 300 premium users break even at Phase 2 costs
- Target: 10% conversion rate → Need 3,000 total users for break-even
- Timeline: Achievable by end of Phase 4 (Month 18-24)

---

## 8. Scalability and Performance

### 8.1 Scaling Dimensions

**Vertical Scaling (Scale Up):**

- Increase CPU/memory for PostgreSQL when query performance degrades
- Trigger: CPU >80% sustained, query latency >500ms p95
- Limit: Single node up to 32 vCPU, 128GB RAM (cloud providers)

**Horizontal Scaling (Scale Out):**

- Add more pods for stateless services (all microservices)
- HPA automatically scales based on CPU/memory metrics
- Trigger: CPU >70%, memory >80%
- Limit: Cost constraints, diminishing returns after 10 replicas

**Database Scaling:**

```
Phase 1-2: Single PostgreSQL instance
Phase 3-4: Primary + Read replicas (2-3 replicas)
Phase 5+: Sharding by family_group_id or Citus distributed PostgreSQL
```

**Cache Scaling:**

```
Phase 1-2: Single Redis instance
Phase 3-4: Redis Sentinel (1 master, 2 replicas)
Phase 5+: Redis Cluster (6+ nodes)
```

### 8.2 Performance Targets

| Metric                      | Phase 1-2 | Phase 3-4 | Phase 5+ |
| --------------------------- | --------- | --------- | -------- |
| **API Response Time (p95)** | <2s       | <1s       | <500ms   |
| **Page Load Time**          | <3s       | <2s       | <1s      |
| **Event Chain Latency**     | <5s       | <3s       | <2s      |
| **Concurrent Users**        | 50        | 500       | 5,000    |
| **Database Queries/sec**    | 100       | 1,000     | 10,000   |
| **Event Bus Messages/sec**  | 10        | 100       | 1,000    |
| **Uptime SLA**              | 99%       | 99.5%     | 99.9%    |

### 8.3 Load Testing Strategy

**Tools:**

- k6 (load testing)
- Locust (user behavior simulation)
- pgbench (PostgreSQL benchmarking)

**Test Scenarios:**

```javascript
// k6 load test script
import http from "k6/http";
import { check, sleep } from "k6";

export let options = {
  stages: [
    { duration: "2m", target: 50 }, // Ramp up to 50 users
    { duration: "5m", target: 50 }, // Stay at 50 users
    { duration: "2m", target: 100 }, // Ramp up to 100 users
    { duration: "5m", target: 100 }, // Stay at 100 users
    { duration: "2m", target: 0 }, // Ramp down
  ],
  thresholds: {
    http_req_duration: ["p(95)<2000"], // 95% of requests under 2s
    http_req_failed: ["rate<0.01"], // Less than 1% errors
  },
};

export default function () {
  // Simulate user workflow
  let loginRes = http.post("https://api.familyhub.com/auth/login", {
    username: "testuser",
    password: "testpass",
  });

  check(loginRes, {
    "login successful": (r) => r.status === 200,
  });

  let token = loginRes.json("access_token");

  let eventsRes = http.get("https://api.familyhub.com/graphql", {
    headers: { Authorization: `Bearer ${token}` },
  });

  check(eventsRes, {
    "events retrieved": (r) => r.status === 200,
  });

  sleep(1);
}
```

**Load Test Schedule:**

- Weekly: Automated smoke tests (10 concurrent users)
- Monthly: Load tests (100 concurrent users)
- Before major releases: Stress tests (2x expected peak)
- Annually: Disaster recovery drills

---

## 9. Disaster Recovery and Business Continuity

### 9.1 Failure Scenarios and Recovery

| Failure Scenario             | Impact                 | RTO      | RPO       | Mitigation                              |
| ---------------------------- | ---------------------- | -------- | --------- | --------------------------------------- |
| **Single pod crash**         | Minimal (auto-restart) | <1 min   | 0         | Kubernetes self-healing                 |
| **Node failure**             | Partial outage         | <5 min   | 0         | Multi-node deployment                   |
| **Database failure**         | Service outage         | <1 hour  | <24 hours | Daily backups, WAL archiving (Phase 5+) |
| **Redis failure**            | Degraded performance   | <15 min  | 0         | Redis Sentinel failover (Phase 3+)      |
| **Complete cluster failure** | Full outage            | <4 hours | <24 hours | Restore from backups to new cluster     |
| **Data center outage**       | Regional outage        | <8 hours | <24 hours | Multi-region deployment (Phase 6+)      |
| **Zitadel outage**           | Login failures         | <30 min  | 0         | Cached tokens, fallback auth (future)   |

### 9.2 Backup and Restore Procedures

**Backup Components:**

```yaml
1. PostgreSQL Database
   - Daily full backups (pg_dump)
   - Hourly WAL archiving (Phase 5+)
   - Retention: 7 days (daily), 4 weeks (weekly), 12 months (monthly)

2. Redis Data
   - RDB snapshots every 15 minutes (ephemeral data, low priority)
   - AOF persistence enabled

3. Kubernetes Configuration
   - GitOps repository (ArgoCD synced)
   - Helm values stored in Git

4. Secrets
   - Sealed secrets in Git
   - Manual backup of Sealed Secrets master key (offline storage)
```

**Restore Procedure (Database):**

```bash
# 1. Identify backup to restore
aws s3 ls s3://familyhub-backups/
# Select: postgres-backup-20251219.dump

# 2. Download backup
aws s3 cp s3://familyhub-backups/postgres-backup-20251219.dump /tmp/

# 3. Create new database
kubectl exec -it postgresql-0 -n family-hub-data -- createdb familyhub_restored

# 4. Restore data
kubectl exec -i postgresql-0 -n family-hub-data -- \
  pg_restore -d familyhub_restored /tmp/postgres-backup-20251219.dump

# 5. Verify data integrity
kubectl exec -it postgresql-0 -n family-hub-data -- \
  psql -d familyhub_restored -c "SELECT COUNT(*) FROM calendar.events;"

# 6. Switch application to restored database
kubectl set env deployment/calendar-service \
  DATABASE_NAME=familyhub_restored -n family-hub

# 7. Monitor application health
kubectl logs -f deployment/calendar-service -n family-hub
```

**Restore Procedure (Full Cluster):**

```bash
# 1. Provision new Kubernetes cluster
# (Cloud provider specific)

# 2. Install core components
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml
kubectl apply -f https://github.com/bitnami-labs/sealed-secrets/releases/download/v0.24.0/controller.yaml

# 3. Deploy ArgoCD
kubectl create namespace argocd
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml

# 4. Connect ArgoCD to Git repository
argocd app create family-hub \
  --repo https://github.com/yourorg/familyhub-k8s \
  --path manifests/production \
  --dest-server https://kubernetes.default.svc \
  --dest-namespace family-hub

# 5. Sync applications
argocd app sync family-hub --prune

# 6. Restore database (see above)

# 7. Update DNS to point to new cluster
# (Cloud provider specific)

# 8. Verify all services healthy
kubectl get pods -n family-hub
curl https://familyhub.yourdomain.com/health
```

### 9.3 Monitoring and Alerting

**Critical Alerts (PagerDuty/Slack):**

- Database down or unreachable
- All replicas of a service are down
- Disk usage >90%
- Error rate >5% for 5 minutes
- API response time >5s p95 for 5 minutes

**Warning Alerts (Email):**

- Pod restart count >10 in 1 hour
- Memory usage >80% sustained for 15 minutes
- Certificate expiring in <7 days
- Backup failure

**Informational (Slack):**

- New deployment completed
- HPA scaling event
- Successful nightly backups

---

## 10. Migration and Deployment Strategy

### 10.1 Deployment Environments

```
┌─────────────────────────────────────────────────────────────┐
│                  Environment Pipeline                        │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Local Dev                                                   │
│  ┌────────────────────────────────────────────────┐         │
│  │ - Docker Compose or Minikube                   │         │
│  │ - Hot reload enabled                            │         │
│  │ - Mock data                                     │         │
│  │ - Developer owns environment                    │         │
│  └────────────────┬───────────────────────────────┘         │
│                   │                                          │
│                   │ git push                                 │
│                   ▼                                          │
│  CI/CD (GitHub Actions)                                      │
│  ┌────────────────────────────────────────────────┐         │
│  │ - Build Docker images                           │         │
│  │ - Run tests (unit, integration)                 │         │
│  │ - Security scanning                             │         │
│  │ - Push images to registry                       │         │
│  └────────────────┬───────────────────────────────┘         │
│                   │                                          │
│                   │ auto-deploy (ArgoCD)                     │
│                   ▼                                          │
│  Dev Environment (Kubernetes)                                │
│  ┌────────────────────────────────────────────────┐         │
│  │ - Latest main branch                            │         │
│  │ - Synthetic test data                           │         │
│  │ - Continuous deployment                         │         │
│  │ - Namespace: family-hub-dev                     │         │
│  └────────────────┬───────────────────────────────┘         │
│                   │                                          │
│                   │ manual promote (git tag)                 │
│                   ▼                                          │
│  Staging Environment (Kubernetes)                            │
│  ┌────────────────────────────────────────────────┐         │
│  │ - Tagged releases                               │         │
│  │ - Production-like data (anonymized)             │         │
│  │ - Manual approval required                      │         │
│  │ - Namespace: family-hub-staging                 │         │
│  │ - Full integration tests                        │         │
│  └────────────────┬───────────────────────────────┘         │
│                   │                                          │
│                   │ manual promote (approval)                │
│                   ▼                                          │
│  Production Environment (Kubernetes)                         │
│  ┌────────────────────────────────────────────────┐         │
│  │ - Stable releases only                          │         │
│  │ - Real user data                                │         │
│  │ - Blue-green deployment                         │         │
│  │ - Namespace: family-hub                         │         │
│  │ - Monitoring and alerting active                │         │
│  └────────────────────────────────────────────────┘         │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 10.2 Deployment Strategies

**Blue-Green Deployment:**

```yaml
# Phase 1: Deploy "green" version alongside "blue"
apiVersion: apps/v1
kind: Deployment
metadata:
  name: calendar-service-green
  labels:
    version: green
spec:
  replicas: 2
  template:
    spec:
      containers:
      - name: calendar
        image: familyhub/calendar-service:v1.2.0

# Phase 2: Test green deployment
kubectl port-forward deployment/calendar-service-green 8080:5002

# Phase 3: Switch traffic to green
kubectl patch service calendar-service -p '{"spec":{"selector":{"version":"green"}}}'

# Phase 4: Monitor for issues (15 minutes)

# Phase 5a: If successful, delete blue
kubectl delete deployment calendar-service-blue

# Phase 5b: If failed, rollback to blue
kubectl patch service calendar-service -p '{"spec":{"selector":{"version":"blue"}}}'
```

**Canary Deployment (ArgoCD):**

```yaml
# Argo Rollouts configuration
apiVersion: argoproj.io/v1alpha1
kind: Rollout
metadata:
  name: calendar-service
spec:
  replicas: 5
  strategy:
    canary:
      steps:
        - setWeight: 20 # Send 20% traffic to new version
        - pause: { duration: 10m } # Monitor for 10 minutes
        - setWeight: 40
        - pause: { duration: 10m }
        - setWeight: 60
        - pause: { duration: 10m }
        - setWeight: 80
        - pause: { duration: 10m }
        - setWeight: 100 # Full rollout
  template:
    spec:
      containers:
        - name: calendar
          image: familyhub/calendar-service:v1.2.0
```

### 10.3 Rollback Procedures

**Automatic Rollback (Health Check Failure):**

```yaml
# Deployment with health checks
apiVersion: apps/v1
kind: Deployment
metadata:
  name: calendar-service
spec:
  progressDeadlineSeconds: 300 # Rollback if not ready in 5 min
  template:
    spec:
      containers:
        - name: calendar
          image: familyhub/calendar-service:v1.2.0
          livenessProbe:
            httpGet:
              path: /health
              port: 5002
            initialDelaySeconds: 30
            periodSeconds: 10
            failureThreshold: 3
          readinessProbe:
            httpGet:
              path: /ready
              port: 5002
            initialDelaySeconds: 10
            periodSeconds: 5
            failureThreshold: 3
```

**Manual Rollback:**

```bash
# View deployment history
kubectl rollout history deployment/calendar-service -n family-hub

# Rollback to previous version
kubectl rollout undo deployment/calendar-service -n family-hub

# Rollback to specific revision
kubectl rollout undo deployment/calendar-service --to-revision=3 -n family-hub

# Monitor rollback progress
kubectl rollout status deployment/calendar-service -n family-hub
```

---

## 11. Architecture Decision Records (ADRs)

### ADR-001: No Service Mesh for Initial Deployment

**Status:** Accepted
**Date:** 2025-12-19
**Decision Maker:** Cloud Architect

**Context:**
Service meshes (Istio, Linkerd) provide advanced traffic management, security, and observability, but add complexity and resource overhead.

**Decision:**
Deploy without service mesh for Phase 0-5, re-evaluate at Phase 6+.

**Rationale:**

- Single developer prefers operational simplicity
- Cost savings: 20-30% lower infrastructure costs
- NGINX Ingress + OpenTelemetry + Polly provides 90% of needed features
- Can add service mesh later without major refactoring

**Consequences:**

- Positive: Lower costs, simpler operations, faster iteration
- Negative: Manual traffic management, no automatic mTLS, less observability

**Alternatives Considered:**

- Istio: Too complex, steep learning curve
- Linkerd: Better than Istio but still adds overhead

---

### ADR-002: Shared Database with Row-Level Security

**Status:** Accepted
**Date:** 2025-12-19
**Decision Maker:** Cloud Architect

**Context:**
Multi-tenancy can be implemented via dedicated databases per tenant, dedicated schemas, or shared database with RLS.

**Decision:**
Use shared PostgreSQL database with Row-Level Security (RLS) for tenant isolation.

**Rationale:**

- Cost: $100/month for 100 tenants vs $10,000/month for dedicated DBs
- Operations: Single database to backup, monitor, upgrade
- PostgreSQL RLS provides strong security guarantees
- Sufficient for family data (not regulated financial/health data requiring dedicated DBs)

**Consequences:**

- Positive: Low cost, simple operations, good performance
- Negative: Risk of "noisy neighbor" (mitigated by query limits), potential data leak if RLS misconfigured

**Migration Path:**

- If needed, shard database by family_group_id at 10,000+ families
- Implement Citus for distributed PostgreSQL

**Alternatives Considered:**

- Dedicated DB per tenant: Too expensive and complex
- Kubernetes namespace per tenant: Massive overhead, not practical

---

### ADR-003: ArgoCD for GitOps

**Status:** Accepted
**Date:** 2025-12-19
**Decision Maker:** Cloud Architect

**Context:**
GitOps tools (ArgoCD, Flux CD) enable declarative, Git-driven deployments.

**Decision:**
Use ArgoCD for GitOps-based continuous deployment.

**Rationale:**

- Excellent UI for single developer (easier than CLI-only Flux)
- Supports multiple sync strategies (auto, manual)
- Strong Helm integration
- Rollback capabilities built-in
- Active community and documentation

**Consequences:**

- Positive: Declarative deployments, easy rollbacks, audit trail in Git
- Negative: Additional component to manage (ArgoCD itself)

**Alternatives Considered:**

- Flux CD: More CLI-focused, less beginner-friendly
- Manual kubectl: Error-prone, no declarative state

---

### ADR-004: Loki + Grafana for Logging

**Status:** Accepted
**Date:** 2025-12-19
**Decision Maker:** Cloud Architect

**Context:**
Centralized logging is essential for troubleshooting distributed systems.

**Decision:**
Use Grafana Loki for log aggregation and Grafana for visualization.

**Rationale:**

- Lightweight: Uses object storage, not indexed search (lower costs than ELK)
- Cloud-agnostic: Works with any S3-compatible storage
- Grafana integration: Unified dashboards for metrics + logs
- Kubernetes-native: LogQL similar to PromQL

**Consequences:**

- Positive: Low resource usage, simple setup, great Kubernetes support
- Negative: Less powerful than Elasticsearch (no full-text search), newer product

**Alternatives Considered:**

- ELK Stack: Too resource-heavy (Elasticsearch requires 2+ GB RAM)
- Seq: .NET-focused but proprietary, less Kubernetes-native

---

## 12. Conclusion and Next Steps

### 12.1 Architecture Summary

Family Hub's cloud architecture is designed for:

1. **Cloud Agnosticism**: Runs on any Kubernetes cluster (DO, Linode, Hetzner, AWS, Azure, GCP, on-premise)
2. **Cost Efficiency**: $150/month for 100 families, $0.50/family/month at 10,000 families
3. **Scalability**: Horizontal scaling for all services, database sharding path defined
4. **Security**: Zero-trust model with TLS, RLS, RBAC, pod security standards
5. **Observability**: Prometheus + Grafana + Loki for comprehensive monitoring
6. **Reliability**: 99.9% uptime target with backup/restore procedures

### 12.2 Implementation Roadmap

**Phase 0 (Week 1-4): Foundation**

- Set up Kubernetes cluster (Minikube local, DigitalOcean cloud)
- Deploy PostgreSQL and Redis
- Configure NGINX Ingress with TLS
- Install Prometheus, Grafana, Loki

**Phase 1 (Week 5-12): Core Services**

- Deploy Auth, Calendar, Task, Communication services
- Configure ArgoCD for GitOps
- Implement HPA for auto-scaling
- Set up monitoring dashboards

**Phase 2 (Week 13-18): Health & Shopping**

- Add Health and Shopping services
- Implement event chains
- Configure backups (S3)

**Phase 3-5 (Week 19-44): Scale and Harden**

- Add remaining services (Meal Planning, Finance)
- Implement PostgreSQL read replicas
- Upgrade to Redis Sentinel
- Security audit and penetration testing

**Phase 6+ (Week 45+): Enterprise Features**

- Multi-region deployment (if needed)
- Database sharding (if needed)
- Advanced observability (traces)

### 12.3 Success Criteria

**Technical Metrics:**

- Deployment time: <5 minutes (GitOps)
- Recovery time: <1 hour (from backups)
- Uptime: >99.5% (Phase 2), >99.9% (Phase 5+)
- API latency: <2s p95 (Phase 2), <1s p95 (Phase 5+)

**Operational Metrics:**

- Infrastructure cost: <$5/family/month at 100 families
- Incident response: <30 minutes to mitigation
- Backup success rate: 100% daily backups
- Security vulnerabilities: 0 critical, <5 high

**Business Metrics:**

- Support 10,000 families without major re-architecture
- Enable 10% premium conversion (cost-effective)
- Scale linearly with user growth

### 12.4 Related Documentation

- [Kubernetes Deployment Guide](/home/andrekirst/git/github/andrekirst/family2/docs/kubernetes-deployment-guide.md)
- [Helm Charts Structure](/home/andrekirst/git/github/andrekirst/family2/docs/helm-charts-structure.md)
- [Observability Stack](/home/andrekirst/git/github/andrekirst/family2/docs/observability-stack.md)
- [CI/CD Pipeline](/home/andrekirst/git/github/andrekirst/family2/docs/cicd-pipeline.md)
- [Multi-Tenancy Strategy](/home/andrekirst/git/github/andrekirst/family2/docs/multi-tenancy-strategy.md)
- [Infrastructure Cost Analysis](/home/andrekirst/git/github/andrekirst/family2/docs/infrastructure-cost-analysis.md)

---

**Document Status:** Architecture Approved
**Next Review:** 2025-03-19 (after Phase 1 completion)
**Maintained By:** Cloud Architect + SRE Team

**Approval Signatures:**

- [ ] Cloud Architect
- [ ] Technical Lead
- [ ] DevOps Engineer
- [ ] Security Lead
