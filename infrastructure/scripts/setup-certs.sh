#!/usr/bin/env bash
# Generate wildcard TLS certificates for *.localhost using mkcert.
# Idempotent: skips generation if certs already exist.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERT_DIR="${SCRIPT_DIR}/../certs"

mkdir -p "$CERT_DIR"

if [[ -f "$CERT_DIR/local.pem" && -f "$CERT_DIR/local-key.pem" ]]; then
  echo "Certificates already exist at $CERT_DIR — skipping."
  exit 0
fi

if ! command -v mkcert &>/dev/null; then
  echo "Error: mkcert is not installed."
  echo "Install it: https://github.com/FiloSottile/mkcert#installation"
  exit 1
fi

# Ensure the local CA is installed in the system trust store
mkcert -install

# Generate wildcard certificate for *.localhost
mkcert \
  -cert-file "$CERT_DIR/local.pem" \
  -key-file "$CERT_DIR/local-key.pem" \
  "*.localhost" "localhost" "127.0.0.1" "::1"

echo "Certificates generated at $CERT_DIR"
