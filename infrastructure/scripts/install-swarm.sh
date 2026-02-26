#!/usr/bin/env bash
# ============================================================================
# FamilyHub — Docker Swarm Installation Script
# ============================================================================
#
# Interactive installer that walks through the full initial setup of the
# FamilyHub Docker Swarm deployment on a Turing Pi 2 (or any Docker Swarm
# cluster).
#
# Run on the Swarm MANAGER node:
#   bash infrastructure/scripts/install-swarm.sh
#
# What this script does (each step can be skipped):
#   1. Check prerequisites (Docker, Swarm, jq, curl)
#   2. Label the storage node for stateful services
#   3. Create the traefik-public overlay network
#   4. Create Swarm secrets (calls setup-swarm-secrets.sh)
#   5. Deploy the base stack — Traefik reverse proxy
#   6. Deploy the GitHub Actions self-hosted runner (optional)
#   7. Deploy the staging stack (optional)
#   8. Provision the Keycloak staging realm (optional)
#   9. Print /etc/hosts entries and next steps
#
# Idempotent: safe to re-run. Each step detects existing resources and skips
# or prompts accordingly.
# ============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SWARM_DIR="${SCRIPT_DIR}/../swarm"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m'

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
info()    { echo -e "  ${CYAN}▸${NC} $*"; }
success() { echo -e "  ${GREEN}✓${NC} $*"; }
warn()    { echo -e "  ${YELLOW}!${NC} $*"; }
fail()    { echo -e "  ${RED}✗${NC} $*"; }

ask_yes_no() {
  local prompt="$1"
  local default="${2:-y}"
  local yn
  if [[ "$default" == "y" ]]; then
    read -rp "  ${prompt} [Y/n] " yn
    yn="${yn:-y}"
  else
    read -rp "  ${prompt} [y/N] " yn
    yn="${yn:-n}"
  fi
  [[ "$yn" =~ ^[Yy] ]]
}

header() {
  echo ""
  echo -e "  ${BOLD}── $1${NC}"
  echo ""
}

# ---------------------------------------------------------------------------
# Step 1: Check prerequisites
# ---------------------------------------------------------------------------
check_prerequisites() {
  header "Step 1/9: Checking prerequisites"

  local failed=0

  # Docker
  if command -v docker &>/dev/null; then
    local docker_version
    docker_version=$(docker version --format '{{.Server.Version}}' 2>/dev/null || echo "unknown")
    success "Docker ${docker_version}"
  else
    fail "Docker not found. Install: https://docs.docker.com/get-docker/"
    failed=1
  fi

  # Docker Swarm mode
  local swarm_status
  swarm_status=$(docker info --format '{{.Swarm.LocalNodeState}}' 2>/dev/null || echo "unknown")
  if [[ "$swarm_status" == "active" ]]; then
    local node_role
    node_role=$(docker info --format '{{.Swarm.ControlAvailable}}' 2>/dev/null || echo "false")
    if [[ "$node_role" == "true" ]]; then
      success "Docker Swarm active (this node is a manager)"
    else
      fail "Docker Swarm active but this node is a WORKER. Run this script on the manager node."
      failed=1
    fi
  else
    fail "Docker Swarm not active. Initialize with: docker swarm init --advertise-addr <ip>"
    failed=1
  fi

  # Node count
  local node_count
  node_count=$(docker node ls --format '{{.ID}}' 2>/dev/null | wc -l || echo "0")
  info "Swarm nodes: ${node_count}"

  # curl
  if command -v curl &>/dev/null; then
    success "curl available"
  else
    fail "curl not found. Install: sudo apt install curl"
    failed=1
  fi

  # jq
  if command -v jq &>/dev/null; then
    success "jq available"
  else
    fail "jq not found. Install: sudo apt install jq"
    failed=1
  fi

  if [[ $failed -ne 0 ]]; then
    echo ""
    fail "Prerequisites not met. Fix the issues above and re-run."
    exit 1
  fi

  success "All prerequisites met."
}

