# Family Hub - Deployment Guide

## Quick Start (Local Docker)

Family Hub v2 is packaged as a single Docker container containing both the Angular frontend and .NET backend.

### Prerequisites

- Docker & Docker Compose installed
- Git repository cloned

### 1. Configure Environment

```bash
# Copy example environment file
cp .env.production.example .env

# Edit .env and set your values (especially JWT_SECRET and POSTGRES_PASSWORD)
nano .env
```

**Required Variables:**

- `POSTGRES_PASSWORD` - Database password (change from default!)
- `JWT_SECRET` - JWT signing key (min 32 characters, use random string)

### 2. Start the Stack

```bash
# Build and start (first time)
docker-compose -f docker-compose.production.yml up -d --build

# Or just start (if already built)
docker-compose -f docker-compose.production.yml up -d
```

### 3. Access the Application

**Local Access:**

- Application: http://localhost:8080
- GraphQL Playground: http://localhost:8080/graphql
- Health Check: http://localhost:8080/health

**First User:**

1. Navigate to http://localhost:8080/register
2. Create account with email/password
3. Login at http://localhost:8080/login

---

## Remote Access (VPN/SSH Tunnel)

For users to access your local deployment remotely:

### Option 1: SSH Tunnel (Simplest)

Users can create an SSH tunnel to your machine:

```bash
# On user's machine
ssh -L 8080:localhost:8080 your-username@your-server-ip

# Then access http://localhost:8080 in their browser
```

### Option 2: Cloudflare Tunnel (Zero Config)

```bash
# Install cloudflared
# https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/install-and-setup/

# Create tunnel (one-time)
cloudflared tunnel create familyhub

# Run tunnel
cloudflared tunnel --url http://localhost:8080

# Share the generated URL (e.g., https://abc-123.trycloudflare.com)
```

### Option 3: ngrok (Quick & Easy)

```bash
# Install ngrok: https://ngrok.com/download

# Create tunnel
ngrok http 8080

# Share the generated URL (e.g., https://abc123.ngrok-free.app)
```

### Option 4: Tailscale VPN (Most Secure)

```bash
# Install Tailscale on your server and users' devices
# https://tailscale.com/download

# Users access via your Tailscale IP
# Example: http://100.64.x.x:8080
```

---

## Container Management

### View Logs

```bash
# All services
docker-compose -f docker-compose.production.yml logs -f

# Just the app
docker-compose -f docker-compose.production.yml logs -f app

# Just the database
docker-compose -f docker-compose.production.yml logs -f postgres
```

### Restart Services

```bash
# Restart everything
docker-compose -f docker-compose.production.yml restart

# Restart just the app (after code changes)
docker-compose -f docker-compose.production.yml up -d --build app
```

### Stop Services

```bash
# Stop (keeps data)
docker-compose -f docker-compose.production.yml stop

# Stop and remove containers (keeps data)
docker-compose -f docker-compose.production.yml down

# Stop and remove everything including data (CAUTION!)
docker-compose -f docker-compose.production.yml down -v
```

---

## Database Access

### Connect to PostgreSQL

```bash
# From host machine
docker exec -it familyhub-db psql -U familyhub -d familyhub

# View users
SELECT id, email, email_verified, created_at FROM users;
```

### Backup Database

```bash
# Create backup
docker exec familyhub-db pg_dump -U familyhub familyhub > backup-$(date +%Y%m%d).sql

# Restore backup
docker exec -i familyhub-db psql -U familyhub familyhub < backup-20260130.sql
```

---

## Development vs Production

### Current Setup

| Service | Development | Production |
|---------|-------------|------------|
| Backend | Localhost:7001 | Docker:8080 |
| Frontend | Localhost:4200 | Docker:8080 (same) |
| Database | Localhost:5432 | Docker:5432 |
| Services | PostgreSQL only | PostgreSQL only |

### Switch to Development

```bash
# Stop production stack
docker-compose -f docker-compose.production.yml down

# Start dev infrastructure
cd infrastructure/docker
docker-compose up -d

# Run backend (terminal 1)
cd ../../src/api-v2/FamilyHub.Api
dotnet run

# Run frontend (terminal 2)
cd ../../../src/frontend/family-hub-web
npm start
```

---

## Troubleshooting

### App Container Fails to Start

```bash
# Check logs
docker logs familyhub-app

# Common issues:
# 1. Database not ready → Wait for postgres health check
# 2. Missing environment variables → Check .env file
# 3. Port 8080 in use → Change port mapping in docker-compose.yml
```

### Database Connection Errors

```bash
# Verify database is healthy
docker ps | grep familyhub-db

# Check connection from app container
docker exec familyhub-app sh -c "ping postgres"

# Verify connection string
docker exec familyhub-app env | grep ConnectionStrings
```

### Frontend Not Loading

```bash
# Verify wwwroot exists in container
docker exec familyhub-app ls -la /app/wwwroot

# Check if static files are served
curl http://localhost:8080/index.html
```

---

## File Structure

```
family2/
├── docker-compose.production.yml    # Production stack definition
├── .env                              # Environment variables (gitignored)
├── .env.production.example           # Example configuration
├── src/
│   ├── api/FamilyHub.Api/
│   │   ├── Dockerfile                # Multi-stage build
│   │   ├── Program.cs                # Serves static files from wwwroot/
│   │   ├── appsettings.Production.json
│   │   └── ...                       # 40 C# files
│   └── frontend/family-hub-web/
│       └── ...                       # 18 TypeScript files
└── infrastructure/docker/
    └── docker-compose.yml            # Dev infrastructure (PostgreSQL only)
```

---

## Security Notes

**For Weekend Testing (Current Setup):**

- ✅ JWT tokens for authentication
- ✅ BCrypt password hashing
- ✅ HTTPS not configured (localhost/VPN access only)
- ⚠️  Change `JWT_SECRET` in .env to a random 32+ character string
- ⚠️  Change `POSTGRES_PASSWORD` from default

**For Public Deployment (Future):**

- Add HTTPS with Let's Encrypt
- Use proper secrets management (Azure Key Vault, AWS Secrets Manager)
- Enable rate limiting
- Add WAF/firewall rules
- Configure backup automation

---

## Weekend Testing Checklist

- [ ] Clone repository on server
- [ ] Create `.env` from `.env.production.example`
- [ ] Set secure `JWT_SECRET` and `POSTGRES_PASSWORD`
- [ ] Run `docker-compose -f docker-compose.production.yml up -d`
- [ ] Access http://localhost:8080 and verify it works
- [ ] Setup VPN/SSH tunnel for remote access
- [ ] Share access URL/instructions with test users
- [ ] Test complete flow: Register → Verify Email → Login → Explore mockups

---

**Last Updated**: 2026-01-31
**Version**: 2.0 (Weekend Restart)
