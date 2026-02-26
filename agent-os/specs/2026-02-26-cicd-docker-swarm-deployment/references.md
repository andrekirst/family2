# References for CI/CD & Docker Swarm Deployment

## Existing Infrastructure (Adapt for Swarm)

### Docker Compose — Per-Environment Stack

- **Location:** `infrastructure/docker-compose.env.yml`
- **Relevance:** Primary pattern for service definitions, Traefik labels, environment variables, and networking
- **Key patterns to replicate:**
  - Same-origin API proxy via Traefik PathPrefix rules (lines 110-116)
  - PostgreSQL health checks (lines 20-24)
  - Environment variable injection for Keycloak, CORS, email (lines 70-97)

### Docker Compose — Shared Services

- **Location:** `infrastructure/docker-compose.shared.yml`
- **Relevance:** Keycloak + MailHog + PostgreSQL for Keycloak — merge into per-environment Swarm stacks
- **Key patterns:** Keycloak startup command, realm configuration

### Docker Compose — Traefik

- **Location:** `infrastructure/docker-compose.traefik.yml`
- **Relevance:** Traefik configuration, port mapping, Docker socket mounting
- **Key change for Swarm:** `swarmMode: true` in provider config

### Traefik Static Config

- **Location:** `infrastructure/traefik/traefik.yml`
- **Relevance:** Base Traefik config to adapt for Swarm mode
- **Key differences:** Remove file provider, add swarmMode, change ports from 4000/4443 to 80/443

### API Dockerfile

- **Location:** `src/FamilyHub.Api/Dockerfile`
- **Relevance:** Multi-stage build pattern (dev → build → final). Add migrate stage between build and final.

### Frontend Dockerfile

- **Location:** `src/frontend/family-hub-web/Dockerfile`
- **Relevance:** Multi-stage build (dev → build → prod). Prod stage needs nginx.conf for SPA routing.

### Keycloak Realm Provisioning

- **Location:** `infrastructure/keycloak/provision-realm.sh`, `infrastructure/keycloak/realm-base.json`
- **Relevance:** Realm template and REST API provisioning script. Adapt URLs for Swarm hostnames.

### Taskfile

- **Location:** `Taskfile.yml`
- **Relevance:** Task automation patterns. May extend with Swarm-specific tasks later.

### Health Check Script

- **Location:** `infrastructure/scripts/health-check.sh`
- **Relevance:** Endpoint verification patterns to replicate in CI deploy workflows.

## External References

### myoung34/github-runner

- Docker image for self-hosted GitHub Actions runners
- Supports ARM64 natively
- Docker socket mounting for builds
- Environment-based configuration (REPO_URL, RUNNER_TOKEN, RUNNER_LABELS)

### Docker Swarm Documentation

- Stack files use Compose v3.8 format with `deploy` key
- Overlay networks for cross-node communication
- Secrets mounted as files at `/run/secrets/<name>`
- Placement constraints via node labels