# ---------------------------------------------------------------------------
# Step 2: Label storage node
# ---------------------------------------------------------------------------
label_storage_node() {
  header "Step 2/9: Label storage node"

  info "Stateful services (PostgreSQL, Keycloak) are pinned to a node with"
  info "the label storage=true so persistent volumes stay on one disk."
  echo ""

  # Check if any node already has the label
  local labeled_nodes
  labeled_nodes=$(docker node ls --filter "node.label=storage=true" --format '{{.Hostname}}' 2>/dev/null || true)

  if [[ -n "$labeled_nodes" ]]; then
    success "Storage node already labeled: ${labeled_nodes}"
    return 0
  fi

  # List available nodes
  echo "  Available nodes:"
  echo ""
  docker node ls --format '  {{.Hostname}}\t{{.Status}}\t{{.ManagerStatus}}' 2>/dev/null
  echo ""

  read -rp "  Enter the hostname to label as storage node: " storage_node
  if [[ -z "$storage_node" ]]; then
    warn "Skipped — no node labeled. Stateful services won't start without this."
    return 0
  fi

  docker node update --label-add storage=true "$storage_node"
  success "Labeled '${storage_node}' with storage=true"
}

# ---------------------------------------------------------------------------
# Step 3: Create overlay network
# ---------------------------------------------------------------------------
create_network() {
  header "Step 3/9: Create traefik-public overlay network"

  if docker network inspect traefik-public &>/dev/null; then
    success "Network traefik-public already exists."
    return 0
  fi

  docker network create --driver overlay --attachable traefik-public
  success "Created overlay network: traefik-public"
}

# ---------------------------------------------------------------------------
# Step 4: Create Swarm secrets
# ---------------------------------------------------------------------------
create_secrets() {
  header "Step 4/9: Create Swarm secrets"

  # Check if all 6 secrets exist
  local existing=0
  for name in staging_db_password staging_kc_db_password staging_kc_admin_password \
              prod_db_password prod_kc_db_password prod_kc_admin_password; do
    if docker secret inspect "$name" &>/dev/null; then
      existing=$((existing + 1))
    fi
  done

  if [[ $existing -eq 6 ]]; then
    success "All 6 Swarm secrets already exist."
    return 0
  fi

  if [[ $existing -gt 0 ]]; then
    info "${existing}/6 secrets exist. The script will skip existing ones."
  fi

  echo ""
  bash "${SCRIPT_DIR}/setup-swarm-secrets.sh"
  success "Swarm secrets configured."
}

# ---------------------------------------------------------------------------
# Step 5: Deploy base stack (Traefik)
# ---------------------------------------------------------------------------
deploy_base_stack() {
  header "Step 5/9: Deploy base stack (Traefik)"

  # Check if already deployed
  if docker stack services fh-base &>/dev/null 2>&1; then
    local service_count
    service_count=$(docker stack services fh-base --format '{{.Name}}' 2>/dev/null | wc -l)
    if [[ $service_count -gt 0 ]]; then
      success "Base stack (fh-base) already deployed with ${service_count} service(s)."
      if ask_yes_no "Redeploy to pick up config changes?" "n"; then
        docker stack deploy -c "${SWARM_DIR}/docker-stack.base.yml" fh-base
        success "Base stack redeployed."
      fi
      return 0
    fi
  fi

  docker stack deploy -c "${SWARM_DIR}/docker-stack.base.yml" fh-base
  success "Base stack deployed."

  info "Waiting for Traefik to start..."
  sleep 10

  # Verify
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 http://localhost:8080/api/overview 2>/dev/null || echo "000")
  if [[ "$status" == "200" ]]; then
    success "Traefik dashboard accessible at http://localhost:8080"
  else
    warn "Traefik not yet responding (HTTP ${status}). It may need a few more seconds."
  fi
}

# ---------------------------------------------------------------------------
# Step 6: Deploy GitHub Actions runner (optional)
# ---------------------------------------------------------------------------
deploy_runner() {
  header "Step 6/9: Deploy GitHub Actions self-hosted runner"

  if docker service inspect github-runner &>/dev/null; then
    success "GitHub runner service already exists."
    if ! ask_yes_no "Recreate the runner?" "n"; then
      return 0
    fi
  fi

  if ! ask_yes_no "Deploy a self-hosted GitHub Actions runner?" "y"; then
    warn "Skipped — you can run this later with: RUNNER_TOKEN=<token> bash infrastructure/scripts/setup-runner.sh"
    return 0
  fi

  info "Get a registration token from:"
  info "  GitHub → Repository Settings → Actions → Runners → New self-hosted runner"
  echo ""
  read -rp "  Enter GitHub runner registration token: " runner_token

  if [[ -z "$runner_token" ]]; then
    warn "Skipped — no token provided."
    return 0
  fi

  RUNNER_TOKEN="$runner_token" bash "${SCRIPT_DIR}/setup-runner.sh"
  success "GitHub runner deployed."
}

