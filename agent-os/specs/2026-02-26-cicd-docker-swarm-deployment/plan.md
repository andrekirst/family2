# CI/CD Pipeline & Docker Swarm Deployment for Turing Pi 2

## Context

Family Hub has a mature local development infrastructure (Docker Compose, Traefik, Keycloak, Taskfile) but **zero CI/CD and zero remote deployment automation**. The goal is to deploy the entire application to a Turing Pi 2 board with two RK1 ARM64 modules running Docker Swarm, with separate staging and production environments.

**Target hardware:** Turing Pi 2 — 2x RK1 (Rockchip RK3588, ARM64, 32GB RAM each), Ubuntu/Debian, Docker Swarm initialized.

**Key decisions from shaping:**

- GitHub Actions with self-hosted ARM64 runner (as Docker service in Swarm)
- GitHub Container Registry (ghcr.io) for images
- Push to `main` → auto-deploy staging; manual trigger → deploy production
- Both environments distributed across both Swarm nodes
- Stateful services (PostgreSQL, Keycloak) in Swarm with pinned placement
- IP-based access for now (`.familyhub.local` hostnames via `/etc/hosts`)

---

## Task 1: Save Spec Documentation

Create `agent-os/specs/2026-02-26-cicd-docker-swarm-deployment/` with:

- `plan.md` — This full plan
- `shape.md` — Shaping notes (scope, hardware, decisions)
- `standards.md` — N/A (no existing CI/CD standards)
- `references.md` — Pointers to existing Docker Compose patterns

---

## Task 2: Modify API Dockerfile — Add Migrate Stage

**File:** `src/FamilyHub.Api/Dockerfile`

Add a `migrate` stage after `build` that installs `dotnet-ef` for running EF Core migrations during deployment. The runtime `final` stage stays slim (no SDK).

```dockerfile
# After the existing build stage, add:

FROM ${MCR_REGISTRY}dotnet/sdk:10.0 AS migrate
WORKDIR /src
COPY --from=build /src .
RUN dotnet tool install --global dotnet-ef --version 10.*
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ef", "database", "update", \
  "--project", "FamilyHub.Api/FamilyHub.Api.csproj", \
  "--no-build"]
```

Also add `HEALTHCHECK` metadata to `final` stage (optional — primary health checks in stack files).

---

## Task 3: Modify Frontend Dockerfile — Add nginx.conf

**Files:**

- `src/frontend/family-hub-web/nginx.conf` (create)
- `src/frontend/family-hub-web/Dockerfile` (modify `prod` stage)

The `prod` stage currently copies built files to nginx but has no config — missing SPA fallback routing, gzip, cache headers, health endpoint.

Create `nginx.conf` with:

- `try_files $uri $uri/ /index.html` (SPA routing)
- Gzip compression
- Cache headers for hashed assets
- `/nginx-health` endpoint for Swarm health checks
- Security headers (X-Frame-Options, X-Content-Type-Options)

Modify Dockerfile `prod` stage:

```dockerfile
FROM ${DOCKERHUB_REGISTRY}nginx:alpine AS prod
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/family-hub-web/browser /usr/share/nginx/html
EXPOSE 80
```

**Note:** The `build` stage copies `.npmrc` which points to `localhost:4873` (Verdaccio). For CI builds, the Dockerfile needs a build arg to override the registry:

```dockerfile
FROM ${DOCKERHUB_REGISTRY}node:22-alpine AS build
ARG NPM_REGISTRY=""
WORKDIR /app
COPY package.json package-lock.json ./
RUN if [ -n "$NPM_REGISTRY" ]; then echo "registry=$NPM_REGISTRY" > .npmrc; else cp .npmrc .npmrc 2>/dev/null || true; fi
RUN npm ci
```

---

## Task 4: Create Environment-Specific appsettings

**Files:**

- `src/FamilyHub.Api/appsettings.Staging.json` — `Information` log level, all real config from env vars
- `src/FamilyHub.Api/appsettings.Production.json` — `Warning` log level, all real config from env vars

These are minimal — Docker Swarm injects all configuration via environment variables.

---

## Task 5: Create Traefik Swarm Configuration

**File:** `infrastructure/swarm/traefik/traefik-swarm.yml`

Key difference from local dev config (`infrastructure/traefik/traefik.yml`):

- `providers.docker.swarmMode: true` (Swarm label discovery)
- Entry points on ports 80/443 (standard, not 4000/4443)
- No HTTPS redirect initially (IP-based, no TLS certs)
- No file provider (all config via Docker labels)

