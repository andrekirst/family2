# Multi-Environment Development Infrastructure

Run multiple fully-isolated FamilyHub development stacks simultaneously, each accessible via unique `*.localhost` subdomains.

## Prerequisites

| Tool | Install |
|------|---------|
| **Docker** + Compose v2 | [docs.docker.com](https://docs.docker.com/get-docker/) |
| **Task** (v3+) | [taskfile.dev](https://taskfile.dev/installation/) |
| **mkcert** | [github.com/FiloSottile/mkcert](https://github.com/FiloSottile/mkcert#installation) |

## Quick Start

```bash
# 1. One-time: generate TLS certificates
task setup:certs

# 2. Start environment for your current branch
task up

# 3. See all URLs
task urls
```

## Environment Hub

A web-based dashboard showing all running environments at a glance:

```bash
# Open hub in browser
task hub

# Or navigate directly
# https://hub.localhost:4443
```

The hub starts automatically with Traefik (`task traefik:up`) and shows clickable cards for each running environment with links to App, API, Keycloak, MailHog, and pgAdmin. It auto-refreshes every 10 seconds.

## Architecture

```
Browser: https://hub.localhost:4443            (Environment Hub)
         https://{env}.localhost:4443          (Frontend)
         https://api-{env}.localhost:4443      (API/GraphQL)
         https://kc-{env}.localhost:4443       (Keycloak)
         https://mail-{env}.localhost:4443     (MailHog)
         https://pgadmin-{env}.localhost:4443  (pgAdmin)
                          |
                   [Traefik :4443]  ← shared, single instance
                   [Hub nginx]      ← static landing page
                   Dashboard :8888
                          |
         +----------------+------------------+
         |                |                  |
   [fh-main_*]    [fh-feature-121_*]   [fh-feature-125_*]
```

Each branch gets a unique env name derived from the branch name:

- `main` → `main`
- `feature/121-sidebar` → `feature-121-sidebar`
- `fix/bug-42` → `fix-bug-42`

## Daily Workflow

```bash
# Start your environment
task up

# View logs (all services or filtered)
task env:logs
task env:logs -- api

# Show running containers
task env:ps

# Stop (data persists in Docker volumes)
task down

# Full reset (removes volumes too)
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

## Troubleshooting

**"mkcert: command not found"**
Install mkcert: `sudo apt install mkcert` or see [installation guide](https://github.com/FiloSottile/mkcert#installation).

**"Certificate not trusted"**
Run `mkcert -install` to add the local CA to your system trust store.

**Port 3443 already in use**
Another Traefik instance is running. Stop it: `task traefik:down`

**Container name conflicts**
Old containers from previous setup may conflict. Run `docker compose down` in the project root first.

**Keycloak "Invalid redirect_uri"**
The realm template uses wildcard URIs (`https://*.localhost:4443/*`). Ensure Keycloak imported the template correctly — check `task env:logs -- keycloak`.

**Hub shows "Cannot reach Traefik API"**
The hub needs Traefik running to fetch router data. Start it with `task traefik:up`. If Traefik is running but the API is unreachable, check that `api.dashboard: true` is set in `traefik/traefik.yml`.
