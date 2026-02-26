#!/usr/bin/env bash
# Manual deployment helper for Docker Swarm environments.
#
# Usage:
#   deploy.sh staging [IMAGE_TAG]       Deploy to staging
#   deploy.sh production [IMAGE_TAG]    Deploy to production
#
# IMAGE_TAG defaults to "latest" if not specified.
#
# This script:
#   1. Runs EF Core migrations against the target database
#   2. Deploys the stack with the specified image tag
#   3. Verifies service health
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SWARM_DIR="${SCRIPT_DIR}/../swarm"

ENV="${1:?Usage: deploy.sh <staging|production> [IMAGE_TAG]}"
IMAGE_TAG="${2:-latest}"

# Validate environment
case "$ENV" in
  staging)
    STACK_NAME="fh-staging"
    STACK_FILE="${SWARM_DIR}/docker-stack.staging.yml"
    NETWORK="staging-internal"
    DB_HOST="fh-staging_postgres"
    HEALTH_URL="http://staging.familyhub.local/health"
    ;;
  production)
    STACK_NAME="fh-production"
    STACK_FILE="${SWARM_DIR}/docker-stack.production.yml"
    NETWORK="prod-internal"
    DB_HOST="fh-production_postgres"
    HEALTH_URL="http://app.familyhub.local/health"
    ;;
  *)
    echo "  ERROR: Unknown environment '$ENV'. Use 'staging' or 'production'."
    exit 1
    ;;
esac

echo ""
echo "  FamilyHub Deployment"
echo "  ──────────────────────────────────────────"
echo "  Environment: ${ENV}"
echo "  Stack:       ${STACK_NAME}"
echo "  Image Tag:   ${IMAGE_TAG}"
echo ""

# ---------------------------------------------------------------------------
# Step 1: Deploy stack (creates network + starts DB if first deploy)
# ---------------------------------------------------------------------------
echo "  [1/3] Deploying stack..."
export IMAGE_TAG
docker stack deploy \
  -c "$STACK_FILE" \
  --with-registry-auth \
  "$STACK_NAME"

echo "  Waiting for database to be ready..."
for i in $(seq 1 24); do
  if docker exec "$(docker ps -q -f name="${STACK_NAME}_postgres")" pg_isready -U familyhub -d familyhub &>/dev/null; then
    echo "  Database is ready."
    break
  fi
  if [[ $i -eq 24 ]]; then
    echo "  WARNING: Database not ready after 120s. Attempting migration anyway..."
  fi
  echo "  Database not ready — waiting 5s... (${i}/24)"
  sleep 5
done

# ---------------------------------------------------------------------------
# Step 2: Run EF Core migrations
# ---------------------------------------------------------------------------
echo "  [2/3] Running database migrations..."

# Get the DB password from the environment variable matching the target env
DB_PASSWORD_VAR="${ENV^^}_DB_PASSWORD"
DB_PASSWORD="${!DB_PASSWORD_VAR}"

if [[ -z "$DB_PASSWORD" ]]; then
  echo "  ERROR: ${DB_PASSWORD_VAR} is not set. Export it before running this script."
  exit 1
fi

# Run migration container on the same overlay network as PostgreSQL
docker run --rm \
  --network "${STACK_NAME}_${NETWORK}" \
  -e "ConnectionStrings__DefaultConnection=Host=${DB_HOST};Port=5432;Database=familyhub;Username=familyhub;Password=${DB_PASSWORD}" \
  "ghcr.io/andrekirst/family2/api-migrate:${IMAGE_TAG}" \
  || { echo "  ERROR: Migration failed."; exit 1; }

echo "  Migrations complete."

# ---------------------------------------------------------------------------
# Step 3: Verify health
# ---------------------------------------------------------------------------
echo "  [3/3] Verifying service health..."

MAX_RETRIES=30
RETRY_INTERVAL=10

for i in $(seq 1 $MAX_RETRIES); do
  status=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 --max-time 10 "$HEALTH_URL" 2>/dev/null || echo "000")
  if [[ "$status" == "200" ]]; then
    echo "  Health check passed (HTTP 200)."
    break
  fi
  if [[ $i -eq $MAX_RETRIES ]]; then
    echo "  WARNING: Health check did not pass after $((MAX_RETRIES * RETRY_INTERVAL))s (HTTP ${status})."
    echo "  Check services: docker stack services ${STACK_NAME}"
    exit 1
  fi
  echo "  Attempt ${i}/${MAX_RETRIES} (HTTP ${status}) — retrying in ${RETRY_INTERVAL}s..."
  sleep "$RETRY_INTERVAL"
done

echo ""
echo "  Deployment complete."
echo "  Services: docker stack services ${STACK_NAME}"
echo ""