---

## Task 6: Create Docker Swarm Base Stack

**File:** `infrastructure/swarm/docker-stack.base.yml`

Shared infrastructure deployed once:

- **Traefik v3.2** — Swarm-mode reverse proxy, global deployment on manager, ports 80 + 8080 (dashboard)
- **Overlay network:** `traefik-public` (attachable, shared by all stacks)

---

## Task 7: Create Docker Swarm Staging Stack

**File:** `infrastructure/swarm/docker-stack.staging.yml`

Full staging environment — mirrors the existing `docker-compose.env.yml` + `docker-compose.shared.yml` patterns but adapted for Swarm:

| Service | Image | Network | Placement | Notes |
|---------|-------|---------|-----------|-------|
| `postgres-staging` | `postgres:16-alpine` | staging-internal | `node.labels.storage==true` | Persistent volume, `POSTGRES_PASSWORD_FILE` from Swarm secret |
| `postgres-keycloak-staging` | `postgres:16-alpine` | staging-internal | `node.labels.storage==true` | Separate DB for Keycloak |
| `keycloak-staging` | `keycloak:23.0.4` | staging-internal + traefik-public | `node.labels.storage==true` | `auth-staging.familyhub.local` |
| `mailhog-staging` | `mailhog/mailhog` | staging-internal + traefik-public | any | `mail-staging.familyhub.local` |
| `api-staging` | `ghcr.io/.../api:${IMAGE_TAG}` | staging-internal + traefik-public | any | Same-origin proxy pattern from existing compose |
| `frontend-staging` | `ghcr.io/.../frontend:${IMAGE_TAG}` | traefik-public | any | `staging.familyhub.local` |

Key features:

- `update_config.order: start-first` for zero-downtime rolling updates
- `rollback_config` for automatic rollback on failure
- Health checks on all services
- External Swarm secrets for database passwords
- Traefik labels replicate existing `docker-compose.env.yml` routing pattern

---

## Task 8: Create Docker Swarm Production Stack

**File:** `infrastructure/swarm/docker-stack.production.yml`

Structurally identical to staging with differences:

- Hostnames: `app.familyhub.local`, `api.familyhub.local`, `auth.familyhub.local`
- `ASPNETCORE_ENVIRONMENT=Production`
- No MailHog (real SMTP config via env vars)
- Different secret names: `prod_db_password`, etc.
- Different volume names: `prod-postgres-data`, etc.
- Potentially 2 replicas for API/Frontend (HA across both nodes)

---

## Task 9: Create Keycloak Swarm Realm Template

**Files:**

- `infrastructure/keycloak/realm-swarm.json` — Copy of `realm-base.json` with Swarm-appropriate redirect URIs (`http://*.familyhub.local/*` instead of `https://*.localhost:4443/*`)
- `infrastructure/scripts/provision-swarm-realm.sh` — Variant of existing `provision-realm.sh` targeting Swarm Keycloak

---

## Task 10: Create Infrastructure Scripts

**Files:**

### `infrastructure/scripts/setup-swarm-secrets.sh`

One-time script to create Docker Swarm secrets on manager node. Prompts for staging + production passwords (DB, Keycloak DB, Keycloak admin).

### `infrastructure/scripts/setup-runner.sh`

Deploy self-hosted GitHub Actions runner as a Swarm service using `myoung34/github-runner:latest` (ARM64 native). Pinned to manager node, Docker socket mounted for builds.

### `infrastructure/scripts/deploy.sh`

Helper for manual deployments: `deploy.sh staging [IMAGE_TAG]` or `deploy.sh production [IMAGE_TAG]`. Runs migrations → deploys stack → verifies health.

---

## Task 11: Create CI Workflow (Build + Test on PRs)

**File:** `.github/workflows/ci.yml`

