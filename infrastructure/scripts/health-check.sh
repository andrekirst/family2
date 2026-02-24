#!/usr/bin/env bash
# Health check script for FamilyHub infrastructure.
#
# Usage:
#   health-check.sh shared                  Check shared services (Traefik, Keycloak, MailHog)
#   health-check.sh env <ENV_NAME>          Check per-environment services (API, Frontend)
#   health-check.sh all <ENV_NAME>          Check everything
#
# Exit code 0 = all healthy, 1 = at least one service unhealthy
set -euo pipefail

MODE="${1:?Usage: health-check.sh <shared|env|all> [ENV_NAME]}"
ENV_NAME="${2:-}"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
NC='\033[0m' # No color
BOLD='\033[1m'

FAILURES=0

# ---------------------------------------------------------------------------
# Check a URL endpoint (HTTP 200 = OK)
# ---------------------------------------------------------------------------
check_url() {
  local name="$1"
  local url="$2"
  local status

  printf "  %-30s " "$name"

  status=$(curl -s -o /dev/null -w "%{http_code}" -k --connect-timeout 5 --max-time 10 "$url" 2>/dev/null || echo "000")

  if [[ "$status" == "200" || "$status" == "204" || "$status" == "301" || "$status" == "302" ]]; then
    echo -e "${GREEN}OK${NC} (HTTP ${status})"
  else
    echo -e "${RED}FAILED${NC} (HTTP ${status})"
    FAILURES=$((FAILURES + 1))
  fi
}

# ---------------------------------------------------------------------------
# Check a Docker container health status
# ---------------------------------------------------------------------------
check_container() {
  local name="$1"
  local container="$2"

  printf "  %-30s " "$name"

  local health
  health=$(docker inspect --format='{{.State.Health.Status}}' "$container" 2>/dev/null || echo "not-found")

  case "$health" in
    healthy)
      echo -e "${GREEN}OK${NC} (healthy)"
      ;;
    starting)
      echo -e "${YELLOW}STARTING${NC}"
      ;;
    unhealthy)
      echo -e "${RED}UNHEALTHY${NC}"
      FAILURES=$((FAILURES + 1))
      ;;
    *)
      echo -e "${RED}NOT FOUND${NC}"
      FAILURES=$((FAILURES + 1))
      ;;
  esac
}

# ---------------------------------------------------------------------------
# Shared services health check
# ---------------------------------------------------------------------------
check_shared() {
  echo ""
  echo -e "  ${BOLD}Shared Services${NC}"
  echo "  ──────────────────────────────────────────"
  check_url "Traefik Dashboard" "http://localhost:8888/api/overview"
  check_url "Keycloak" "https://auth.localhost:4443/realms/master"
  check_url "MailHog" "https://mail.localhost:4443"
  check_url "Hub Dashboard" "https://hub.localhost:4443"
  check_url "npm Registry" "https://npm.localhost:4443"
  check_url "NuGet Gallery" "https://nuget.localhost:4443"
  echo ""
}

# ---------------------------------------------------------------------------
# Per-environment health check
# ---------------------------------------------------------------------------
check_env() {
  local env_name="$1"

  if [[ -z "$env_name" ]]; then
    echo "  ERROR: ENV_NAME required for env check."
    exit 1
  fi

  echo ""
  echo -e "  ${BOLD}Environment: ${env_name}${NC}"
  echo "  ──────────────────────────────────────────"
  check_url "API /health" "https://api-${env_name}.localhost:4443/health"
  check_url "Frontend" "https://${env_name}.localhost:4443"
  check_url "pgAdmin" "https://pgadmin-${env_name}.localhost:4443"
  check_url "Config Endpoint" "https://${env_name}.localhost:4443/config"

  # Verify Keycloak realm exists
  local realm_name="FamilyHub-${env_name}"
  printf "  %-30s " "Realm ${realm_name}"
  local realm_status
  realm_status=$(curl -s -o /dev/null -w "%{http_code}" -k --connect-timeout 5 --max-time 10 \
    "https://auth.localhost:4443/realms/${realm_name}/.well-known/openid-configuration" 2>/dev/null || echo "000")
  if [[ "$realm_status" == "200" ]]; then
    echo -e "${GREEN}OK${NC}"
  else
    echo -e "${RED}NOT FOUND${NC} (HTTP ${realm_status})"
    FAILURES=$((FAILURES + 1))
  fi

  echo ""
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
echo ""
echo -e "  ${BOLD}FamilyHub Health Check${NC}"

case "$MODE" in
  shared)
    check_shared
    ;;
  env)
    check_env "$ENV_NAME"
    ;;
  all)
    check_shared
    check_env "$ENV_NAME"
    ;;
  *)
    echo "  Usage: health-check.sh <shared|env|all> [ENV_NAME]"
    exit 1
    ;;
esac

if [[ $FAILURES -eq 0 ]]; then
  echo -e "  ${GREEN}${BOLD}All checks passed.${NC}"
else
  echo -e "  ${RED}${BOLD}${FAILURES} check(s) failed.${NC}"
fi

echo ""
exit $FAILURES
