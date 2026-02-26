# Standards for CI/CD & Docker Swarm Deployment

No existing CI/CD or deployment standards exist in `agent-os/standards/`. This feature will establish the initial patterns.

## Standards to Create After Implementation

Once the CI/CD pipelines and Swarm deployment are operational, consider extracting these standards:

### `infrastructure/docker-swarm-stack`

- Stack file structure (base + per-environment)
- Service naming conventions (`{service}-{env}`)
- Placement constraints for stateful services
- Update and rollback configuration
- Health check patterns

### `infrastructure/github-actions-ci`

- Workflow structure (CI vs deploy)
- Job separation (test → build → deploy)
- Self-hosted runner configuration
- Secret management patterns
- Environment protection rules

### `infrastructure/container-images`

- Multi-stage Dockerfile patterns (dev → build → migrate → final)
- ARM64-native build strategy
- Image tagging convention (`sha-<short-hash>`)
- Registry organization (`ghcr.io/{owner}/{repo}/{service}`)
