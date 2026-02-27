#!/usr/bin/env bash
# Provision a Keycloak realm for Docker Swarm environments (staging/production).
#
# Usage:
#   provision-swarm-realm.sh staging              # Create realm FamilyHub-staging
#   provision-swarm-realm.sh production            # Create realm FamilyHub-production
#   provision-swarm-realm.sh staging --delete      # Delete realm
#
# Environment variables:
#   KC_URL              - Keycloak base URL (default: auto-detect from env name)
#   KC_ADMIN            - Admin username (default: admin)
#   KC_ADMIN_PASS       - Admin password (required for Swarm)
#
# Requires: curl, jq, sed
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEMPLATE="${SCRIPT_DIR}/../keycloak/realm-swarm.json"

ENV_NAME="${1:?Usage: provision-swarm-realm.sh <staging|production> [--delete]}"
DELETE_FLAG="${2:-}"
REALM_NAME="FamilyHub-${ENV_NAME}"

KC_ADMIN="${KC_ADMIN:-admin}"
KC_ADMIN_PASS="${KC_ADMIN_PASS:?KC_ADMIN_PASS is required for Swarm provisioning}"

MAX_RETRIES=30
RETRY_INTERVAL=5

# ---------------------------------------------------------------------------
# Determine frontend hostname based on environment
# ---------------------------------------------------------------------------
get_frontend_host() {
  case "$ENV_NAME" in
    staging)    echo "staging.familyhub.local" ;;
    production) echo "app.familyhub.local" ;;
    *)          echo "${ENV_NAME}.familyhub.local" ;;
  esac
}

# ---------------------------------------------------------------------------
# Auto-detect Keycloak URL based on environment
# ---------------------------------------------------------------------------
detect_kc_url() {
  if [[ -n "${KC_URL:-}" ]]; then
    echo "$KC_URL"
    return
  fi
  case "$ENV_NAME" in
    staging)    echo "http://auth-staging.familyhub.local" ;;
    production) echo "http://auth.familyhub.local" ;;
    *)          echo "http://auth-${ENV_NAME}.familyhub.local" ;;
  esac
}

KC_URL="$(detect_kc_url)"
FRONTEND_HOST="$(get_frontend_host)"
CURL_FLAGS="-s"

# ---------------------------------------------------------------------------
# Wait for Keycloak to be ready
# ---------------------------------------------------------------------------
wait_for_keycloak() {
  echo "  Waiting for Keycloak at ${KC_URL}..."
  for i in $(seq 1 $MAX_RETRIES); do
    local http_code
    http_code=$(curl $CURL_FLAGS -o /dev/null -w "%{http_code}" "${KC_URL}/realms/master" 2>/dev/null || echo "000")
    if [[ "$http_code" == "200" ]]; then
      echo "  Keycloak is ready (HTTP 200)."
      return 0
    fi
    echo "  Attempt ${i}/${MAX_RETRIES} (HTTP ${http_code}) — retrying in ${RETRY_INTERVAL}s..."
    sleep "$RETRY_INTERVAL"
  done
  echo "  ERROR: Keycloak did not become ready after $((MAX_RETRIES * RETRY_INTERVAL))s."
  exit 1
}

# ---------------------------------------------------------------------------
# Get admin access token
# ---------------------------------------------------------------------------
get_admin_token() {
  local response
  response=$(curl $CURL_FLAGS -X POST "${KC_URL}/realms/master/protocol/openid-connect/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "grant_type=password&client_id=admin-cli&username=${KC_ADMIN}&password=${KC_ADMIN_PASS}" 2>/dev/null)
  echo "$response" | jq -r '.access_token'
}

# ---------------------------------------------------------------------------
# Delete realm if it exists
# ---------------------------------------------------------------------------
delete_realm() {
  local token="$1"
  local status
  status=$(curl $CURL_FLAGS -o /dev/null -w "%{http_code}" \
    -X GET "${KC_URL}/admin/realms/${REALM_NAME}" \
    -H "Authorization: Bearer ${token}" 2>&1 || true)

  if [[ "$status" == "200" ]]; then
    echo "  Deleting existing realm ${REALM_NAME}..."
    curl $CURL_FLAGS -X DELETE "${KC_URL}/admin/realms/${REALM_NAME}" \
      -H "Authorization: Bearer ${token}"
    echo "  Realm ${REALM_NAME} deleted."
  else
    echo "  Realm ${REALM_NAME} does not exist (HTTP ${status})."
  fi
}

# ---------------------------------------------------------------------------
# Create realm from template
# ---------------------------------------------------------------------------
create_realm() {
  local token="$1"

  if [[ ! -f "$TEMPLATE" ]]; then
    echo "  ERROR: Realm template not found at ${TEMPLATE}"
    exit 1
  fi

  echo "  Creating realm ${REALM_NAME} from template..."

  local realm_json
  realm_json=$(sed \
    -e "s/__REALM_NAME__/${REALM_NAME}/g" \
    -e "s/__FRONTEND_HOST__/${FRONTEND_HOST}/g" \
    "$TEMPLATE")

  echo "$realm_json" | curl $CURL_FLAGS -X POST "${KC_URL}/admin/realms" \
    -H "Authorization: Bearer ${token}" \
    -H "Content-Type: application/json" \
    -d @-

  echo "  Realm ${REALM_NAME} created successfully."
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
echo ""
echo "  Keycloak Swarm Realm Provisioning"
echo "  ──────────────────────────────────────────"
echo "  Environment:   ${ENV_NAME}"
echo "  Realm:         ${REALM_NAME}"
echo "  Keycloak URL:  ${KC_URL}"
echo "  Frontend Host: ${FRONTEND_HOST}"
echo ""

wait_for_keycloak

TOKEN=$(get_admin_token)
if [[ -z "$TOKEN" || "$TOKEN" == "null" ]]; then
  echo "  ERROR: Failed to get admin token from Keycloak."
  exit 1
fi

if [[ "$DELETE_FLAG" == "--delete" ]]; then
  delete_realm "$TOKEN"
  echo ""
  echo "  Done."
  exit 0
fi

# Create: delete first (idempotent recreate)
delete_realm "$TOKEN"
create_realm "$TOKEN"

echo ""
echo "  Realm ready:"
echo "    Admin: ${KC_URL}/admin/master/console/#/${REALM_NAME}"
echo "    OIDC:  ${KC_URL}/realms/${REALM_NAME}"
echo ""
echo "  Test users: testowner / test123 (family-owner)"
echo "              testmember / test123 (family-member)"
echo ""