# ---------------------------------------------------------------------------
# Step 7: Deploy staging stack (optional)
# ---------------------------------------------------------------------------
deploy_staging() {
  header "Step 7/9: Deploy staging stack"

  if ! ask_yes_no "Deploy the staging environment now?" "y"; then
    warn "Skipped — deploy later with: bash infrastructure/scripts/deploy.sh staging <image-tag>"
    return 0
  fi

  # Check if images are available
  info "The staging stack needs container images from ghcr.io."
  info "If this is the first deployment and no images have been built yet,"
  info "you can build them locally or wait for the CI/CD pipeline."
  echo ""

  read -rp "  Image tag to deploy [latest]: " image_tag
  image_tag="${image_tag:-latest}"

  # Check if we can pull the images
  info "Checking image availability..."
  local pull_ok=true
  for img in api frontend; do
    if ! docker image inspect "ghcr.io/andrekirst/family2/${img}:${image_tag}" &>/dev/null; then
      if ! docker pull "ghcr.io/andrekirst/family2/${img}:${image_tag}" 2>/dev/null; then
        fail "Cannot find ghcr.io/andrekirst/family2/${img}:${image_tag}"
        pull_ok=false
      fi
    fi
  done

  if [[ "$pull_ok" == "false" ]]; then
    warn "Images not available. You may need to:"
    info "  1. Log in: echo '<PAT>' | docker login ghcr.io -u <user> --password-stdin"
    info "  2. Build and push images first (push to main triggers CI/CD)"
    info "  3. Or build locally — see the deployment guide (infrastructure/swarm/README.md)"
    echo ""
    if ! ask_yes_no "Try deploying anyway?" "n"; then
      return 0
    fi
  fi

  # Get the staging DB password for env var injection
  info "The staging stack needs database passwords as environment variables"
  info "for Keycloak (which doesn't support _FILE secrets)."
  echo ""
  read -rsp "  Staging DB password (same as Swarm secret): " staging_db_pw
  echo ""
  read -rsp "  Staging Keycloak DB password: " staging_kc_db_pw
  echo ""
  read -rsp "  Staging Keycloak admin password: " staging_kc_admin_pw
  echo ""

  export IMAGE_TAG="$image_tag"
  export STAGING_DB_PASSWORD="$staging_db_pw"
  export STAGING_KC_DB_PASSWORD="$staging_kc_db_pw"
  export STAGING_KC_ADMIN_PASSWORD="$staging_kc_admin_pw"

  docker stack deploy \
    -c "${SWARM_DIR}/docker-stack.staging.yml" \
    --with-registry-auth \
    fh-staging

  success "Staging stack deployed."
  info "Services starting — Keycloak may take 2+ minutes on first boot."
  echo ""

  # Quick health poll
  info "Waiting for services to start..."
  sleep 15

  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 http://staging.familyhub.local/health 2>/dev/null || echo "000")
  if [[ "$status" == "200" ]]; then
    success "API health check passed."
  else
    info "API not yet responding (HTTP ${status}) — this is normal for first deployment."
    info "Monitor with: docker stack services fh-staging"
  fi
}

