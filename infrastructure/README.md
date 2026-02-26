# Multi-Environment Development Infrastructure

Run multiple fully-isolated FamilyHub development stacks simultaneously, each accessible via unique `*.localhost` subdomains (or `*.dev.andrekirst.de` for Google OAuth compatibility). Shared services (Keycloak, MailHog, Infisical) run once; per-environment services (Postgres, API, Frontend, pgAdmin) are isolated per branch.

## Architecture

```
Shared Layer (runs once)
  docker-compose.traefik.yml     docker-compose.shared.yml
  ─────────────────────────      ────────────────────────
  Traefik (4000/4443/8888)       Keycloak (auth.localhost)     <- per-env realms
  Hub Dashboard                  Keycloak Postgres
  Verdaccio (npm proxy)          MailHog (mail.localhost)
  BaGet (NuGet proxy)            Infisical (secrets.localhost)
  Docker Registry Mirrors

Per-Env: fh-feature-150          Per-Env: fh-main
  docker-compose.env.yml           docker-compose.env.yml
  ────────────────────             ────────────────────
  Postgres (isolated)              Postgres (isolated)
  API (dotnet watch)               API (dotnet watch)
  Frontend (ng serve)              Frontend (ng serve)
  pgAdmin                          pgAdmin
```

## Prerequisites

