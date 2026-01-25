#!/bin/bash
# Family Hub - DevContainer Post-Create Setup

set -e

echo "=== Family Hub DevContainer Setup ==="

# Install Playwright browsers
echo "Installing Playwright browsers..."
cd /workspace/src/frontend/family-hub-web
npx playwright install --with-deps chromium

# Restore .NET packages
echo "Restoring .NET packages..."
cd /workspace/src/api
dotnet restore

# Install npm packages
echo "Installing npm packages..."
cd /workspace/src/frontend/family-hub-web
npm ci

# Trust HTTPS certificates
echo "Setting up HTTPS development certificates..."
dotnet dev-certs https --trust 2>/dev/null || true

# Wait for services to be ready
echo "Waiting for infrastructure services..."
for i in {1..30}; do
  if pg_isready -h postgres -U familyhub -q 2>/dev/null; then
    echo "PostgreSQL is ready!"
    break
  fi
  echo "Waiting for PostgreSQL... ($i/30)"
  sleep 2
done

# Apply EF Core migrations
echo "Applying database migrations..."
cd /workspace/src/api
dotnet ef database update --project FamilyHub.Infrastructure --startup-project FamilyHub.Api 2>/dev/null || echo "Migrations will be applied on first run"

echo "=== DevContainer Setup Complete ==="
echo ""
echo "Available services:"
echo "  - PostgreSQL:      localhost:5432"
echo "  - RabbitMQ:        localhost:15672 (admin/Dev123!)"
echo "  - Zitadel (OAuth): localhost:8080"
echo "  - MailHog:         localhost:8025"
echo "  - Seq (Logging):   localhost:5341"
echo "  - Redis:           localhost:6379"
echo ""
echo "To start development:"
echo "  Backend:  cd src/api && dotnet run --project FamilyHub.Api"
echo "  Frontend: cd src/frontend/family-hub-web && npm start"
