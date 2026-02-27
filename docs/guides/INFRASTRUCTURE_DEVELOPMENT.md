# Infrastructure Development Guide

**Purpose:** Guide for Docker Compose, Kubernetes deployment, CI/CD pipelines, and infrastructure management in Family Hub.

**Tech Stack:** Docker Compose, Kubernetes (K8s), GitHub Actions, PostgreSQL 16, RabbitMQ, Redis, Zitadel, Seq, MailHog

---

## Quick Reference

### Local Development Stack

**Services (Docker Compose):**

- **PostgreSQL 16** - Primary database (port 5432)
- **RabbitMQ 3.12** - Message broker (ports 5672, 15672)
- **Redis 7 Alpine** - Real-time subscriptions (port 6379)
- **Zitadel v2.47.0** - OAuth 2.0 provider (port 8080)
- **Seq** - Structured logging (port 5341)
- **MailHog** - Email testing (ports 1025, 8025)

**Configuration:** `infrastructure/docker/docker-compose.yml`

---

## Critical Patterns (3)

### 1. Docker Compose (Local Development)

**Start Infrastructure:**

```bash
cd infrastructure/docker
docker-compose up -d

# Verify all services healthy
docker-compose ps

# View logs
docker-compose logs -f <service-name>

# Stop all services
docker-compose down

# Restart single service
docker-compose restart <service-name>
```

**Environment Variables (.env file):**

```bash
# Create .env file in infrastructure/docker/
POSTGRES_PASSWORD=Dev123!
RABBITMQ_PASSWORD=Dev123!
ZITADEL_MASTERKEY=MasterKey123!MustBe32Characters!
ZITADEL_DB_PASSWORD=Dev123!
ZITADEL_ADMIN_PASSWORD=Admin123!

# SMTP (optional, defaults to MailHog)
SMTP_HOST=mailhog
SMTP_PORT=1025
SMTP_FROM_ADDRESS=noreply@familyhub.local
```

**Service URLs:**

- Backend API: `http://localhost:7000/graphql`
- Frontend: `http://localhost:4200`
- Zitadel: `http://localhost:8080`
- RabbitMQ Management: `http://localhost:15672` (familyhub/Dev123!)
- Seq Logging: `http://localhost:5341`
- MailHog UI: `http://localhost:8025`
- Redis: `localhost:6379` (no UI, use redis-cli)

**Health Checks:**

```bash
# PostgreSQL
docker exec familyhub-postgres pg_isready -U familyhub

# RabbitMQ
docker exec familyhub-rabbitmq rabbitmq-diagnostics ping

# Redis
docker exec familyhub-redis redis-cli ping
# Expected: PONG

# Backend Health Endpoint (includes Redis check)
curl http://localhost:7000/health/redis

# Check all containers
docker-compose ps
```

---

### 2. Kubernetes Deployment (Phase 5+)

**Planned architecture** for microservices phase (Phase 5).

**Namespace Organization:**

```yaml
# Development
kubectl create namespace familyhub-dev

# Staging
kubectl create namespace familyhub-staging

# Production
kubectl create namespace familyhub-prod
```

**Deployment Pattern (per microservice):**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: auth-service
  namespace: familyhub-prod
spec:
  replicas: 3
  selector:
    matchLabels:
      app: auth-service
  template:
    metadata:
      labels:
        app: auth-service
    spec:
      containers:
      - name: auth-service
        image: familyhub/auth-service:latest
        ports:
        - containerPort: 8080
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: postgres-credentials
              key: connection-string
        - name: RabbitMQ__Host
          value: "rabbitmq-service"
        resources:
          requests:
            memory: "256Mi"
            cpu: "100m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

**Service Pattern:**

```yaml
apiVersion: v1
kind: Service
metadata:
  name: auth-service
  namespace: familyhub-prod
spec:
  selector:
    app: auth-service
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: ClusterIP
```

**Ingress (API Gateway):**

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: familyhub-ingress
  namespace: familyhub-prod
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
spec:
  ingressClassName: nginx
  tls:
  - hosts:
    - api.familyhub.com
    secretName: familyhub-tls
  rules:
  - host: api.familyhub.com
    http:
      paths:
      - path: /auth
        pathType: Prefix
        backend:
          service:
            name: auth-service
            port:
              number: 80
