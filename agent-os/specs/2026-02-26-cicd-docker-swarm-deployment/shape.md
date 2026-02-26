# CI/CD & Docker Swarm Deployment — Shaping Notes

## Scope

Deploy the full Family Hub application (API, Frontend, PostgreSQL, Keycloak) to a Turing Pi 2 with two RK1 ARM64 modules via Docker Swarm. Create CI/CD pipelines using GitHub Actions for automated staging deployment and manual production promotion.

## Decisions

- **Docker Swarm over Kubernetes:** 2-node ARM64 cluster is too small for K8s overhead. Swarm is built into Docker, uses compose-like YAML, and suits a single-developer project.
- **Self-hosted runner as Swarm service:** Avoids SSH key management, runs natively on ARM64 (no QEMU), has direct Docker socket access for builds and deploys.
- **GHCR over Docker Hub:** Free for the repo, native GitHub Actions integration via `GITHUB_TOKEN`, no rate limits.
- **Host-based routing via `.familyhub.local`:** Consistent with existing Traefik patterns. Requires `/etc/hosts` entries on client machines. Easily replaced with real DNS later.
- **Port 80 (no TLS initially):** IP-based access means no certificate possible. Add TLS when domains are configured.
- **Separate migration image:** EF Core `dotnet-ef` tooling stays in SDK-based image, keeps runtime image slim (~200MB vs ~800MB).
- **PostgreSQL `_FILE` secrets:** Supported by postgres:16-alpine. Keycloak 23.0.4 does NOT support `_FILE` — use plain env vars from deploy scripts.

## Context

- **Hardware:** Turing Pi 2, 2x RK1 (Rockchip RK3588, ARM64/aarch64, 32GB RAM each, Ubuntu/Debian)
- **Visuals:** None
- **References:** Existing Docker Compose files (`infrastructure/docker-compose.*.yml`), Traefik config, Keycloak provisioning scripts
- **Product alignment:** Enables Phase 1 MVP deployment to real hardware for testing and demo purposes

## Standards Applied

- No existing CI/CD standards in `agent-os/standards/` — this work will establish them
- Follows existing Docker Compose patterns (Traefik labels, health checks, overlay networks)
- Follows existing Keycloak realm provisioning pattern (`infrastructure/keycloak/provision-realm.sh`)
