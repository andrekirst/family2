#!/usr/bin/env bash
# ============================================================================
# FamilyHub — Docker Swarm Bootstrap
# ============================================================================
#
# One-liner to set up a fresh Turing Pi 2 (or any Docker Swarm node):
#
#   curl -sSL https://raw.githubusercontent.com/andrekirst/family2/main/infrastructure/scripts/bootstrap-swarm.sh | bash
#
# Or with a custom install directory:
#
#   curl -sSL https://raw.githubusercontent.com/andrekirst/family2/main/infrastructure/scripts/bootstrap-swarm.sh | INSTALL_DIR=/opt/familyhub bash
#
# What this script does:
#   1. Checks basic prerequisites (git, docker, curl, jq)
#   2. Clones the repository (or pulls latest if already cloned)
#   3. Hands off to the interactive install-swarm.sh
#
# ============================================================================
set -euo pipefail

REPO_URL="${REPO_URL:-https://github.com/andrekirst/family2.git}"
BRANCH="${BRANCH:-main}"
INSTALL_DIR="${INSTALL_DIR:-/opt/familyhub}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m'

info()    { echo -e "  ${CYAN}▸${NC} $*"; }
success() { echo -e "  ${GREEN}✓${NC} $*"; }
warn()    { echo -e "  ${YELLOW}!${NC} $*"; }
fail()    { echo -e "  ${RED}✗${NC} $*"; exit 1; }

echo ""
echo -e "  ${BOLD}╔════════════════════════════════════════════════╗${NC}"
echo -e "  ${BOLD}║   FamilyHub — Swarm Bootstrap                 ║${NC}"
echo -e "  ${BOLD}╚════════════════════════════════════════════════╝${NC}"
echo ""

# ---------------------------------------------------------------------------
# Check basic prerequisites
# ---------------------------------------------------------------------------
info "Checking prerequisites..."

command -v git    &>/dev/null || fail "git not found. Install: sudo apt install git"
command -v docker &>/dev/null || fail "Docker not found. Install: https://docs.docker.com/get-docker/"
command -v curl   &>/dev/null || fail "curl not found. Install: sudo apt install curl"
command -v jq     &>/dev/null || fail "jq not found. Install: sudo apt install jq"

# Check Swarm mode
swarm_status=$(docker info --format '{{.Swarm.LocalNodeState}}' 2>/dev/null || echo "inactive")
if [[ "$swarm_status" != "active" ]]; then
  fail "Docker Swarm not active. Initialize first:\n\n  docker swarm init --advertise-addr <this-node-ip>\n"
fi

success "All prerequisites met."

# ---------------------------------------------------------------------------
# Clone or update repository
# ---------------------------------------------------------------------------
echo ""
info "Install directory: ${INSTALL_DIR}"
info "Repository:        ${REPO_URL}"
info "Branch:            ${BRANCH}"
echo ""

if [[ -d "${INSTALL_DIR}/.git" ]]; then
  info "Repository already exists — pulling latest..."
  git -C "$INSTALL_DIR" fetch origin "$BRANCH"
  git -C "$INSTALL_DIR" checkout "$BRANCH"
  git -C "$INSTALL_DIR" pull origin "$BRANCH"
  success "Repository updated."
else
  info "Cloning repository..."
  mkdir -p "$(dirname "$INSTALL_DIR")"
  git clone --branch "$BRANCH" --single-branch "$REPO_URL" "$INSTALL_DIR"
  success "Repository cloned to ${INSTALL_DIR}"
fi

# ---------------------------------------------------------------------------
# Hand off to install script
# ---------------------------------------------------------------------------
echo ""
info "Starting installation..."
echo ""

exec bash "${INSTALL_DIR}/infrastructure/scripts/install-swarm.sh"
