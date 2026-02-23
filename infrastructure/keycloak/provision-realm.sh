#!/usr/bin/env bash
# Provision a per-environment Keycloak realm via the Admin REST API.
#
# Usage:
#   provision-realm.sh <ENV_NAME>              # Create/replace realm FamilyHub-<ENV_NAME>
#   provision-realm.sh <ENV_NAME> --delete     # Delete realm FamilyHub-<ENV_NAME>
#
# Requires: curl, jq
# Keycloak must be running and healthy at http://keycloak:8080 (Docker network)
# or http://localhost:8080 (host network).
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEMPLATE="${SCRIPT_DIR}/realm-base.json"

ENV_NAME="${1:?Usage: provision-realm.sh <ENV_NAME> [--delete]}"
DELETE_FLAG="${2:-}"
REALM_NAME="FamilyHub-${ENV_NAME}"

# Keycloak base URL — auto-detect: try Traefik-proxied URL from host
# Override with KC_URL env var if running inside Docker network
KC_ADMIN="${KC_ADMIN:-admin}"
KC_ADMIN_PASS="${KC_ADMIN_PASS:-admin}"

MAX_RETRIES=30
RETRY_INTERVAL=5

# ---------------------------------------------------------------------------
# Auto-detect Keycloak URL
# ---------------------------------------------------------------------------
detect_kc_url() {
  if [[ -n "${KC_URL:-}" ]]; then
    echo "$KC_URL"
    return
  fi
  # Running on host — use Traefik-proxied HTTPS URL
  echo "https://auth.localhost:4443"
}

KC_URL="$(detect_kc_url)"
# curl flags: -k for self-signed certs when going through Traefik
CURL_FLAGS="-s"
if [[ "$KC_URL" == https://* ]]; then
  CURL_FLAGS="-sk"
fi

# ---------------------------------------------------------------------------
# Wait for Keycloak to be ready
# ---------------------------------------------------------------------------
wait_for_keycloak() {
  echo "  Waiting for Keycloak at ${KC_URL}..."
  for i in $(seq 1 $MAX_RETRIES); do
    # Use /realms/master as health probe — check HTTP 200 explicitly
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

  # Process template: replace placeholders
  local realm_json
  realm_json=$(sed \
    -e "s/__REALM_NAME__/${REALM_NAME}/g" \
    -e "s/__ENV_NAME__/${ENV_NAME}/g" \
    "$TEMPLATE")

  # Create realm via partial import (handles users + clients + roles)
  echo "$realm_json" | curl $CURL_FLAGS -X POST "${KC_URL}/admin/realms" \
    -H "Authorization: Bearer ${token}" \
    -H "Content-Type: application/json" \
    -d @-

  echo "  Realm ${REALM_NAME} created successfully."
}

# ---------------------------------------------------------------------------
# Configure Google IdP (optional — if credentials are available)
# ---------------------------------------------------------------------------
configure_google_idp() {
  local token="$1"

  # Check if Google OAuth credentials are provided via environment
  if [[ -z "${GOOGLE_CLIENT_ID:-}" || -z "${GOOGLE_CLIENT_SECRET:-}" ]]; then
    echo "  Skipping Google IdP (GOOGLE_CLIENT_ID / GOOGLE_CLIENT_SECRET not set)."
    return 0
  fi

  echo "  Configuring Google Identity Provider..."
  curl $CURL_FLAGS -X POST "${KC_URL}/admin/realms/${REALM_NAME}/identity-provider/instances" \
    -H "Authorization: Bearer ${token}" \
    -H "Content-Type: application/json" \
    -d "{
      \"alias\": \"google\",
      \"providerId\": \"google\",
      \"enabled\": true,
      \"trustEmail\": true,
      \"storeToken\": false,
      \"addReadTokenRoleOnCreate\": false,
      \"firstBrokerLoginFlowAlias\": \"first broker login\",
      \"config\": {
        \"clientId\": \"${GOOGLE_CLIENT_ID}\",
        \"clientSecret\": \"${GOOGLE_CLIENT_SECRET}\",
        \"defaultScope\": \"openid profile email\",
        \"syncMode\": \"IMPORT\"
      }
    }"
  echo "  Google IdP configured."
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
echo ""
echo "  Keycloak Realm Provisioning"
echo "  ──────────────────────────────────────────"
echo "  Environment: ${ENV_NAME}"
echo "  Realm:       ${REALM_NAME}"
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
configure_google_idp "$TOKEN"

echo ""
echo "  Realm ready:"
echo "    Admin:  https://auth.localhost:4443/admin/master/console/#/${REALM_NAME}"
echo "    OIDC:   https://auth.localhost:4443/realms/${REALM_NAME}"
echo ""
echo "  Test users: testowner / test123 (family-owner)"
echo "              testmember / test123 (family-member)"
echo ""
