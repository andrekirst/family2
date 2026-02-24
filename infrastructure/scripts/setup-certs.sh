#!/usr/bin/env bash
# Generate wildcard TLS certificates for *.localhost and *.dev.andrekirst.de using mkcert.
# Idempotent: skips generation if certs already exist. Use --force to regenerate.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERT_DIR="${SCRIPT_DIR}/../certs"

mkdir -p "$CERT_DIR"

if [[ "${1:-}" != "--force" && -f "$CERT_DIR/local.pem" && -f "$CERT_DIR/local-key.pem" ]]; then
  echo "Certificates already exist at $CERT_DIR — skipping. Use --force to regenerate."
  exit 0
fi

if ! command -v mkcert &>/dev/null; then
  echo "Error: mkcert is not installed."
  echo "Install it: https://github.com/FiloSottile/mkcert#installation"
  exit 1
fi

# Ensure the local CA is installed in the system trust store
mkcert -install

# Generate wildcard certificate covering all service domains
# Shared: auth.localhost, mail.localhost, hub.localhost, secrets.localhost, npm/nuget
# Per-env: *.localhost (catches {env}.localhost, api-{env}.localhost, pgadmin-{env}.localhost)
# Dual-domain: same for *.dev.andrekirst.de
mkcert \
  -cert-file "$CERT_DIR/local.pem" \
  -key-file "$CERT_DIR/local-key.pem" \
  "*.localhost" "localhost" \
  "auth.localhost" "mail.localhost" "hub.localhost" \
  "npm.localhost" "nuget.localhost" "secrets.localhost" \
  "*.dev.andrekirst.de" "dev.andrekirst.de" \
  "auth.dev.andrekirst.de" "mail.dev.andrekirst.de" \
  "hub.dev.andrekirst.de" "secrets.dev.andrekirst.de" \
  "127.0.0.1" "::1"

echo "Certificates generated at $CERT_DIR"
