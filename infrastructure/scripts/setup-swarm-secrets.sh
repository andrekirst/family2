#!/usr/bin/env bash
# Create Docker Swarm secrets for staging and production environments.
#
# Usage: Run on the Swarm manager node.
#   bash setup-swarm-secrets.sh
#
# Creates secrets:
#   staging_db_password, staging_kc_db_password, staging_kc_admin_password
#   prod_db_password, prod_kc_db_password, prod_kc_admin_password
set -euo pipefail

echo ""
echo "  Docker Swarm Secret Setup"
echo "  ──────────────────────────────────────────"
echo ""

create_secret() {
  local name="$1"
  local prompt="$2"

  # Skip if secret already exists
  if docker secret inspect "$name" &>/dev/null; then
    echo "  Secret '$name' already exists — skipping."
    return 0
  fi

  read -rsp "  ${prompt}: " value
  echo ""

  if [[ -z "$value" ]]; then
    echo "  ERROR: Empty value for '$name'. Aborting."
    exit 1
  fi

  echo -n "$value" | docker secret create "$name" -
  echo "  Created secret: $name"
}

echo "  === Staging Secrets ==="
echo ""
create_secret "staging_db_password"       "Staging PostgreSQL password"
create_secret "staging_kc_db_password"    "Staging Keycloak DB password"
create_secret "staging_kc_admin_password" "Staging Keycloak admin password"

echo ""
echo "  === Production Secrets ==="
echo ""
create_secret "prod_db_password"       "Production PostgreSQL password"
create_secret "prod_kc_db_password"    "Production Keycloak DB password"
create_secret "prod_kc_admin_password" "Production Keycloak admin password"

echo ""
echo "  All secrets created. Verify with: docker secret ls"
echo ""
