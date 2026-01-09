# Local Development Setup

**Purpose:** Get Family Hub running on your local machine for development.

**Estimated Time:** 15-30 minutes (first-time setup)

---

## Prerequisites

Before starting, ensure you have these tools installed:

### Required

| Tool | Minimum Version | Purpose | Installation |
|------|----------------|---------|--------------|
| **Docker Desktop** | 24.0+ | Run local services (PostgreSQL, RabbitMQ, Zitadel) | [docker.com](https://www.docker.com/products/docker-desktop) |
| **Node.js** | 20.x LTS | Frontend build and development | [nodejs.org](https://nodejs.org/) |
| **.NET SDK** | 10.0+ | Backend API development | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| **Git** | 2.40+ | Version control | [git-scm.com](https://git-scm.com/) |

### Optional but Recommended

| Tool | Purpose |
|------|---------|
| **Visual Studio Code** | Code editor with extensions for Angular + C# |
| **Azure Data Studio** / **pgAdmin** | PostgreSQL database management |
| **Postman** / **Insomnia** | API testing (GraphQL) |

### Verify Installation

```bash
# Check versions
docker --version          # Should be 24.0+
node --version            # Should be v20.x
npm --version             # Should be 10.x+
dotnet --version          # Should be 10.0+
git --version             # Should be 2.40+
```

---

## Quick Start (5 Steps)

### 1. Clone Repository

```bash
git clone https://github.com/andrekirst/family2.git
cd family2
```

### 2. Start Infrastructure Services

```bash
# Navigate to Docker Compose directory
cd infrastructure/docker

# Start all services (PostgreSQL, RabbitMQ, Zitadel, Seq, MailHog)
docker-compose up -d

# Verify all services are healthy
docker-compose ps
```

**Expected Output:**

```
NAME                     STATUS       PORTS
familyhub-postgres       Up (healthy) 0.0.0.0:5432->5432/tcp
familyhub-rabbitmq       Up (healthy) 0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
familyhub-zitadel        Up           0.0.0.0:8080->8080/tcp
familyhub-seq            Up           0.0.0.0:5341->80/tcp
familyhub-mailhog        Up           0.0.0.0:1025->1025/tcp, 0.0.0.0:8025->8025/tcp
```

### 3. Set Up Backend (.NET API)

```bash
# Navigate to API project
cd ../../src/api

# Restore NuGet packages
dotnet restore

# Apply database migrations (creates schema + seed data)
dotnet ef database update --context AuthDbContext --project Modules/FamilyHub.Modules.Auth --startup-project FamilyHub.Api
# Repeat for other module DbContexts as they're added (FamilyDbContext, CalendarDbContext, etc.)

# Run backend API
dotnet run --project FamilyHub.Api
```

**Backend runs on:** `https://localhost:7000` (HTTPS) or `http://localhost:5000` (HTTP)

**GraphQL Playground:** `https://localhost:7000/graphql`

### 4. Set Up Frontend (Angular)

Open a **new terminal** (keep backend running):

```bash
# Navigate to frontend project
cd src/frontend/family-hub-web

# Install npm dependencies
npm install

# Start development server
npm start
```

**Frontend runs on:** `http://localhost:4200`

### 5. Verify Setup

1. **Open frontend:** http://localhost:4200
2. **Test GraphQL:** https://localhost:7000/graphql
3. **Check Zitadel:** http://localhost:8080 (admin@familyhub.local / Admin123!)
4. **Check RabbitMQ:** http://localhost:15672 (familyhub / Dev123!)
5. **Check Seq Logs:** http://localhost:5341
6. **Check MailHog:** http://localhost:8025

---

## Detailed Setup Guide

### Infrastructure Services (Docker Compose)

#### Service Overview

| Service | Port | Purpose | Credentials |
|---------|------|---------|-------------|
| **PostgreSQL** | 5432 | Primary database | familyhub / Dev123! |
| **RabbitMQ** | 5672 (AMQP)<br>15672 (UI) | Event message broker | familyhub / Dev123! |
| **Zitadel** | 8080 | OAuth 2.0 / OIDC provider | admin@familyhub.local / Admin123! |
| **Seq** | 5341 | Structured logging UI | (no auth in dev) |
| **MailHog** | 1025 (SMTP)<br>8025 (UI) | Fake SMTP server | (no auth) |

#### Environment Variables (Optional)

Create `.env` file in `infrastructure/docker/`:

```env
# PostgreSQL
POSTGRES_PASSWORD=YourSecurePassword

# RabbitMQ
RABBITMQ_PASSWORD=YourSecurePassword

# Zitadel
ZITADEL_MASTERKEY=MasterKey123!MustBe32Characters!  # Must be exactly 32 characters
ZITADEL_DB_PASSWORD=YourSecurePassword
ZITADEL_ADMIN_PASSWORD=YourSecurePassword
```

**Default Passwords (if .env not provided):**

- PostgreSQL: `Dev123!`
- RabbitMQ: `Dev123!`
- Zitadel Admin: `Admin123!`

#### Starting/Stopping Services

```bash
# Start all services (detached mode)
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose stop

# Stop and remove containers (preserves volumes)
docker-compose down

# Stop, remove containers AND volumes (clean slate)
docker-compose down -v
```

#### Health Checks

Wait 30-60 seconds after `docker-compose up` for services to initialize:

```bash
# Check service health
docker-compose ps

# Check PostgreSQL
docker exec familyhub-postgres pg_isready -U familyhub

# Check RabbitMQ
docker exec familyhub-rabbitmq rabbitmq-diagnostics ping
```

---

### Backend Setup (.NET API)

#### Database Migrations

Family Hub uses **EF Core Code-First** migrations. One DbContext per module:

```bash
# Navigate to API directory
cd src/api

# AuthModule (always run this first)
dotnet ef database update --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api

# When other modules are added, run their migrations:
# dotnet ef database update --context FamilyDbContext --project Modules/FamilyHub.Modules.Family --startup-project FamilyHub.Api
# dotnet ef database update --context CalendarDbContext --project Modules/FamilyHub.Modules.Calendar --startup-project FamilyHub.Api
```

**Verify Migrations:**

```bash
# Connect to PostgreSQL
docker exec -it familyhub-postgres psql -U familyhub -d familyhub

# List schemas
\dn

# Expected output:
# Name    | Owner
# --------+----------
# auth    | familyhub  (from AuthModule)
# family  | familyhub  (when FamilyModule added)
# public  | familyhub

# List tables in auth schema
\dt auth.*

# Exit
\q
```

#### Running the Backend

```bash
# Development mode (hot reload)
cd src/api
dotnet run --project FamilyHub.Api

# Or use watch mode (auto-restart on file changes)
dotnet watch --project FamilyHub.Api
```

**Backend URLs:**

- HTTPS: `https://localhost:7000`
- HTTP: `http://localhost:5000`
- GraphQL Playground: `https://localhost:7000/graphql`
- Swagger (if enabled): `https://localhost:7000/swagger`

---

### Frontend Setup (Angular)

#### Install Dependencies

```bash
cd src/frontend/family-hub-web

# Install npm packages (first time or after pulling updates)
npm install
```

#### Running the Frontend

```bash
# Development server (hot reload)
npm start

# Or with Angular CLI directly
ng serve

# With specific port
ng serve --port 4300
```

**Frontend URLs:**

- Dev Server: `http://localhost:4200`
- Auto-opens in browser by default

#### Build Production Bundle

```bash
# Production build (optimized)
npm run build:prod

# Output: dist/family-hub-web/
```

---

### Zitadel OAuth Configuration

**Zitadel** provides OAuth 2.0 / OIDC authentication for Family Hub.

#### Initial Setup (First-Time Only)

1. **Access Zitadel Console:** http://localhost:8080
2. **Login:**
   - Username: `admin@familyhub.local`
   - Password: `Admin123!` (or your custom password from .env)

#### Create OAuth Application (Manual - Phase 0)

**⚠️ This will be automated in Phase 1 via API configuration**

1. Navigate to: **Projects** → **FamilyHub** (auto-created)
2. Click **New Application**
3. **Application Details:**
   - Name: `Family Hub Web`
   - Type: `USER_AGENT` (for Angular SPA)
4. **Redirect URIs:**
   - Add: `http://localhost:4200/callback`
   - Add: `http://localhost:4200/silent-renew` (for token refresh)
5. **Post Logout Redirect URIs:**
   - Add: `http://localhost:4200`
6. **Application Settings:**
   - Auth Method: `PKCE` (Proof Key for Code Exchange)
   - Enable: Response Type `CODE`
   - Enable: Grant Type `AUTHORIZATION_CODE`
7. **Save** and copy **Client ID**

#### Configure Frontend with Client ID

Update `src/frontend/family-hub-web/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7000/graphql',
  zitadel: {
    issuer: 'http://localhost:8080',
    clientId: '<YOUR_CLIENT_ID>', // Paste from Zitadel
    redirectUri: 'http://localhost:4200/callback',
    postLogoutRedirectUri: 'http://localhost:4200',
    scope: 'openid profile email'
  }
};
```

**See Also:** `docs/authentication/OAUTH_INTEGRATION_GUIDE.md` for complete OAuth setup.

---

## Testing Your Setup

### Backend API Tests

```bash
# Navigate to API directory
cd src/api

# Run all unit tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportsFormat=lcov
```

### Frontend Tests

```bash
# Navigate to frontend directory
cd src/frontend/family-hub-web

# Run unit tests (Karma + Jasmine)
npm test

# Run E2E tests (Playwright)
npm run e2e

# Run E2E in headless mode (CI)
npm run e2e:headless
```

### Manual GraphQL Query

1. Open GraphQL Playground: `https://localhost:7000/graphql`
2. Test a query:

```graphql
query {
  __schema {
    queryType {
      name
    }
  }
}
```

1. If OAuth is configured, you'll need an access token (see docs/authentication/OAUTH_INTEGRATION_GUIDE.md)

---

## Common Issues & Troubleshooting

### Issue: Docker services won't start

**Symptoms:** `docker-compose up` fails or services show `Exited` status

**Solutions:**

```bash
# Check if ports are already in use
lsof -i :5432  # PostgreSQL
lsof -i :5672  # RabbitMQ
lsof -i :8080  # Zitadel

# Stop conflicting services or change ports in docker-compose.yml

# Clean up and restart
docker-compose down -v
docker-compose up -d
```

### Issue: Zitadel won't start (stuck on "waiting for database")

**Symptoms:** Zitadel logs show `waiting for postgres` indefinitely

**Solutions:**

1. Ensure PostgreSQL is fully healthy (not just "Up"):

   ```bash
   docker-compose ps postgres
   # Should show "Up (healthy)"
   ```

2. Check Zitadel logs:

   ```bash
   docker-compose logs zitadel
   ```

3. Verify Zitadel database exists:

   ```bash
   docker exec -it familyhub-postgres psql -U familyhub -l
   # Should list "zitadel" database
   ```

4. If stuck, recreate Zitadel:

   ```bash
   docker-compose stop zitadel
   docker-compose rm zitadel
   docker volume rm docker_postgres_data
   docker-compose up -d
   ```

### Issue: Backend can't connect to PostgreSQL

**Symptoms:** `dotnet run` fails with "password authentication failed" or "connection refused"

**Solutions:**

1. Check connection string in `src/api/FamilyHub.Api/appsettings.Development.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=familyhub;Username=familyhub;Password=Dev123!"
     }
   }
   ```

2. Verify PostgreSQL is running:

   ```bash
   docker-compose ps postgres
   ```

3. Test connection manually:

   ```bash
   docker exec -it familyhub-postgres psql -U familyhub -d familyhub
   ```

### Issue: Frontend can't connect to backend API

**Symptoms:** Browser console shows `ERR_CONNECTION_REFUSED` or CORS errors

**Solutions:**

1. Verify backend is running:

   ```bash
   # Should show GraphQL Playground
   curl https://localhost:7000/graphql -k
   ```

2. Check API URL in `src/frontend/family-hub-web/src/environments/environment.ts`:

   ```typescript
   apiUrl: 'https://localhost:7000/graphql'  // Must match backend URL
   ```

3. **CORS Error:** Ensure backend allows frontend origin. Check `src/api/FamilyHub.Api/Program.cs`:

   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowFrontend", policy =>
       {
           policy.WithOrigins("http://localhost:4200")
                 .AllowAnyMethod()
                 .AllowAnyHeader();
       });
   });
   ```

### Issue: EF Core migrations fail

**Symptoms:** `dotnet ef database update` fails with "could not connect to server"

**Solutions:**

1. Ensure PostgreSQL is healthy:

   ```bash
   docker-compose ps postgres
   ```

2. Verify connection string matches Docker Compose:

   ```bash
   # Check appsettings.Development.json
   cat src/api/FamilyHub.Api/appsettings.Development.json
   ```

3. Run migration with explicit connection string:

   ```bash
   dotnet ef database update --context AuthDbContext \
     --project Modules/FamilyHub.Modules.Auth \
     --startup-project FamilyHub.Api \
     --connection "Host=localhost;Port=5432;Database=familyhub;Username=familyhub;Password=Dev123!"
   ```

### Issue: RabbitMQ connection refused

**Symptoms:** Backend logs show "RabbitMQ.Client.Exceptions.BrokerUnreachableException"

**Solutions:**

1. Verify RabbitMQ is running and healthy:

   ```bash
   docker-compose ps rabbitmq
   docker-compose logs rabbitmq
   ```

2. Check RabbitMQ management UI: http://localhost:15672 (familyhub / Dev123!)

3. Verify backend configuration in `appsettings.Development.json`:

   ```json
   {
     "RabbitMQ": {
       "Host": "localhost",
       "Port": 5672,
       "Username": "familyhub",
       "Password": "Dev123!"
     }
   }
   ```

### Issue: npm install fails

**Symptoms:** `npm install` fails with dependency resolution errors

**Solutions:**

1. Clear npm cache:

   ```bash
   npm cache clean --force
   ```

2. Delete node_modules and package-lock.json:

   ```bash
   rm -rf node_modules package-lock.json
   npm install
   ```

3. Use correct Node version:

   ```bash
   node --version  # Should be v20.x LTS
   ```

### Issue: Port already in use

**Symptoms:** "address already in use" errors

**Solutions:**

```bash
# Find process using port
lsof -i :4200  # Frontend
lsof -i :7000  # Backend
lsof -i :5432  # PostgreSQL