| Tool | Install |
|------|---------|
| **Docker** + Compose v2 | [docs.docker.com](https://docs.docker.com/get-docker/) |
| **Task** (v3+) | [taskfile.dev](https://taskfile.dev/installation/) |
| **mkcert** | [github.com/FiloSottile/mkcert](https://github.com/FiloSottile/mkcert#installation) |
| **jq** | `sudo apt install jq` or `brew install jq` |
| **curl** | Usually pre-installed |

## Quick Start

```bash
# 1. One-time: generate TLS certificates
task setup:certs

# 2. Start shared services (Traefik + Keycloak + MailHog + proxies)
task shared:up

# 3. Start environment for your current branch
task up

# 4. See all URLs
task urls
```

`task up` automatically starts shared services if they're not running.

## URL Convention

| Service | URL |
|---------|-----|
| Frontend | `https://{env}.localhost:4443` |
| API | `https://api-{env}.localhost:4443/graphql` |
| pgAdmin | `https://pgadmin-{env}.localhost:4443` |
| Keycloak (shared) | `https://auth.localhost:4443` |
| MailHog (shared) | `https://mail.localhost:4443` |
| Infisical (shared) | `https://secrets.localhost:4443` |
| Hub Dashboard | `https://hub.localhost:4443` |
| Traefik Dashboard | `http://localhost:8888` |

Dual-domain: Replace `.localhost` with `.dev.andrekirst.de` for Google OAuth compatibility.

## Environment Names

Branch names are converted to Docker-safe environment names:

- `main` -> `main`
- `feature/150-file-management` -> `feature-150-file-management`
- `fix/bug-42` -> `fix-bug-42`

## Keycloak Realm Provisioning

Each environment gets its own Keycloak realm (`FamilyHub-{env}`) provisioned via the Admin REST API:

```bash
# Automatic (happens during `task up`)
# Manual provisioning:
bash infrastructure/keycloak/provision-realm.sh feature-150-file-management

# Delete a realm:
bash infrastructure/keycloak/provision-realm.sh feature-150-file-management --delete
```

## Daily Workflow

```bash
# Start your environment
task up

# View logs (all services or filtered)
task env:logs
task env:logs -- api

# Health check
task status

# Show running containers
task env:ps

# Stop (data persists in Docker volumes)
task down

# Full reset (removes volumes + Keycloak realm)
task env:destroy

# List all running environments
task list
```

## Test Users

| Username | Password | Role |
|----------|----------|------|
| `testowner` | `test123` | family-owner |
| `testmember` | `test123` | family-member |

Keycloak admin: `admin` / `admin`
pgAdmin: `admin@familyhub.dev` / `admin`

## Host-Based Development (no Docker)

The traditional workflow still works:

```bash
docker compose up -d          # Start postgres, keycloak, mailhog
dotnet run --project src/FamilyHub.Api
cd src/frontend/family-hub-web && ng serve
```

This uses `localhost` ports directly (5432, 8080, 5152, 4200).

## Secrets Management (Infisical)

Infisical is a self-hosted secrets vault that runs as **shared infrastructure** alongside Traefik and the package caching proxies. It starts once with `task traefik:up` and serves all branch environments. The .NET API fetches secrets at startup via a custom `IConfigurationProvider`.

| Access | URL |
|--------|-----|
| Traefik TLS | `https://infisical.dev.andrekirst.de:4443` |
| Direct (host) | `http://localhost:8180` |

### First-Time Setup

```bash
# 1. Start shared infrastructure (Infisical starts automatically)
task traefik:up

# 2. Open Infisical UI and create an account
open http://localhost:8180

# 3. Create a project (e.g., "FamilyHub")

# 4. Add secrets to the project (Environment: dev, Path: /):
#    GoogleIntegration__OAuth__ClientId=<your-google-client-id>
#    GoogleIntegration__OAuth__ClientSecret=<your-google-client-secret>
#    GoogleIntegration__EncryptionKey=<generate-with-openssl-rand-hex-32>

# 5. Create a Machine Identity (Settings -> Machine Identities)
#    - Auth method: Universal Auth
#    - Copy Client ID and Client Secret

# 6. Add credentials to .env (copy from .env.example)
cp .env.example .env
# Fill in: INFISICAL_PROJECT_ID, INFISICAL_CLIENT_ID, INFISICAL_CLIENT_SECRET
```

### Host-Based Development

For the single-dev workflow (`docker compose up -d` from the project root), Infisical runs inside the root `docker-compose.yml` at `localhost:8180`.

```bash
# Export Infisical env vars, then run the API
source .env
dotnet run --project src/FamilyHub.Api
```

The API reads `INFISICAL_CLIENT_ID` and `INFISICAL_CLIENT_SECRET` from the environment. If these are not set, Infisical is skipped and the API uses `appsettings.json` defaults.

### Multi-Environment (Docker Compose)

Per-environment API containers reach Infisical via `http://infisical:8080` on the shared `traefik-public` network. Set the `INFISICAL_*` env vars in `.env` or export them before running `task up`.

### Without Infisical

If you don't need secrets management, simply don't set the `INFISICAL_*` env vars. The API will start normally using `appsettings.json` defaults.

## Troubleshooting

**"mkcert: command not found"**
Install mkcert: `sudo apt install mkcert` or see [installation guide](https://github.com/FiloSottile/mkcert#installation).

**"Certificate not trusted"**
Run `mkcert -install` to add the local CA to your system trust store. Use `--force` flag with setup-certs to regenerate.

**Port 4443 already in use**
Another Traefik instance is running. Stop it: `task shared:down`

**Keycloak slow to start**
First startup can take 60+ seconds. Check progress: `task shared:logs -- keycloak`

**Realm provisioning fails**
Keycloak must be fully healthy. Wait and retry: `bash infrastructure/keycloak/provision-realm.sh {env-name}`

**Keycloak "Invalid redirect_uri"**
The realm template uses wildcard URIs. Ensure Keycloak imported the template correctly -- check `task env:logs -- keycloak`.

**Hub shows "Cannot reach Traefik API"**
Start shared services: `task shared:up`

**Multiple environments**
Each worktree can run its own environment simultaneously. Just run `task up` in each worktree.

---

## Docker Swarm Deployment (Turing Pi 2)

FamilyHub deploys to a Turing Pi 2 board with 2x RK1 ARM64 modules running Docker Swarm. Staging deploys automatically on push to `main`; production requires manual trigger.

### Architecture

```
Turing Pi 2 (2x RK1, ARM64, 32GB RAM each)
Docker Swarm Mode

Base Stack (fh-base)                    Traefik v3.2 (port 80/443, dashboard :8080)
                                        Overlay network: traefik-public

Staging Stack (fh-staging)              Production Stack (fh-production)
─────────────────────────               ──────────────────────────────
PostgreSQL (pinned)                     PostgreSQL (pinned)
Keycloak (pinned)                       Keycloak (pinned)
MailHog                                 API (2 replicas)
API (1 replica)                         Frontend (2 replicas)
Frontend (1 replica)
```

### URL Convention (Swarm)

| Service | Staging | Production |
|---------|---------|------------|
| Frontend | `http://staging.familyhub.local` | `http://app.familyhub.local` |
| API | `http://api-staging.familyhub.local` | `http://api.familyhub.local` |
| Keycloak | `http://auth-staging.familyhub.local` | `http://auth.familyhub.local` |
| MailHog | `http://mail-staging.familyhub.local` | N/A (real SMTP) |

Add to client `/etc/hosts`:

```
<manager-ip> staging.familyhub.local api-staging.familyhub.local auth-staging.familyhub.local mail-staging.familyhub.local app.familyhub.local api.familyhub.local auth.familyhub.local
```

### CI/CD Workflows

| Workflow | Trigger | Runner | Purpose |
|----------|---------|--------|---------|
| `ci.yml` | PR to main | GitHub-hosted | Build + test (backend + frontend) |
| `deploy-staging.yml` | Push to main | Self-hosted ARM64 | Test, build images, deploy staging |
| `deploy-production.yml` | Manual dispatch | Self-hosted ARM64 | Deploy to production (approval required) |

### One-Time Swarm Setup

Run on the Swarm manager node:

```bash
# 1. Label storage node
docker node update --label-add storage=true <node1>

# 2. Create shared overlay network
docker network create --driver overlay --attachable traefik-public

# 3. Create Swarm secrets
bash infrastructure/scripts/setup-swarm-secrets.sh

# 4. Deploy self-hosted GitHub runner
RUNNER_TOKEN=<token> bash infrastructure/scripts/setup-runner.sh

# 5. Deploy base stack (Traefik)
docker stack deploy -c infrastructure/swarm/docker-stack.base.yml fh-base

# 6. Push to main to trigger first staging deployment
```

### Manual Deployment

```bash
# Deploy staging
bash infrastructure/scripts/deploy.sh staging sha-abc1234

# Deploy production
bash infrastructure/scripts/deploy.sh production sha-abc1234
```

### Swarm Files

```
infrastructure/swarm/
├── traefik/
│   └── traefik-swarm.yml          # Traefik static config (Swarm mode)
├── docker-stack.base.yml          # Base: Traefik reverse proxy
├── docker-stack.staging.yml       # Full staging environment
└── docker-stack.production.yml    # Full production environment
```
