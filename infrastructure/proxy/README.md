# Package Caching Proxies

Local caching proxies for npm, NuGet, and Docker images. Eliminates redundant downloads during development and Docker builds.

## Architecture

```
npm ci ──────► Verdaccio (:4873) ──► npmjs.org
dotnet restore ► BaGet    (:5555) ──► nuget.org
docker pull ──► Registry  (:5001) ──► Docker Hub
              ► Registry  (:5002) ──► MCR (mcr.microsoft.com)
              ► Registry  (:5003) ──► GHCR (ghcr.io)
```

All proxy services run in the shared Traefik compose stack (`docker-compose.traefik.yml`). Their data volumes use the `fh-proxy-*` prefix and are declared `external`, so they survive `task env:destroy`.

## Services

| Service | Port | Web UI | Cache Target |
|---------|------|--------|--------------|
| Verdaccio | `localhost:4873` | `https://npm.dev.andrekirst.de:4443` | npmjs.org |
| BaGet | `localhost:5555` | `https://nuget.dev.andrekirst.de:4443` | nuget.org |
| Registry (Docker Hub) | `localhost:5001` | — | registry-1.docker.io |
| Registry (MCR) | `localhost:5002` | — | mcr.microsoft.com |
| Registry (GHCR) | `localhost:5003` | — | ghcr.io |

## Usage

Proxies start automatically with Traefik:

```bash
task traefik:up    # starts Traefik + all proxy services
```

### npm (Verdaccio)

The `.npmrc` in the frontend project points npm to the local Verdaccio instance:

```
registry=http://localhost:4873/
```

First `npm ci` downloads from npmjs.org via Verdaccio; subsequent runs serve from cache.

### NuGet (BaGet)

The BaGet source is injected only during Docker builds via the `BAGET_URL` build arg in the API Dockerfile. This avoids breaking local `dotnet test` when BaGet isn't running (NuGet errors on unreachable HTTP sources).

In Docker builds (`docker-compose.env.yml`), the BaGet source is added automatically:

```yaml
args:
  BAGET_URL: ${BAGET_URL:-http://localhost:5555/v3/index.json}
```

Local `dotnet restore` and `dotnet test` use the default nuget.org source — no proxy needed.

A reference `NuGet.Config` is kept at `infrastructure/proxy/baget/NuGet.Config` for documentation.

### Docker Images

Docker builds use `--build-arg` to route through local registries:

```bash
docker build --build-arg DOCKERHUB_REGISTRY=localhost:5001/ ...
docker build --build-arg MCR_REGISTRY=localhost:5002/ ...
```

The `docker-compose.env.yml` sets these args automatically.

## Task Commands

```bash
task proxy:status   # show proxy container status
task proxy:logs     # follow proxy service logs
task proxy:usage    # show cache disk usage
task proxy:clean    # stop proxies + remove cache volumes (with confirmation)
```

## Troubleshooting

### npm install fails with ECONNREFUSED

Verdaccio isn't running. Start it with `task traefik:up`, or remove `.npmrc` to use npmjs.org directly.

### dotnet restore is slow despite BaGet running

BaGet may still be fetching packages on first restore. Check logs: `task proxy:logs -- baget`

### Docker build can't pull images through proxy

Ensure the registry containers are running: `task proxy:status`. The registry proxies require `network: host` in the build step to access `localhost` ports.

## Volume Management

Cache volumes persist across `task env:destroy` by design. To reclaim disk space:

```bash
task proxy:usage    # check sizes first
task proxy:clean    # remove all cache data
```
