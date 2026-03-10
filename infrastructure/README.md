# FamilyHub Development Infrastructure

Two modes for local development: **Simple** (recommended) for everyday single-branch work, and **Multi-Environment** for parallel branch isolation.

## Prerequisites

| Tool | Install |
|------|---------|
| **Docker** + Compose v2 | [docs.docker.com](https://docs.docker.com/get-docker/) |
| **Task** (v3+) | [taskfile.dev](https://taskfile.dev/installation/) |
| **mkcert** | [github.com/FiloSottile/mkcert](https://github.com/FiloSottile/mkcert#installation) |
| **jq** | `sudo apt install jq` or `brew install jq` |
| **curl** | Usually pre-installed |

## Quick Start (Simple Mode)

```bash
# 1. One-time: generate TLS certificates
task setup:certs

# 2. Set simple mode (persists across sessions)
task mode:set -- simple

# 3. Start everything
task up

# 4. See all URLs
task urls
```

That's it. Everything runs on `https://localhost:4443`.

## Mode Management

```bash
task mode:set -- simple     # Switch to simple mode
task mode:set -- multi      # Switch to multi-environment mode
task mode:status            # Show current mode
task mode:reset             # Clear mode (will prompt on next 'task up')
```

You can also override mode per-command: `MODE=simple task up`

First `task up` without a mode set will show an interactive prompt.

## Simple Mode

All services on a single hostname with path-based routing:

| Service | URL |
|---------|-----|
| Frontend | `https://localhost:4443` |
| API (GraphQL) | `https://localhost:4443/graphql` |
| API Health | `https://localhost:4443/health` |
| Config | `https://localhost:4443/config` |
| Keycloak | `https://localhost:4443/auth` |
| MailHog | `https://mail.localhost:4443` |
| pgAdmin | `https://pgadmin.localhost:4443` |
| MinIO | `https://minio.localhost:4443` |
| npm Registry | `https://npm.localhost:4443` |
| NuGet Gallery | `https://nuget.localhost:4443` |
| Traefik Dashboard | `http://localhost:8888` |

**Keycloak realm:** `FamilyHub-dev`

**What's excluded in simple mode:** Infisical, Docker registry mirrors, Hub dashboard.

### Daily Workflow (Simple)

```bash
task up                # Start all services
task status            # Health check
task simple:logs       # Follow all logs
task simple:logs -- api  # Follow API logs only
task simple:ps         # Show containers
task down              # Stop (data persists)
task destroy           # Full reset (removes volumes + realm)
```

## Multi-Environment Mode

Each git branch gets a fully isolated stack with unique subdomains. Shared services (Keycloak, MailHog, Infisical) run once.

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

### Environment Names

Branch names are converted to Docker-safe environment names:

- `main` → `main`
- `feature/150-file-management` → `feature-150-file-management`
- `fix/bug-42` → `fix-bug-42`

### Daily Workflow (Multi)

```bash
task up                # Provision realm + start env for current branch
task status            # Health dashboard (shared + environment)
task env:logs          # Follow logs
task env:logs -- api   # API logs only
task env:ps            # Show containers
task down              # Stop (data persists)
task env:destroy       # Full reset (removes volumes + realm)
task list              # List all running environments
```

### Shared Services

```bash
task shared:up         # Start shared layer (auto-started by 'task up')
task shared:down       # Stop shared layer
task shared:status     # Health check shared services
task shared:logs       # Follow shared logs
```

## Switching Modes

When switching modes, destroy the old mode's containers first:

```bash
# From simple → multi
task destroy           # Clean up simple mode
task mode:set -- multi
task up

# From multi → simple
task destroy           # Clean up multi mode
task mode:set -- simple
task up
```

Volumes are mode-isolated (`fh-simple-*` vs `fh-{env}-*`), so data doesn't cross over.

## URL Reference (Side-by-Side)

| Service | Simple Mode | Multi-Env Mode |
|---------|------------|----------------|
| Frontend | `localhost:4443` | `{env}.localhost:4443` |
| API | `localhost:4443/graphql` | `api-{env}.localhost:4443/graphql` |
| Keycloak | `localhost:4443/auth` | `auth.localhost:4443` |
| MailHog | `mail.localhost:4443` | `mail.localhost:4443` |
| pgAdmin | `pgadmin.localhost:4443` | `pgadmin-{env}.localhost:4443` |
| MinIO | `minio.localhost:4443` | `minio-{env}.localhost:4443` |
| Realm | `FamilyHub-dev` | `FamilyHub-{env}` |

## Keycloak Realm Provisioning

Each environment gets a Keycloak realm provisioned via the Admin REST API:

```bash
# Automatic (happens during `task up`)
# Manual:
bash infrastructure/keycloak/provision-realm.sh dev --mode simple
bash infrastructure/keycloak/provision-realm.sh feature-150 --mode multi

# Delete:
bash infrastructure/keycloak/provision-realm.sh dev --mode simple --delete
```

## Test Users

| Username | Password | Role |
|----------|----------|------|
| `testowner` | `test123` | family-owner |
| `testmember` | `test123` | family-member |

Keycloak admin: `admin` / `admin`
pgAdmin: `admin@familyhub.dev` / `admin`

## Architecture

```
Simple Mode (one compose stack: fh-simple)
  docker-compose.base.yml + docker-compose.simple.yml
  ─────────────────────────────────────────────────────
  Traefik → path-based routing on localhost:4443
  Postgres, Keycloak (+ KC DB), API, Frontend
  MailHog, pgAdmin, MinIO, Verdaccio, BaGet

Multi-Env Mode (shared + per-env stacks)
  docker-compose.base.yml + docker-compose.multi.yml
  ─────────────────────────────────────────────────────
  Shared: Traefik, Keycloak (+ KC DB), MailHog, Infisical
          Verdaccio, BaGet, Docker Registry Mirrors, Hub
  Per-env: Postgres, API, Frontend, pgAdmin, MinIO
```

## Compose File Layout

| File | Purpose |
|------|---------|
| `docker-compose.base.yml` | Common service definitions (no Traefik labels) |
| `docker-compose.simple.yml` | Simple mode overlay (path-based routing) |
| `docker-compose.multi.yml` | Multi-env overlay (subdomain routing, Infisical, registries) |
| `docker-compose.traefik.yml` | **Deprecated** — kept for transition |
| `docker-compose.shared.yml` | **Deprecated** — kept for transition |
| `docker-compose.env.yml` | **Deprecated** — kept for transition |

## Troubleshooting

**"mkcert: command not found"**
Install mkcert: `sudo apt install mkcert` or see [installation guide](https://github.com/FiloSottile/mkcert#installation).

**"Certificate not trusted"**
Run `mkcert -install` to add the local CA to your system trust store.

**Port 4443 already in use**
Another Traefik instance is running. Stop it: `task down`

**Keycloak slow to start**
First startup can take 60+ seconds. Check progress: `task simple:logs -- keycloak`

**Realm provisioning fails**
Keycloak must be fully healthy. Wait and retry: `bash infrastructure/keycloak/provision-realm.sh dev --mode simple`

**Keycloak "Invalid redirect_uri"**
The realm template includes `localhost:4443` redirect URIs for both modes. Verify the realm imported correctly.

**No mode set**
Run `task mode:set -- simple` to set a mode, or just run `task up` to get an interactive prompt.

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

### CI/CD Workflows

| Workflow | Trigger | Runner | Purpose |
|----------|---------|--------|---------|
| `ci.yml` | PR to main | GitHub-hosted | Build + test (backend + frontend) |
| `deploy-staging.yml` | Push to main | Self-hosted ARM64 | Test, build images, deploy staging |
| `deploy-production.yml` | Manual dispatch | Self-hosted ARM64 | Deploy to production (approval required) |

### Swarm Files

```
infrastructure/swarm/
├── traefik/
│   └── traefik-swarm.yml          # Traefik static config (Swarm mode)
├── docker-stack.base.yml          # Base: Traefik reverse proxy
├── docker-stack.staging.yml       # Full staging environment
└── docker-stack.production.yml    # Full production environment
```
