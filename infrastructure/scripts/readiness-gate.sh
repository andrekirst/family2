#!/usr/bin/env bash
# =============================================================================
# Readiness Gate — blocks until all services are verified operational
#
# Polls services through their external-facing Traefik URLs (not just Docker
# internal health) to ensure the full routing chain is working.
#
# Usage:
#   readiness-gate.sh <env-name> [OPTIONS]
#
# Options:
#   --timeout=N        Maximum wait time in seconds (default: 300)
#   --auto-recover     Enable auto-recovery for known failure patterns
#   --verbose          Show detailed check output
#   --skip-oidc        Skip full OIDC token exchange test
# =============================================================================
set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
ENV_NAME="${1:?Usage: readiness-gate.sh <env-name> [--timeout=N] [--auto-recover] [--verbose] [--skip-oidc]}"
shift

TIMEOUT=300
AUTO_RECOVER=false
VERBOSE=false
SKIP_OIDC=false

for arg in "$@"; do
  case "$arg" in
    --timeout=*) TIMEOUT="${arg#*=}" ;;
    --auto-recover) AUTO_RECOVER=true ;;
    --verbose) VERBOSE=true ;;
    --skip-oidc) SKIP_OIDC=true ;;
    *) echo "Unknown option: $arg"; exit 1 ;;
  esac
done

COMPOSE_PROJECT="fh-${ENV_NAME}"
BASE_URL="https://${ENV_NAME}.localhost:4443"
API_URL="https://api-${ENV_NAME}.localhost:4443"
AUTH_URL="https://auth.localhost:4443"
REALM="FamilyHub-${ENV_NAME}"

# Curl options: insecure (self-signed certs), silent, fail on HTTP errors
CURL_OPTS=(-sk --connect-timeout 5 --max-time 10)

# ---------------------------------------------------------------------------
# ANSI colors (only if terminal supports it)
# ---------------------------------------------------------------------------
if [[ -t 1 ]]; then
  GREEN='\033[0;32m'
  YELLOW='\033[0;33m'
  RED='\033[0;31m'
  CYAN='\033[0;36m'
  BOLD='\033[1m'
  DIM='\033[2m'
  RESET='\033[0m'
else
  GREEN='' YELLOW='' RED='' CYAN='' BOLD='' DIM='' RESET=''
fi

# ---------------------------------------------------------------------------
# State tracking
# ---------------------------------------------------------------------------
declare -A STATUS=( [postgres]="--" [keycloak]="--" [api]="--" [frontend]="--" [auth]="--" )
declare -A RECOVERED=()  # Track auto-recovery attempts (max 1 per service)
START_TIME=$(date +%s)

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
log_verbose() {
  if $VERBOSE; then
    echo -e "  ${DIM}$*${RESET}"
  fi
}

render_status() {
  local line="  Waiting:"
  for svc in postgres keycloak api frontend auth; do
    local s="${STATUS[$svc]}"
    case "$s" in
      ok) line+=" ${GREEN}[ok]${RESET} $svc" ;;
      ..) line+=" ${YELLOW}[..]${RESET} $svc" ;;
      --) line+=" ${DIM}[--]${RESET} $svc" ;;
      !) line+="  ${RED}[!!]${RESET} $svc" ;;
    esac
  done

  local elapsed=$(( $(date +%s) - START_TIME ))
  line+="  ${DIM}(${elapsed}s)${RESET}"

  if [[ -t 1 ]]; then
    printf "\r\033[K%b" "$line"
  else
    echo -e "$line"
  fi
}

elapsed() {
  echo $(( $(date +%s) - START_TIME ))
}

all_ok() {
  for svc in postgres keycloak api frontend auth; do
    [[ "${STATUS[$svc]}" == "ok" ]] || return 1
  done
  return 0
}

# ---------------------------------------------------------------------------
# Auto-recovery actions (limited to 1 attempt per service)
# ---------------------------------------------------------------------------
try_recover() {
  local service="$1"
  local action="$2"

  if ! $AUTO_RECOVER; then return 1; fi
  if [[ -n "${RECOVERED[$service]:-}" ]]; then return 1; fi

  RECOVERED[$service]=1
  echo ""
  echo -e "  ${YELLOW}⟳ Auto-recovery:${RESET} $action"
  eval "$action" 2>&1 | sed 's/^/    /' || true
  return 0
}