# ---------------------------------------------------------------------------
# Step 8: Provision Keycloak staging realm (optional)
# ---------------------------------------------------------------------------
provision_keycloak() {
  header "Step 8/9: Provision Keycloak staging realm"

  # Check if staging stack is deployed
  if ! docker stack services fh-staging &>/dev/null 2>&1; then
    warn "Staging stack not deployed — skipping Keycloak provisioning."
    info "Run later with: KC_ADMIN_PASS=<pw> bash infrastructure/scripts/provision-swarm-realm.sh staging"
    return 0
  fi

  if ! ask_yes_no "Provision the Keycloak realm for staging?" "y"; then
    warn "Skipped — run later with: KC_ADMIN_PASS=<pw> bash infrastructure/scripts/provision-swarm-realm.sh staging"
    return 0
  fi

  info "Checking if Keycloak is ready..."
  local kc_ready=false
  for i in $(seq 1 12); do
    local kc_status
    kc_status=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 http://auth-staging.familyhub.local/realms/master 2>/dev/null || echo "000")
    if [[ "$kc_status" == "200" ]]; then
      kc_ready=true
      break
    fi
    info "Keycloak not ready yet (HTTP ${kc_status}) — waiting 15s... (${i}/12)"
    sleep 15
  done

  if [[ "$kc_ready" == "false" ]]; then
    warn "Keycloak did not become ready within 3 minutes."
    info "Run later with: KC_ADMIN_PASS=<pw> bash infrastructure/scripts/provision-swarm-realm.sh staging"
    return 0
  fi

  read -rsp "  Staging Keycloak admin password: " kc_pass
  echo ""

  KC_ADMIN_PASS="$kc_pass" bash "${SCRIPT_DIR}/provision-swarm-realm.sh" staging
  success "Keycloak staging realm provisioned."
}

# ---------------------------------------------------------------------------
# Step 9: Summary and next steps
# ---------------------------------------------------------------------------
print_summary() {
  header "Step 9/9: Summary"

  echo -e "  ${BOLD}Installed components:${NC}"
  echo ""

  # Check what's running
  for stack in fh-base fh-staging fh-production; do
    local count
    count=$(docker stack services "$stack" --format '{{.Name}}' 2>/dev/null | wc -l || echo "0")
    if [[ $count -gt 0 ]]; then
      success "${stack}: ${count} service(s)"
    else
      info "${stack}: not deployed"
    fi
  done

  if docker service inspect github-runner &>/dev/null; then
    success "github-runner: running"
  else
    info "github-runner: not deployed"
  fi

  local secret_count
  secret_count=$(docker secret ls --format '{{.Name}}' 2>/dev/null | wc -l || echo "0")
  info "Swarm secrets: ${secret_count}"

  # /etc/hosts
  echo ""
  echo -e "  ${BOLD}/etc/hosts entries:${NC}"
  echo ""

  local manager_ip
  manager_ip=$(docker info --format '{{.Swarm.NodeAddr}}' 2>/dev/null || echo "<manager-ip>")

  echo "  Add to /etc/hosts on every client machine:"
  echo ""
  echo -e "  ${CYAN}${manager_ip} staging.familyhub.local api-staging.familyhub.local auth-staging.familyhub.local mail-staging.familyhub.local${NC}"
  echo -e "  ${CYAN}${manager_ip} app.familyhub.local api.familyhub.local auth.familyhub.local traefik.familyhub.local${NC}"

  # Next steps
  echo ""
  echo -e "  ${BOLD}Next steps:${NC}"
  echo ""
  echo "  1. Add /etc/hosts entries on your development machine (see above)"
  echo "  2. Configure GitHub environments and secrets"
  echo "     → Repository Settings → Environments: staging (no rules), production (reviewers)"
  echo "     → Repository Settings → Secrets: STAGING_DB_PASSWORD, STAGING_KC_DB_PASSWORD, etc."
  echo "  3. Push to main to trigger the first automated staging deployment"
  echo "  4. Verify: http://staging.familyhub.local"
  echo ""
  echo "  For the full deployment guide, see:"
  echo "    infrastructure/swarm/README.md"
  echo ""
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
echo ""
echo -e "  ${BOLD}╔════════════════════════════════════════════════╗${NC}"
echo -e "  ${BOLD}║   FamilyHub — Docker Swarm Installation       ║${NC}"
echo -e "  ${BOLD}╚════════════════════════════════════════════════╝${NC}"
echo ""
echo "  This script will walk you through the initial Swarm setup."
echo "  Each step can be skipped. Safe to re-run (idempotent)."
echo ""

if ! ask_yes_no "Continue?" "y"; then
  echo "  Aborted."
  exit 0
fi

check_prerequisites
label_storage_node
create_network
create_secrets
deploy_base_stack
deploy_runner
deploy_staging
provision_keycloak
print_summary