Triggers on PRs to `main`. Runs on `ubuntu-latest` (GitHub-hosted — tests don't need ARM64):

1. Setup .NET 10 + Node.js 22
2. `dotnet restore` + `dotnet build` + `dotnet test` (backend)
3. `npm ci` + `ng build` + `ng test` (frontend, with `npm_config_registry` override to bypass Verdaccio)
4. Upload test results as artifacts

---

## Task 12: Create Staging Deploy Workflow

**File:** `.github/workflows/deploy-staging.yml`

Triggers on push to `main`:

1. **`test` job** (ubuntu-latest) — Run all tests
2. **`build-images` job** (self-hosted, ARM64) — Build & push to ghcr.io:
   - `ghcr.io/andrekirst/family2/api:sha-<short>` (final stage)
   - `ghcr.io/andrekirst/family2/api-migrate:sha-<short>` (migrate stage)
   - `ghcr.io/andrekirst/family2/frontend:sha-<short>` (prod stage)
3. **`deploy-staging` job** (self-hosted, ARM64) — GitHub environment `staging`:
   - Run migration container against staging DB
   - `docker stack deploy -c docker-stack.staging.yml --with-registry-auth fh-staging`
   - Health check loop

---

## Task 13: Create Production Deploy Workflow

**File:** `.github/workflows/deploy-production.yml`

Manual trigger (`workflow_dispatch`) with confirmation input:

1. Validate `confirm == "deploy"`
2. Run migration container against production DB
3. `docker stack deploy -c docker-stack.production.yml --with-registry-auth fh-production`
4. Health check

Uses GitHub environment `production` with required reviewers for approval gate.

---

## Task 14: Update Documentation

**Files:**

- `infrastructure/README.md` — Add Swarm deployment section
- `docs/guides/INFRASTRUCTURE_DEVELOPMENT.md` — Add CI/CD and Swarm documentation

---

## Implementation Order

```
Task 1  → Spec documentation (save shaping context)
Task 2  → API Dockerfile (migrate stage)
Task 3  → Frontend Dockerfile + nginx.conf
Task 4  → appsettings.Staging/Production.json
Task 5  → Traefik Swarm config
Task 6  → Base stack (Traefik)
Task 7  → Staging stack
Task 8  → Production stack
Task 9  → Keycloak realm template for Swarm
Task 10 → Infrastructure scripts (secrets, runner, deploy)
Task 11 → CI workflow
Task 12 → Staging deploy workflow
Task 13 → Production deploy workflow
Task 14 → Documentation updates
```

---

## One-Time Manual Setup (on Turing Pi 2)

After all files are committed, execute on the Swarm manager:

1. `docker node update --label-add storage=true <node1-name>` (pin stateful services)
2. `docker network create --driver overlay --attachable traefik-public`
3. Run `infrastructure/scripts/setup-swarm-secrets.sh`
4. Run `infrastructure/scripts/setup-runner.sh` (registers GitHub runner)
5. `docker stack deploy -c infrastructure/swarm/docker-stack.base.yml fh-base`
6. Verify Traefik dashboard at `http://<manager-ip>:8080`
7. Push to `main` to trigger first staging deployment
8. Add to client `/etc/hosts`: `<manager-ip> staging.familyhub.local api-staging.familyhub.local auth-staging.familyhub.local mail-staging.familyhub.local app.familyhub.local api.familyhub.local auth.familyhub.local`

---

## GitHub Configuration Required

**Repository Settings → Environments:**

- `staging` — No protection rules (auto-deploy)
- `production` — Required reviewers (manual approval)

**Repository Settings → Secrets & Variables → Actions:**

- Per-environment secrets: `{STAGING,PROD}_DB_PASSWORD`, `{STAGING,PROD}_KC_DB_PASSWORD`, `{STAGING,PROD}_KC_ADMIN_PASSWORD`

**Repository Settings → Actions → Runners:**

- Self-hosted runner will auto-register via the Swarm service

---

## Verification

1. **CI workflow:** Create a PR → verify build + tests pass on GitHub-hosted runner
2. **Image build:** Merge PR → verify ARM64 images appear in ghcr.io packages
3. **Staging deploy:** Verify `docker stack services fh-staging` shows all services running
4. **Health check:** `wget -qO- http://staging.familyhub.local/health` returns healthy
5. **Frontend:** Browse `http://staging.familyhub.local` → Angular app loads
6. **Keycloak:** Browse `http://auth-staging.familyhub.local` → login page renders
7. **Production deploy:** Trigger manual workflow → verify `fh-production` stack is healthy

---

## Key Technical Considerations

- **ARM64 native builds:** Self-hosted runner is ARM64, so `docker build` produces native images — no QEMU/buildx overhead
- **.npmrc override:** Frontend's `.npmrc` points to Verdaccio (localhost:4873). CI builds must override with public npm registry
- **Keycloak secrets:** KC 23.0.4 doesn't support `_FILE` env vars — pass passwords as plain env vars from GitHub secrets
- **Migration networking:** Migration container must connect to overlay network where PostgreSQL runs — deploy stack first (creates network + starts DB), then run migration, then update API image
- **Runner token rotation:** `myoung34/github-runner` needs a fresh registration token. Consider GitHub App for auto-rotation long-term