# Kill process
kill -9 <PID>

# Or run on different port
ng serve --port 4300
dotnet run --urls "https://localhost:7001;http://localhost:5001"
```

---

## Development Workflow

### Typical Development Session

```bash
# 1. Start infrastructure (once per day or as needed)
cd infrastructure/docker
docker-compose up -d

# 2. Start backend (Terminal 1)
cd ../../src/api
dotnet watch --project FamilyHub.Api

# 3. Start frontend (Terminal 2)
cd ../frontend/family-hub-web
npm start

# 4. Open browser
# Frontend: http://localhost:4200
# GraphQL: https://localhost:7000/graphql
```

### Before Committing Code

```bash
# Format code (auto-runs via hooks)
cd src/frontend/family-hub-web
npm run format:json

# Run tests
cd ../../api
dotnet test
cd ../frontend/family-hub-web
npm test
npm run e2e:headless

# Check for linting issues
ng lint
```

### Pulling Latest Changes

```bash
# Pull from git
git pull origin main

# Update dependencies
cd src/api
dotnet restore
cd ../frontend/family-hub-web
npm install

# Apply new migrations
cd ../../api
dotnet ef database update --context AuthDbContext --project Modules/FamilyHub.Modules.Auth --startup-project FamilyHub.Api
# Repeat for other modules