```

**See:** `infrastructure/k8s/` for full manifests (created in Phase 5).

---

### 3. CI/CD Pipeline (GitHub Actions)

**Workflow:** `.github/workflows/ci-cd.yml`

**Pipeline Stages:**

1. **Build & Test** (on PR)
   - Checkout code
   - Setup .NET & Node
   - Restore dependencies
   - Build backend & frontend
   - Run unit tests
   - Run integration tests
   - Run E2E tests (Playwright)

2. **Docker Build** (on merge to main)
   - Build Docker images
   - Tag with commit SHA
   - Push to registry (Docker Hub / Azure ACR)

3. **Deploy** (manual approval)
   - Deploy to staging
   - Run smoke tests
   - Deploy to production (if staging passes)

**Example CI workflow:**

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'

    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '20'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Unit Tests
      run: dotnet test --no-build --verbosity normal

    - name: E2E Tests
      run: |
        cd src/frontend/family-hub-web
        npm ci
        npx playwright install
        npm run e2e:headless
```

**Secrets Configuration (GitHub Settings):**

- `DOCKER_USERNAME`
- `DOCKER_PASSWORD`
- `AZURE_CREDENTIALS`
- `KUBECONFIG`

---

## Common Infrastructure Tasks

### Add New Service to Docker Compose

1. Add service definition in `docker-compose.yml`
2. Configure ports, volumes, environment variables
3. Add to `familyhub-network`
4. Add health check
5. Restart: `docker-compose up -d`

### Update Service Version

```bash
# Edit docker-compose.yml (change image version)
# Restart service
docker-compose up -d <service-name>

# Verify new version
docker-compose ps
docker-compose logs <service-name>
```

### Reset Local Environment

```bash
# Stop all services
docker-compose down

# Remove volumes (CAUTION: deletes all data)
docker-compose down -v

# Remove images
docker-compose down --rmi all

# Fresh start
docker-compose up -d
```

### View Service Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f postgres
docker-compose logs -f rabbitmq
docker-compose logs -f zitadel

# Last N lines
docker-compose logs --tail=100 postgres
```

---

## Observability

### Structured Logging (Seq)

**Access:** `http://localhost:5341`

**Backend logging:**

```csharp
// Program.cs
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.Seq("http://localhost:5341"));

// In code
_logger.LogInformation("User {UserId} created family {FamilyId}",
    userId, familyId);
```

**Query logs:**

```
# In Seq UI
UserId = @UserId
and Level = 'Error'
```

### Metrics (Future - Phase 5+)

Planned: Prometheus + Grafana

- Application metrics (request count, latency, errors)
- Database metrics (query performance, connections)
- RabbitMQ metrics (queue depth, message rate)

### Tracing (Future - Phase 5+)

Planned: OpenTelemetry + Jaeger

- Distributed tracing across microservices
- Request flow visualization
- Performance bottleneck identification

---

## Debugging Infrastructure Issues

### Docker Issues

```bash
# Service not starting
docker-compose ps
docker-compose logs <service-name>

# Port conflicts
lsof -i :<port>
kill <pid>

# Disk space
docker system df
docker system prune -a

# Network issues
docker network ls
docker network inspect familyhub-network
```

### Database Connection

```bash
# Test from host
psql -h localhost -U familyhub -d familyhub

# Test from container
docker exec -it familyhub-postgres psql -U familyhub -d familyhub

# Check connection string
echo $ConnectionStrings__DefaultConnection
```

### RabbitMQ Issues

```bash
# Check management UI
open http://localhost:15672

# Check queues
docker exec familyhub-rabbitmq rabbitmqctl list_queues

# Reset RabbitMQ
docker-compose restart rabbitmq
```

### Redis Issues

```bash
# Test connection
docker exec familyhub-redis redis-cli ping
# Expected: PONG

# Monitor real-time activity
docker exec -it familyhub-redis redis-cli MONITOR
# Shows all commands in real-time

# Check subscription messages
docker exec -it familyhub-redis redis-cli
> PUBSUB CHANNELS family-*
> PUBSUB NUMSUB family-members-changed:123e4567-e89b-12d3-a456-426614174000

# View Redis logs
docker-compose logs -f redis

# Reset Redis (clears all data)
docker exec familyhub-redis redis-cli FLUSHALL

# Restart Redis
docker-compose restart redis
```

