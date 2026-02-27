#!/usr/bin/env bash
# Deploy a self-hosted GitHub Actions runner as a Docker Swarm service.
#
# Usage:
#   RUNNER_TOKEN=<token> bash setup-runner.sh
#
# The runner token can be obtained from:
#   GitHub → Repository Settings → Actions → Runners → New self-hosted runner
#
# Prerequisites:
#   - Docker Swarm initialized
#   - Running on the manager node
set -euo pipefail

RUNNER_TOKEN="${RUNNER_TOKEN:?RUNNER_TOKEN is required. Get it from GitHub → Settings → Actions → Runners}"
REPO_URL="${REPO_URL:-https://github.com/andrekirst/family2}"
RUNNER_NAME="${RUNNER_NAME:-turing-pi-runner}"
RUNNER_LABELS="${RUNNER_LABELS:-self-hosted,linux,arm64}"

echo ""
echo "  GitHub Actions Runner Setup (Swarm Service)"
echo "  ──────────────────────────────────────────"
echo "  Repository: ${REPO_URL}"
echo "  Runner:     ${RUNNER_NAME}"
echo "  Labels:     ${RUNNER_LABELS}"
echo ""

# Remove existing service if it exists
if docker service inspect github-runner &>/dev/null; then
  echo "  Removing existing runner service..."
  docker service rm github-runner
  sleep 5
fi

docker service create \
  --name github-runner \
  --constraint 'node.role == manager' \
  --mount type=bind,source=/var/run/docker.sock,target=/var/run/docker.sock \
  --env REPO_URL="${REPO_URL}" \
  --env RUNNER_TOKEN="${RUNNER_TOKEN}" \
  --env RUNNER_NAME="${RUNNER_NAME}" \
  --env RUNNER_LABELS="${RUNNER_LABELS}" \
  --env RUNNER_SCOPE="repo" \
  --env EPHEMERAL="false" \
  --env DISABLE_AUTO_UPDATE="true" \
  --restart-condition any \
  --restart-delay 10s \
  myoung34/github-runner:latest

echo ""
echo "  Runner service created. Verify with:"
echo "    docker service logs github-runner"
echo "    docker service ps github-runner"
echo ""
echo "  The runner should appear in GitHub → Settings → Actions → Runners"
echo ""