# ---------------------------------------------------------------------------
# Check functions
# ---------------------------------------------------------------------------
check_postgres() {
  STATUS[postgres]=".."
  local container
  container=$(docker ps --filter "name=${COMPOSE_PROJECT}-postgres" --filter "status=running" --format '{{.Names}}' | head -1)
  if [[ -z "$container" ]]; then
    log_verbose "Postgres: no running container found"
    return 1
  fi

  local health
  health=$(docker inspect --format='{{.State.Health.Status}}' "$container" 2>/dev/null || echo "unknown")
  if [[ "$health" == "healthy" ]]; then
    STATUS[postgres]="ok"
    return 0
  fi

  log_verbose "Postgres: container health=$health"

  # Auto-recovery: restart if unhealthy for a while
  if [[ "$health" == "unhealthy" ]] && (( $(elapsed) > 120 )); then
    try_recover "postgres" "docker restart $container"
  fi
  return 1
}

check_keycloak() {
  STATUS[keycloak]=".."
  local oidc_url="${AUTH_URL}/realms/${REALM}/.well-known/openid-configuration"

  if curl "${CURL_OPTS[@]}" "$oidc_url" | grep -q "token_endpoint" 2>/dev/null; then
    STATUS[keycloak]="ok"
    return 0
  fi

  log_verbose "Keycloak: OIDC discovery failed for realm ${REALM}"

  # Auto-recovery: re-provision realm if Keycloak itself is healthy
  local kc_container
  kc_container=$(docker ps --filter "name=fh-shared-keycloak" --filter "status=running" --format '{{.Names}}' | head -1)
  if [[ -n "$kc_container" ]]; then
    local kc_health
    kc_health=$(docker inspect --format='{{.State.Health.Status}}' "$kc_container" 2>/dev/null || echo "unknown")
    if [[ "$kc_health" == "healthy" ]]; then
      local script_dir
      script_dir="$(cd "$(dirname "$0")/.." && pwd)/keycloak/provision-realm.sh"
      if [[ -f "$script_dir" ]]; then
        try_recover "keycloak" "bash $script_dir ${ENV_NAME}"
      fi
    fi
  fi
  return 1
}

check_api() {
  STATUS[api]=".."
  local health_url="${BASE_URL}/health"

  local http_code
  http_code=$(curl "${CURL_OPTS[@]}" -o /dev/null -w '%{http_code}' "$health_url" 2>/dev/null || echo "000")
  if [[ "$http_code" == "200" ]]; then
    STATUS[api]="ok"
    return 0
  fi

  log_verbose "API: health check returned HTTP $http_code"

  # Auto-recovery: restart container if stuck for a long time
  if (( $(elapsed) > 120 )); then
    local container
    container=$(docker ps --filter "name=${COMPOSE_PROJECT}-api" --filter "status=running" --format '{{.Names}}' | head -1)
    if [[ -n "$container" ]]; then
      try_recover "api" "docker restart $container"
    fi
  fi
  return 1
}

check_frontend() {
  STATUS[frontend]=".."
  local html
  html=$(curl "${CURL_OPTS[@]}" "${BASE_URL}/" 2>/dev/null || echo "")

  if echo "$html" | grep -qiE '<app-root>|<!doctype html>'; then
    STATUS[frontend]="ok"
    return 0
  fi

  log_verbose "Frontend: did not receive valid HTML from ${BASE_URL}/"

  # Auto-recovery: restart container if stuck for a long time
  if (( $(elapsed) > 180 )); then
    local container
    container=$(docker ps --filter "name=${COMPOSE_PROJECT}-frontend" --filter "status=running" --format '{{.Names}}' | head -1)
    if [[ -n "$container" ]]; then
      try_recover "frontend" "docker restart $container"
    fi
  fi
  return 1
}