# Restart services
docker-compose restart
```

---

## Next Steps

Once your local environment is running:

1. **Read Architecture:** [docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)
2. **Review Workflows:** [docs/development/WORKFLOWS.md](docs/development/WORKFLOWS.md)
3. **Understand Coding Standards:** [docs/development/CODING_STANDARDS.md](docs/development/CODING_STANDARDS.md)
4. **Implement First Feature:** [docs/development/IMPLEMENTATION_WORKFLOW.md](docs/development/IMPLEMENTATION_WORKFLOW.md)
5. **Configure OAuth:** [docs/authentication/OAUTH_INTEGRATION_GUIDE.md](docs/authentication/OAUTH_INTEGRATION_GUIDE.md)

---

## Additional Resources

- **Docker Compose File:** `infrastructure/docker/docker-compose.yml`
- **Backend API:** `src/api/FamilyHub.Api/`
- **Frontend App:** `src/frontend/family-hub-web/`
- **Database Docs:** `database/docs/`
- **Architecture Docs:** `docs/architecture/`

**Questions or Issues?** Check [docs/development/DEBUGGING_GUIDE.md](docs/development/DEBUGGING_GUIDE.md) or create an issue using the template in `.github/ISSUE_TEMPLATE/`.

---

**Last Updated:** 2026-01-09
**Version:** 1.0.0
