# Multi-Environment Development Infrastructure

Run multiple fully-isolated FamilyHub development stacks simultaneously, each accessible via unique `*.localhost` subdomains. Shared services (Keycloak, MailHog) run once; per-environment services (Postgres, API, Frontend, pgAdmin) are isolated per branch.

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

**Hub shows "Cannot reach Traefik API"**
Start shared services: `task shared:up`

**Multiple environments**
Each worktree can run its own environment simultaneously. Just run `task up` in each worktree.