check_auth() {
  STATUS[auth]=".."

  if $SKIP_OIDC; then
    # Lightweight check: verify /config returns valid JSON with correct realm
    local config_json
    config_json=$(curl "${CURL_OPTS[@]}" "${BASE_URL}/config" 2>/dev/null || echo "")
    if echo "$config_json" | grep -q "${REALM}" 2>/dev/null; then
      STATUS[auth]="ok"
      return 0
    fi
    log_verbose "Auth: /config did not contain realm ${REALM}"
    return 1
  fi

  # Full OIDC token exchange test
  local token_url="${AUTH_URL}/realms/${REALM}/protocol/openid-connect/token"
  local token_response
  token_response=$(curl "${CURL_OPTS[@]}" -X POST "$token_url" \
    -d "grant_type=password" \
    -d "client_id=familyhub-test" \
    -d "username=testowner" \
    -d "password=test123" \
    -d "scope=openid" 2>/dev/null || echo "")

  if echo "$token_response" | grep -q "access_token" 2>/dev/null; then
    # Optionally verify the token works against GraphQL
    local access_token
    access_token=$(echo "$token_response" | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)
    if [[ -n "$access_token" ]]; then
      local gql_response
      gql_response=$(curl "${CURL_OPTS[@]}" "${BASE_URL}/graphql" \
        -H "Authorization: Bearer $access_token" \
        -H "Content-Type: application/json" \
        -d '{"query":"{ __typename }"}' 2>/dev/null || echo "")
      if echo "$gql_response" | grep -q "__typename" 2>/dev/null; then
        STATUS[auth]="ok"
        return 0
      fi
      log_verbose "Auth: GraphQL introspection failed"
    fi
  fi

  log_verbose "Auth: OIDC token exchange failed"
  return 1
}

# ---------------------------------------------------------------------------
# Main loop
# ---------------------------------------------------------------------------
echo ""
echo -e "  ${BOLD}Readiness Gate${RESET} — Environment: ${CYAN}${ENV_NAME}${RESET}"
echo -e "  ${DIM}Timeout: ${TIMEOUT}s | Auto-recover: ${AUTO_RECOVER} | OIDC: $( $SKIP_OIDC && echo 'skip' || echo 'full' )${RESET}"
echo ""

while true; do
  # Check elapsed time
  if (( $(elapsed) >= TIMEOUT )); then
    echo ""
    echo ""
    echo -e "  ${RED}✗ Readiness gate timed out after ${TIMEOUT}s${RESET}"
    echo ""
    echo "  Failed services:"
    for svc in postgres keycloak api frontend auth; do
      if [[ "${STATUS[$svc]}" != "ok" ]]; then
        echo -e "    ${RED}✗${RESET} $svc"
      fi
    done
    echo ""
    echo "  Suggested fixes:"
    [[ "${STATUS[postgres]}" != "ok" ]] && echo "    - Check postgres: docker logs ${COMPOSE_PROJECT}-postgres-1"
    [[ "${STATUS[keycloak]}" != "ok" ]] && echo "    - Re-provision realm: bash infrastructure/keycloak/provision-realm.sh ${ENV_NAME}"
    [[ "${STATUS[api]}" != "ok" ]] && echo "    - Check API logs: task env:logs -- api"
    [[ "${STATUS[frontend]}" != "ok" ]] && echo "    - Check frontend logs: task env:logs -- frontend"
    [[ "${STATUS[auth]}" != "ok" ]] && echo "    - Verify Keycloak realm: ${AUTH_URL}/admin/master/console/#/${REALM}"
    echo ""
    exit 1
  fi

  # Run checks in dependency order — skip checks for services that are already OK
  [[ "${STATUS[postgres]}" != "ok" ]] && check_postgres || true
  [[ "${STATUS[keycloak]}" != "ok" ]] && { [[ "${STATUS[postgres]}" == "ok" ]] && check_keycloak || true; } || true
  [[ "${STATUS[api]}" != "ok" ]] && { [[ "${STATUS[postgres]}" == "ok" ]] && check_api || true; } || true
  [[ "${STATUS[frontend]}" != "ok" ]] && { [[ "${STATUS[api]}" == "ok" ]] && check_frontend || true; } || true
  [[ "${STATUS[auth]}" != "ok" ]] && { [[ "${STATUS[api]}" == "ok" && "${STATUS[keycloak]}" == "ok" && "${STATUS[frontend]}" == "ok" ]] && check_auth || true; } || true

  render_status

  if all_ok; then
    total_elapsed=$(( $(date +%s) - START_TIME ))
    echo ""
    echo ""
    echo -e "  ${GREEN}✓ All services ready in ${total_elapsed}s${RESET}"
    echo ""
    exit 0
  fi

  sleep 3
done
