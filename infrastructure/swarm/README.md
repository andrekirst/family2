# Docker Swarm Deployment Guide

Deploy the full FamilyHub application (API, Frontend, PostgreSQL, Keycloak) to a Turing Pi 2 board running Docker Swarm, with separate staging and production environments and automated CI/CD via GitHub Actions.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Prerequisites](#prerequisites)
3. [Initial Swarm Setup](#initial-swarm-setup)
4. [GitHub Configuration](#github-configuration)
5. [First Deployment](#first-deployment)
6. [CI/CD Pipeline](#cicd-pipeline)
7. [Manual Deployment](#manual-deployment)
8. [Keycloak Realm Provisioning](#keycloak-realm-provisioning)
9. [Day-to-Day Operations](#day-to-day-operations)
10. [URL Reference](#url-reference)
11. [File Reference](#file-reference)
12. [Troubleshooting](#troubleshooting)
13. [Known Limitations](#known-limitations)

---

## Architecture Overview

### Hardware

**Turing Pi 2** with 2x RK1 modules:

- Rockchip RK3588, ARM64/aarch64
- 32 GB RAM each
- Ubuntu/Debian
- Docker Swarm initialized (one manager, one worker)

### Stack Layout

Three Docker Swarm stacks run on the cluster:

```
┌─────────────────────────────────────────────────────────────────────┐
│                     Turing Pi 2 — Docker Swarm                      │
│                                                                     │
│  Base Stack (fh-base)                                               │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │ Traefik v3.2 (ports 80, 443, 8080)                            │ │
│  │ Overlay network: traefik-public                                │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                     │
│  Staging Stack (fh-staging)          Production Stack (fh-production)│
│  ┌──────────────────────────┐       ┌──────────────────────────┐   │
│  │ PostgreSQL        pinned │       │ PostgreSQL        pinned │   │
│  │ Keycloak DB       pinned │       │ Keycloak DB       pinned │   │
│  │ Keycloak          pinned │       │ Keycloak          pinned │   │
│  │ MailHog                  │       │ API            2 replicas │   │
│  │ API            1 replica │       │ Frontend       2 replicas │   │
│  │ Frontend       1 replica │       └──────────────────────────┘   │
│  └──────────────────────────┘                                       │
└─────────────────────────────────────────────────────────────────────┘
```

- **Base stack** — Traefik reverse proxy shared by both environments
- **Staging** — Auto-deployed on every push to `main`; includes MailHog for email testing
- **Production** — Manually triggered; 2 replicas for API/Frontend (HA across both nodes); real SMTP

### Container Images

Built natively on ARM64 (no QEMU emulation):

| Image | Dockerfile Stage | Size | Purpose |
|-------|-----------------|------|---------|
| `ghcr.io/andrekirst/family2/api` | `final` | ~200 MB | ASP.NET runtime |
| `ghcr.io/andrekirst/family2/api-migrate` | `migrate` | ~800 MB | SDK + dotnet-ef (one-shot migration) |
| `ghcr.io/andrekirst/family2/frontend` | `prod` | ~50 MB | nginx serving Angular SPA |

Tag convention: `sha-<full-commit-sha>` (also tagged `latest`)

### Networking

Each environment uses an isolated overlay network for internal communication:

- `traefik-public` — Shared external network (Traefik discovers services here)
- `staging-internal` — Staging PostgreSQL, Keycloak, API (not exposed externally)
- `prod-internal` — Production PostgreSQL, Keycloak, API (not exposed externally)

Traefik routes incoming HTTP requests to the correct environment based on the `Host` header.

---

## Prerequisites

### On the Turing Pi 2 nodes

| Requirement | Notes |
|------------|-------|
| Docker Engine 24+ | With Docker Swarm initialized |
| Docker Compose v2 | For local testing of stack files (optional) |
| `curl` | Used by health check scripts |
| `jq` | Used by Keycloak provisioning script |
| Internet access | To pull images from ghcr.io |

### Docker Swarm initialization

If Swarm is not yet initialized:

```bash
# On the manager node (node 1)
docker swarm init --advertise-addr <manager-ip>

# On the worker node (node 2) — use the join token from the output above
docker swarm join --token <token> <manager-ip>:2377
```

Verify:

```bash
docker node ls
# Should show 2 nodes, one as Leader
```

### On GitHub

- Repository: `andrekirst/family2`
- Permissions to create environments, secrets, and configure runners

### On your development machine

- Git access to the repository
- Access to push to `main` branch

---

## Initial Swarm Setup

Run these steps **once** on the Swarm manager node.

### Quick Install (recommended)

On a fresh Swarm manager node — no clone needed:

```bash
curl -sSL https://raw.githubusercontent.com/andrekirst/family2/main/infrastructure/scripts/bootstrap-swarm.sh | bash
```

This clones the repo to `/opt/familyhub` and runs the interactive installer. Customize with environment variables:

```bash
# Custom install directory and branch
curl -sSL https://raw.githubusercontent.com/andrekirst/family2/main/infrastructure/scripts/bootstrap-swarm.sh \
  | INSTALL_DIR=/home/admin/familyhub BRANCH=main bash
```

If you already have the repo cloned, run the installer directly:

```bash
bash infrastructure/scripts/install-swarm.sh
```

The installer checks prerequisites, labels the storage node, creates the network and secrets, deploys Traefik and the GitHub runner, and optionally deploys staging with Keycloak realm provisioning. Each step can be skipped and the script is safe to re-run.

### Manual Setup (step-by-step)

### Step 1: Label the storage node

Stateful services (PostgreSQL, Keycloak) need persistent volumes. Pin them to a specific node so volumes are always on the same disk:

```bash
# Find node names
docker node ls

# Label the node that will host databases
docker node update --label-add storage=true <node-name>
```

### Step 2: Create the overlay network

The shared `traefik-public` network connects Traefik to services across all stacks:

```bash
docker network create --driver overlay --attachable traefik-public
```

### Step 3: Create Swarm secrets

Database and Keycloak admin passwords are stored as Docker Swarm secrets (encrypted at rest, mounted as files inside containers):

```bash
# Clone the repo on the manager node (or copy the scripts)
cd infrastructure/scripts

# Interactive — prompts for each password
bash setup-swarm-secrets.sh
```

This creates 6 secrets:

| Secret | Used by |
|--------|---------|
| `staging_db_password` | Staging PostgreSQL |
| `staging_kc_db_password` | Staging Keycloak DB |
| `staging_kc_admin_password` | Staging Keycloak admin |
| `prod_db_password` | Production PostgreSQL |
| `prod_kc_db_password` | Production Keycloak DB |
| `prod_kc_admin_password` | Production Keycloak admin |

Verify:

```bash
docker secret ls
```

> **Note:** Swarm secrets are immutable. To change a password, remove and recreate the secret, then redeploy the affected stack.

### Step 4: Deploy the base stack (Traefik)

```bash
cd infrastructure/swarm
docker stack deploy -c docker-stack.base.yml fh-base
```

Verify:

```bash
# Check service is running
docker stack services fh-base

# Traefik dashboard should be accessible
curl -s http://localhost:8080/api/overview | jq .
```

### Step 5: Configure `/etc/hosts` on client machines

Since we use `.familyhub.local` hostnames (no real DNS), add entries on every machine that needs to access the services:

```bash
# Add to /etc/hosts (replace <manager-ip> with the actual IP)
<manager-ip> staging.familyhub.local api-staging.familyhub.local auth-staging.familyhub.local mail-staging.familyhub.local
<manager-ip> app.familyhub.local api.familyhub.local auth.familyhub.local traefik.familyhub.local
```

---

## GitHub Configuration

### Step 1: Create environments

Go to **Repository Settings → Environments** and create:

| Environment | Protection Rules |
|-------------|-----------------|
| `staging` | None (auto-deploy) |
| `production` | Required reviewers (add yourself or team members) |

### Step 2: Add secrets

Go to **Repository Settings → Secrets and variables → Actions** and add these secrets per environment:

**Staging environment secrets:**

| Secret | Description |
|--------|-------------|
| `STAGING_DB_PASSWORD` | Same value used in `setup-swarm-secrets.sh` for staging PostgreSQL |
| `STAGING_KC_DB_PASSWORD` | Same value for staging Keycloak DB |
| `STAGING_KC_ADMIN_PASSWORD` | Same value for staging Keycloak admin |

**Production environment secrets:**

| Secret | Description |
|--------|-------------|
| `PROD_DB_PASSWORD` | Same value used for production PostgreSQL |
| `PROD_KC_DB_PASSWORD` | Same value for production Keycloak DB |
| `PROD_KC_ADMIN_PASSWORD` | Same value for production Keycloak admin |

> **Important:** The GitHub secrets must match the Swarm secrets exactly. They are used by the deploy workflow to pass connection strings to the migration container and inject environment variables into the stack deployment.

### Step 3: Deploy the self-hosted runner

The runner builds ARM64-native Docker images and deploys stacks. It runs as a Swarm service on the manager node with access to the Docker socket.

1. Go to **Repository Settings → Actions → Runners → New self-hosted runner**
2. Copy the registration token
3. On the Swarm manager node:

```bash
RUNNER_TOKEN=<token-from-github> bash infrastructure/scripts/setup-runner.sh
```

Verify:

```bash
# Check service is running
docker service ps github-runner

# Check logs
docker service logs github-runner

# Should appear in GitHub → Settings → Actions → Runners as "online"
```

> **Runner labels:** The runner registers with labels `self-hosted,linux,arm64`. The deploy workflows use `runs-on: [self-hosted, linux, arm64]` to target it.

---

## First Deployment

After completing the initial setup and GitHub configuration:

### Option A: Automated (recommended)

1. Push or merge a change to `main`
2. The `deploy-staging.yml` workflow triggers automatically:
   - Tests run on GitHub-hosted runner
   - Images build on the self-hosted ARM64 runner
   - Images push to ghcr.io
   - Stack deploys to Swarm
   - Migrations run
   - Health check verifies the deployment
3. Monitor at: **Repository → Actions → Deploy Staging**

### Option B: Manual

```bash
# On the Swarm manager node

# 1. Log in to GitHub Container Registry
echo "<github-pat>" | docker login ghcr.io -u <github-username> --password-stdin

# 2. Build images locally (from repo root)
cd src
docker build --target final -t ghcr.io/andrekirst/family2/api:latest -f FamilyHub.Api/Dockerfile .
docker build --target migrate -t ghcr.io/andrekirst/family2/api-migrate:latest -f FamilyHub.Api/Dockerfile .
cd frontend/family-hub-web
docker build --target prod --build-arg NPM_REGISTRY=https://registry.npmjs.org/ -t ghcr.io/andrekirst/family2/frontend:latest -f Dockerfile .

# 3. Deploy using the helper script
export STAGING_DB_PASSWORD=<your-staging-db-password>
bash infrastructure/scripts/deploy.sh staging latest
```

### Post-deployment: Provision Keycloak realm

After the first deployment, Keycloak is running but has no realm configured:

```bash
KC_ADMIN_PASS=<staging-kc-admin-password> bash infrastructure/scripts/provision-swarm-realm.sh staging
```

See [Keycloak Realm Provisioning](#keycloak-realm-provisioning) for details.

### Verification checklist

After the first deployment, verify each service:

```bash
# 1. All services running
docker stack services fh-staging

# 2. API health
curl http://staging.familyhub.local/health
# Expected: 200 OK

# 3. Frontend loads
curl -s http://staging.familyhub.local | head -5
# Expected: HTML with <app-root>

# 4. GraphQL endpoint
curl -s http://staging.familyhub.local/graphql?query={__typename}
# Expected: {"data":{"__typename":"Query"}}

# 5. Keycloak
curl -s http://auth-staging.familyhub.local/realms/FamilyHub-staging/.well-known/openid-configuration | jq .issuer
# Expected: "http://auth-staging.familyhub.local/realms/FamilyHub-staging"

# 6. MailHog (staging only)
curl -s http://mail-staging.familyhub.local
# Expected: 200 OK (MailHog UI)
```

---

## CI/CD Pipeline

### Overview

```
                   ┌──────────────────────────────────────┐
  PR to main  ───► │ ci.yml                               │
                   │  Backend: restore → build → test      │
                   │  Frontend: install → build             │
                   │  Runner: GitHub-hosted (ubuntu-latest) │
                   └──────────────────────────────────────┘

                   ┌──────────────────────────────────────┐
  Push to     ───► │ deploy-staging.yml                    │
  main             │  1. test (GitHub-hosted)              │
                   │  2. build-images (self-hosted ARM64)  │
                   │     → push to ghcr.io                 │
                   │  3. deploy-staging (self-hosted ARM64)│
                   │     → migrations → stack deploy       │
                   └──────────────────────────────────────┘

                   ┌──────────────────────────────────────┐
  Manual      ───► │ deploy-production.yml                 │
  dispatch         │  1. validate confirmation ("deploy")  │
                   │  2. deploy-production (self-hosted)   │
                   │     → pull images → migrations        │
                   │     → stack deploy → health check     │
                   │  Requires: environment approval       │
                   └──────────────────────────────────────┘
```

### CI Workflow (`ci.yml`)

**Trigger:** Pull request to `main`
**Runner:** GitHub-hosted `ubuntu-latest`

Runs backend build + tests (`.NET 10, FamilyHub.slnx`) and frontend build (`Angular, npm ci + ng build`). Frontend uses `--registry https://registry.npmjs.org/` to bypass the local Verdaccio cache.

### Staging Deploy Workflow (`deploy-staging.yml`)

**Trigger:** Push to `main`
**Runner:** GitHub-hosted (test job) + self-hosted ARM64 (build + deploy jobs)

1. **test** — Full backend test suite on GitHub-hosted runner
2. **build-images** — Build three ARM64 Docker images, push to ghcr.io with `sha-<commit>` and `latest` tags
3. **deploy-staging** — Deploy stack, wait for DB, run migrations, health check

### Production Deploy Workflow (`deploy-production.yml`)

**Trigger:** Manual `workflow_dispatch` from GitHub Actions UI
**Inputs:**

- `image_tag` — The image tag to deploy (e.g., `sha-abc123...`)
- `confirm` — Must type `"deploy"` to confirm

**Protection:** Uses the `production` GitHub environment which requires reviewer approval.

**How to deploy to production:**

1. Go to **Actions → Deploy Production → Run workflow**
2. Enter the `image_tag` (find it from the staging deploy run or from ghcr.io packages)
3. Type `deploy` in the confirmation field
4. Click **Run workflow**
5. Approve when prompted (production environment protection)

---

## Manual Deployment

The `deploy.sh` helper script handles the full deployment sequence:

```bash
# Deploy to staging
export STAGING_DB_PASSWORD=<password>
bash infrastructure/scripts/deploy.sh staging <image-tag>

# Deploy to production
export PROD_DB_PASSWORD=<password>
bash infrastructure/scripts/deploy.sh production <image-tag>
```

The script performs three steps:

1. **Deploy stack** — `docker stack deploy` with the specified image tag
2. **Run migrations** — Starts a one-shot `api-migrate` container on the stack's internal network
3. **Health check** — Polls the health endpoint for up to 5 minutes

Use `latest` as the image tag to deploy the most recently built images.

---

## Keycloak Realm Provisioning

Each environment needs a Keycloak realm configured with the FamilyHub OAuth clients and test users.

### Create a realm

```bash
# Staging
KC_ADMIN_PASS=<staging-kc-admin-password> \
  bash infrastructure/scripts/provision-swarm-realm.sh staging

# Production
KC_ADMIN_PASS=<prod-kc-admin-password> \
  bash infrastructure/scripts/provision-swarm-realm.sh production
```

This creates:

- Realm `FamilyHub-staging` or `FamilyHub-production`
- OAuth clients: `familyhub-api` (backend), `familyhub-web` (frontend SPA with PKCE)
- Redirect URIs matching the environment's frontend hostname
- Test users: `testowner` / `test123` and `testmember` / `test123`
- Roles: `family-owner`, `family-admin`, `family-member`, `family-child`

### Delete a realm

```bash
KC_ADMIN_PASS=<password> \
  bash infrastructure/scripts/provision-swarm-realm.sh staging --delete
```

### Override Keycloak URL

By default the script auto-detects the Keycloak URL based on the environment name. Override with:

```bash
KC_URL=http://<custom-url> KC_ADMIN_PASS=<password> \
  bash infrastructure/scripts/provision-swarm-realm.sh staging
```

---

## Day-to-Day Operations

### View stack services

```bash
# List all services in a stack
docker stack services fh-staging
docker stack services fh-production
docker stack services fh-base

# Detailed task list (shows which node each container runs on)
docker stack ps fh-staging
docker stack ps fh-production
```

### View logs

```bash
# Follow logs for a specific service
docker service logs -f fh-staging_api
docker service logs -f fh-staging_frontend
docker service logs -f fh-staging_keycloak
docker service logs -f fh-staging_postgres

# Last 100 lines
docker service logs --tail 100 fh-staging_api

# Timestamps
docker service logs -t fh-staging_api
```

### Scale services

```bash
# Scale API to 3 replicas
docker service scale fh-production_api=3

# Scale back to 2
docker service scale fh-production_api=2
```

### Force a service update (restart)

```bash
# Rolling restart (zero-downtime)
docker service update --force fh-staging_api
```

### Update a single service image

```bash
# Update just the API to a new image
docker service update --image ghcr.io/andrekirst/family2/api:sha-newcommit fh-staging_api
```

### Remove a stack

```bash
# Remove staging (keeps volumes and secrets)
docker stack rm fh-staging

# Remove production
docker stack rm fh-production
```

> **Warning:** Removing a stack does NOT remove volumes. Data persists. To delete data, manually remove volumes with `docker volume rm`.

### Inspect secrets

```bash
# List all secrets
docker secret ls

# Secrets cannot be read once created — only inspected for metadata
docker secret inspect staging_db_password
```

### Rotate a secret

Swarm secrets are immutable. To change a password:

```bash
# 1. Remove the old secret (must remove from all services first)
docker stack rm fh-staging
docker secret rm staging_db_password

# 2. Create new secret
echo -n "new-password" | docker secret create staging_db_password -

# 3. Redeploy
docker stack deploy -c docker-stack.staging.yml --with-registry-auth fh-staging
```

### Monitor node health

```bash
docker node ls
docker node inspect <node-name> --pretty
docker system df        # Disk usage
docker system prune -a  # Clean up unused images/containers (careful!)
```

---

## URL Reference

### Staging

| Service | URL |
|---------|-----|
| Frontend | `http://staging.familyhub.local` |
| API (direct) | `http://api-staging.familyhub.local` |
| API (same-origin) | `http://staging.familyhub.local/graphql` |
| Health check | `http://staging.familyhub.local/health` |
| Frontend config | `http://staging.familyhub.local/config` |
| Keycloak | `http://auth-staging.familyhub.local` |
| Keycloak admin | `http://auth-staging.familyhub.local/admin/master/console` |
| MailHog | `http://mail-staging.familyhub.local` |

### Production

| Service | URL |
|---------|-----|
| Frontend | `http://app.familyhub.local` |
| API (direct) | `http://api.familyhub.local` |
| API (same-origin) | `http://app.familyhub.local/graphql` |
| Health check | `http://app.familyhub.local/health` |
| Frontend config | `http://app.familyhub.local/config` |
| Keycloak | `http://auth.familyhub.local` |
| Keycloak admin | `http://auth.familyhub.local/admin/master/console` |

### Shared

| Service | URL |
|---------|-----|
| Traefik dashboard | `http://<manager-ip>:8080` |

### `/etc/hosts` entry

```
<manager-ip> staging.familyhub.local api-staging.familyhub.local auth-staging.familyhub.local mail-staging.familyhub.local app.familyhub.local api.familyhub.local auth.familyhub.local traefik.familyhub.local
```

---

## File Reference

```
infrastructure/
├── swarm/
│   ├── README.md                      # This file
│   ├── traefik/
│   │   └── traefik-swarm.yml          # Traefik static config (Swarm mode)
│   ├── docker-stack.base.yml          # Base stack: Traefik reverse proxy
│   ├── docker-stack.staging.yml       # Staging: full environment
│   └── docker-stack.production.yml    # Production: full environment
├── keycloak/
│   ├── realm-base.json                # Realm template (local dev)
│   ├── realm-swarm.json               # Realm template (Swarm environments)
│   └── provision-realm.sh             # Realm provisioning (local dev)
├── scripts/
│   ├── setup-swarm-secrets.sh         # Create Swarm secrets (one-time)
│   ├── setup-runner.sh                # Deploy GitHub Actions runner (one-time)
│   ├── deploy.sh                      # Manual deployment helper
│   ├── provision-swarm-realm.sh       # Keycloak realm for Swarm
│   └── health-check.sh               # Health check (local dev)
.github/
└── workflows/
    ├── ci.yml                         # CI: build + test on PRs
    ├── deploy-staging.yml             # Auto-deploy staging on push to main
    └── deploy-production.yml          # Manual production deploy
src/
├── FamilyHub.Api/
│   ├── Dockerfile                     # dev → build → migrate → final
│   ├── appsettings.Staging.json       # Staging log levels
│   └── appsettings.Production.json    # Production log levels
└── frontend/family-hub-web/
    ├── Dockerfile                     # dev → build → prod
    └── nginx.conf                     # SPA routing, gzip, health endpoint
```

---

## Troubleshooting

### Service not starting

```bash
# Check service status
docker service ps fh-staging_api --no-trunc

# Look for error messages in the task list
docker service ps fh-staging_api --format "{{.Error}}" --no-trunc

# Check logs
docker service logs fh-staging_api --tail 50
```

### Container stuck in "Pending" state

Usually a placement constraint issue:

```bash
# Check node labels
docker node inspect <node-name> --format '{{.Spec.Labels}}'

# Ensure storage label is set
docker node update --label-add storage=true <node-name>
```

### Health check failing

```bash
# Test from the manager node
curl -v http://staging.familyhub.local/health

# Test API directly (bypassing Traefik)
docker exec $(docker ps -q -f name=fh-staging_api) wget -qO- http://localhost:5152/health

# Check Traefik routing
curl http://localhost:8080/api/http/routers | jq '.[] | select(.name | contains("staging"))'
```

### Database connection issues

```bash
# Check PostgreSQL is healthy
docker service logs fh-staging_postgres --tail 20

# Verify secret is mounted correctly
docker exec $(docker ps -q -f name=fh-staging_postgres) cat /run/secrets/staging_db_password

# Test connection from API container
docker exec $(docker ps -q -f name=fh-staging_api) sh -c \
  'wget -qO- "http://localhost:5152/health"'
```

### Migration fails

```bash
# Run migration manually with verbose output
docker run --rm -it \
  --network fh-staging_staging-internal \
  -e "ConnectionStrings__DefaultConnection=Host=fh-staging_postgres;Port=5432;Database=familyhub;Username=familyhub;Password=<password>" \
  ghcr.io/andrekirst/family2/api-migrate:latest \
  dotnet ef database update \
    --project FamilyHub.Api/FamilyHub.Api.csproj \
    --no-build \
    --verbose
```

### Keycloak slow to start

Keycloak can take 2+ minutes on first startup (database schema creation). The health check has a 120s `start_period` to accommodate this:

```bash
# Watch Keycloak startup
docker service logs -f fh-staging_keycloak
```

### Realm provisioning fails

```bash
# Verify Keycloak is ready
curl http://auth-staging.familyhub.local/realms/master

# Check the provisioning script with debug output
KC_ADMIN_PASS=<password> bash -x infrastructure/scripts/provision-swarm-realm.sh staging
```

### Runner not picking up jobs

```bash
# Check runner service
docker service logs github-runner --tail 50

# Runner may need a fresh registration token
docker service rm github-runner
RUNNER_TOKEN=<new-token> bash infrastructure/scripts/setup-runner.sh
```

### Images not found (ghcr.io pull errors)

```bash
# Verify you're logged in to ghcr.io on the Swarm node
docker login ghcr.io

# Check the image exists
docker pull ghcr.io/andrekirst/family2/api:latest

# Ensure --with-registry-auth is used during stack deploy
docker stack deploy -c docker-stack.staging.yml --with-registry-auth fh-staging
```

### Rolling back a failed deployment

```bash
# Swarm auto-rollback is configured, but to manually rollback a service:
docker service rollback fh-staging_api

# Or redeploy with a previous image tag:
docker service update --image ghcr.io/andrekirst/family2/api:sha-<previous-commit> fh-staging_api
```

---

## Known Limitations

- **No TLS:** Uses HTTP (port 80) with `.familyhub.local` hostnames. TLS requires real DNS and certificates (e.g., Let's Encrypt). Add TLS when domains are configured.
- **No real DNS:** Relies on `/etc/hosts` entries. Replace with proper DNS when available.
- **Keycloak `_FILE` not supported:** Keycloak 23.0.4 does not support reading secrets from `_FILE` environment variables. Passwords are passed as plain environment variables from the deploy workflow.
- **Runner token rotation:** `myoung34/github-runner` needs a registration token that may expire. Consider a GitHub App for automatic token rotation.
- **Single storage node:** Stateful services are pinned to one node. If that node fails, databases are unavailable until it recovers.
- **No backup automation:** PostgreSQL volumes are not backed up automatically. Consider adding a scheduled backup service.
