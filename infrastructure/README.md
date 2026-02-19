# Multi-Environment Development Infrastructure

Run multiple fully-isolated FamilyHub development stacks simultaneously, each accessible via unique `*.dev.andrekirst.de` subdomains.

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
# https://hub.dev.andrekirst.de:4443
```

The hub starts automatically with Traefik (`task traefik:up`) and shows clickable cards for each running environment with links to App, API, Keycloak, MailHog, and pgAdmin. It auto-refreshes every 10 seconds.

## Architecture

```
Browser: https://hub.dev.andrekirst.de:4443            (Environment Hub)
         https://{env}.dev.andrekirst.de:4443          (Frontend)
         https://api-{env}.dev.andrekirst.de:4443      (API/GraphQL)
         https://kc-{env}.dev.andrekirst.de:4443       (Keycloak)
         https://mail-{env}.dev.andrekirst.de:4443     (MailHog)
         https://pgadmin-{env}.dev.andrekirst.de:4443  (pgAdmin)
                          |
                   [Traefik :4443]  <- shared, single instance
                   [Hub nginx]      <- static landing page
                   Dashboard :8888
                          |
         +----------------+------------------+
         |                |                  |
   [fh-main_*]    [fh-feature-121_*]   [fh-feature-125_*]
```

Each branch gets a unique env name derived from the branch name:

- `main` -> `main`
- `feature/121-sidebar` -> `feature-121-sidebar`
- `fix/bug-42` -> `fix-bug-42`

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
Run `mkcert -install` to add the local CA to your system trust store.

**Port 3443 already in use**
Another Traefik instance is running. Stop it: `task traefik:down`

**Container name conflicts**
Old containers from previous setup may conflict. Run `docker compose down` in the project root first.

**Keycloak "Invalid redirect_uri"**
The realm template uses wildcard URIs (`https://*.dev.andrekirst.de:4443/*`). Ensure Keycloak imported the template correctly -- check `task env:logs -- keycloak`.

**Hub shows "Cannot reach Traefik API"**
The hub needs Traefik running to fetch router data. Start it with `task traefik:up`. If Traefik is running but the API is unreachable, check that `api.dashboard: true` is set in `traefik/traefik.yml`.
