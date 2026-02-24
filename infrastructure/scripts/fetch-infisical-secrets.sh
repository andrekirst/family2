#!/usr/bin/env bash
# Fetch Google OAuth secrets from Infisical and output export statements.
#
# Supports two authentication modes:
#   1. Service Token:    export INFISICAL_SERVICE_TOKEN=st.xxx
#   2. Universal Auth:   export INFISICAL_CLIENT_ID=... INFISICAL_CLIENT_SECRET=... INFISICAL_PROJECT_ID=...
#
# Usage:
#   eval "$(bash infrastructure/scripts/fetch-infisical-secrets.sh)"
#
# The script outputs nothing (and exits 0) if Infisical is not configured
# or unreachable, so it's safe to source unconditionally.
set -euo pipefail

INFISICAL_URL="${INFISICAL_URL:-http://localhost:8180}"

# ---------------------------------------------------------------------------
# Mode 1: Service Token (INFISICAL_SERVICE_TOKEN)
# ---------------------------------------------------------------------------
if [[ -n "${INFISICAL_SERVICE_TOKEN:-}" ]]; then
  SECRETS_RESPONSE=$(curl -sf --max-time 5 \
    -H "Authorization: Bearer ${INFISICAL_SERVICE_TOKEN}" \
    "${INFISICAL_URL}/api/v3/secrets/raw?environment=dev&secretPath=%2F" \
  ) || exit 0

  python3 << 'PYEOF' - "$SECRETS_RESPONSE"
import json, sys

KEY_MAP = {
    "GoogleIntegration__OAuth__ClientId": "GOOGLE_CLIENT_ID",
    "GoogleIntegration__OAuth__ClientSecret": "GOOGLE_CLIENT_SECRET",
    "GoogleIntegration__EncryptionKey": "GOOGLE_ENCRYPTION_KEY",
}

try:
    data = json.loads(sys.argv[1])
    secrets = data.get("secrets", [])
    for s in secrets:
        key = s.get("secretKey", "")
        val = s.get("secretValue", "")
        if key in KEY_MAP and val:
            safe_val = val.replace("'", "'\"'\"'")
            print(f"export {KEY_MAP[key]}='{safe_val}'")
except Exception:
    pass
PYEOF
  exit 0
fi

# ---------------------------------------------------------------------------
# Mode 2: Universal Auth (INFISICAL_CLIENT_ID + INFISICAL_CLIENT_SECRET)
# ---------------------------------------------------------------------------
if [[ -z "${INFISICAL_CLIENT_ID:-}" || -z "${INFISICAL_CLIENT_SECRET:-}" || -z "${INFISICAL_PROJECT_ID:-}" ]]; then
  exit 0
fi

AUTH_RESPONSE=$(curl -sf --max-time 5 \
  -X POST "${INFISICAL_URL}/api/v1/auth/universal-auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"clientId\":\"${INFISICAL_CLIENT_ID}\",\"clientSecret\":\"${INFISICAL_CLIENT_SECRET}\"}" \
) || exit 0

ACCESS_TOKEN=$(python3 -c "import json,sys; print(json.load(sys.stdin)['accessToken'])" <<< "$AUTH_RESPONSE" 2>/dev/null) || exit 0

if [[ -z "$ACCESS_TOKEN" ]]; then
  exit 0
fi

SECRETS_RESPONSE=$(curl -sf --max-time 5 \
  -H "Authorization: Bearer ${ACCESS_TOKEN}" \
  "${INFISICAL_URL}/api/v4/secrets?projectId=${INFISICAL_PROJECT_ID}&environment=dev&secretPath=%2F&viewSecretValue=true" \
) || exit 0

python3 << 'PYEOF' - "$SECRETS_RESPONSE"
import json, sys

KEY_MAP = {
    "GoogleIntegration__OAuth__ClientId": "GOOGLE_CLIENT_ID",
    "GoogleIntegration__OAuth__ClientSecret": "GOOGLE_CLIENT_SECRET",
    "GoogleIntegration__EncryptionKey": "GOOGLE_ENCRYPTION_KEY",
}

try:
    data = json.loads(sys.argv[1])
    secrets = data.get("secrets", [])
    for s in secrets:
        key = s.get("secretKey", "")
        val = s.get("secretValue", "")
        if key in KEY_MAP and val:
            safe_val = val.replace("'", "'\"'\"'")
            print(f"export {KEY_MAP[key]}='{safe_val}'")
except Exception:
    pass
PYEOF