**See:** [docs/development/DEBUGGING_GUIDE.md](../docs/development/DEBUGGING_GUIDE.md#infrastructure-issues)

---

## Docker Swarm Deployment (Turing Pi 2)

### Target Hardware

Turing Pi 2 board with 2x RK1 modules (Rockchip RK3588, ARM64, 32GB RAM each) running Docker Swarm.

### Architecture

Three stack layers:

- **Base stack** (`fh-base`) — Traefik v3.2 reverse proxy with Swarm-mode service discovery
- **Staging stack** (`fh-staging`) — Full environment auto-deployed on push to `main`
- **Production stack** (`fh-production`) — Full environment deployed via manual trigger

Both staging and production run on the same Swarm cluster with separate overlay networks for isolation.

### Container Images

Built natively on ARM64 self-hosted runner (no QEMU emulation):

| Image | Registry | Stage | Purpose |
|-------|----------|-------|---------|
| `api` | `ghcr.io/andrekirst/family2/api` | `final` | ASP.NET runtime (~200MB) |
| `api-migrate` | `ghcr.io/andrekirst/family2/api-migrate` | `migrate` | SDK + dotnet-ef (~800MB) |
| `frontend` | `ghcr.io/andrekirst/family2/frontend` | `prod` | nginx + Angular dist |

Tag convention: `sha-<full-commit-sha>`

### CI/CD Pipeline

```
PR to main  ──►  ci.yml (GitHub-hosted)
                   └── Backend: restore → build → test
                   └── Frontend: install → build

Push to main ──►  deploy-staging.yml (GitHub-hosted + self-hosted)
                   └── test (GitHub-hosted)
                   └── build-images (self-hosted ARM64) → ghcr.io
                   └── deploy-staging (self-hosted ARM64) → Swarm

Manual       ──►  deploy-production.yml (self-hosted)
                   └── validate confirmation
                   └── deploy-production → Swarm (requires reviewer approval)
```

### Swarm Stack Files

```
infrastructure/swarm/
├── traefik/
│   └── traefik-swarm.yml           # Traefik static config (Swarm mode, ports 80/443)
├── docker-stack.base.yml           # Base: Traefik reverse proxy (deploy once)
├── docker-stack.staging.yml        # Staging: PostgreSQL, Keycloak, MailHog, API, Frontend
└── docker-stack.production.yml     # Production: PostgreSQL, Keycloak, API (2x), Frontend (2x)
```

### Key Swarm Patterns

**Placement constraints:** Stateful services (PostgreSQL, Keycloak) pinned to nodes with `node.labels.storage==true` to keep data volumes on a specific node.

**Rolling updates:** `update_config.order: start-first` ensures new containers are healthy before old ones are removed (zero-downtime for API and Frontend).

**Automatic rollback:** `rollback_config` enables Swarm to automatically revert to the previous version if the new deployment fails health checks.

**Secrets:** Database passwords stored as Docker Swarm secrets, mounted at `/run/secrets/<name>`. PostgreSQL reads via `POSTGRES_PASSWORD_FILE`; Keycloak receives passwords as environment variables (doesn't support `_FILE`).

### Infrastructure Scripts

| Script | Purpose |
|--------|---------|
| `setup-swarm-secrets.sh` | Create Docker Swarm secrets for staging + production |
| `setup-runner.sh` | Deploy GitHub Actions self-hosted runner as Swarm service |
| `deploy.sh` | Manual deployment helper (migrations → stack deploy → health check) |
| `provision-swarm-realm.sh` | Provision Keycloak realm for Swarm environments |

### GitHub Configuration Required

**Repository Settings → Environments:**

- `staging` — No protection rules (auto-deploy)
- `production` — Required reviewers (manual approval)

**Repository Settings → Secrets & Variables → Actions:**

- Per-environment: `{STAGING,PROD}_DB_PASSWORD`, `{STAGING,PROD}_KC_DB_PASSWORD`, `{STAGING,PROD}_KC_ADMIN_PASSWORD`

---

## Related Documentation

- **Local Setup:** [docs/development/LOCAL_DEVELOPMENT_SETUP.md](../docs/development/LOCAL_DEVELOPMENT_SETUP.md) - Complete setup guide
- **Debugging:** [docs/development/DEBUGGING_GUIDE.md](../docs/development/DEBUGGING_GUIDE.md) - Troubleshooting
- **Architecture:** [docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md](../docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) - Deployment strategy
- **Swarm README:** [infrastructure/README.md](../../infrastructure/README.md) - Quick start for Swarm deployment

---

**Last Updated:** 2026-02-26
**Derived from:** Root CLAUDE.md v9.0.0
**Canonical Sources:**

- infrastructure/docker/docker-compose.yml (Service configuration)
- infrastructure/swarm/ (Swarm stack files)
- .github/workflows/ (CI/CD pipelines)
- docs/development/LOCAL_DEVELOPMENT_SETUP.md (Setup instructions)
- docs/development/DEBUGGING_GUIDE.md (Troubleshooting)

**Recent Changes:**

- 2026-02-26: Added Docker Swarm deployment and CI/CD pipelines (#196)
- 2026-01-14: Added Redis 7 Alpine for GraphQL subscriptions (#84)

**Sync Checklist:**

- [x] Docker Compose services match docker-compose.yml
- [x] Environment variables match LOCAL_DEVELOPMENT_SETUP.md
- [x] Port mappings match documented URLs
- [x] Health checks accurate
- [x] Swarm stack files documented
- [x] CI/CD workflows documented
