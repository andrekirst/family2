# Zitadel Setup Guide - Family Hub OAuth Integration

**Version:** 1.0
**Last Updated:** 2024-12-22
**Audience:** Developers, DevOps Engineers

---

## Table of Contents

1. [Overview](#overview)
2. [Local Development Setup](#local-development-setup)
3. [Production Setup](#production-setup)
4. [Configuration Reference](#configuration-reference)
5. [Testing OAuth Flow](#testing-oauth-flow)
6. [Troubleshooting](#troubleshooting)
7. [Security Considerations](#security-considerations)

---

## Overview

Family Hub uses **Zitadel** as its OAuth 2.0 / OpenID Connect provider. Zitadel handles:
- User authentication (login/logout)
- Multi-factor authentication (TOTP, WebAuthn)
- Social login (Google, Microsoft, Apple)
- Session management
- Token issuance and validation

**OAuth Flow:** Authorization Code with PKCE (Proof Key for Code Exchange)

**Architecture:**
```
Angular App → Zitadel Login UI → Authorization Code → Backend API
Backend API → Validates JWT (RS256) → User Sync → Database
```

---

## Local Development Setup

### Prerequisites

- Docker Desktop installed
- .NET 8.0 SDK
- Node.js 18+ (for Angular frontend)
- Git

### Step 1: Start Zitadel with Docker Compose

**Create `docker-compose.zitadel.yml`:**

```yaml
version: '3.8'

services:
  zitadel-db:
    image: postgres:16-alpine
    container_name: zitadel-postgres
    restart: unless-stopped
    environment:
      POSTGRES_USER: zitadel
      POSTGRES_PASSWORD: zitadel-dev-password
      POSTGRES_DB: zitadel
    volumes:
      - zitadel_db_data:/var/lib/postgresql/data
    ports:
      - "5433:5432"
    networks:
      - zitadel-network

  zitadel:
    image: ghcr.io/zitadel/zitadel:v2.43.4
    container_name: zitadel
    restart: unless-stopped
    command: 'start-from-init --masterkey "MasterkeyNeedsToHave32Characters" --tlsMode disabled'
    environment:
      ZITADEL_DATABASE_POSTGRES_HOST: zitadel-db
      ZITADEL_DATABASE_POSTGRES_PORT: 5432
      ZITADEL_DATABASE_POSTGRES_DATABASE: zitadel
      ZITADEL_DATABASE_POSTGRES_USER_USERNAME: zitadel
      ZITADEL_DATABASE_POSTGRES_USER_PASSWORD: zitadel-dev-password
      ZITADEL_DATABASE_POSTGRES_USER_SSL_MODE: disable
      ZITADEL_DATABASE_POSTGRES_ADMIN_USERNAME: zitadel
      ZITADEL_DATABASE_POSTGRES_ADMIN_PASSWORD: zitadel-dev-password
      ZITADEL_DATABASE_POSTGRES_ADMIN_SSL_MODE: disable
      ZITADEL_EXTERNALSECURE: 'false'
      ZITADEL_EXTERNALPORT: 8080
      ZITADEL_EXTERNALDOMAIN: localhost
    depends_on:
      - zitadel-db
    ports:
      - "8080:8080"
    networks:
      - zitadel-network
    volumes:
      - zitadel_data:/data

volumes:
  zitadel_db_data:
    driver: local
  zitadel_data:
    driver: local

networks:
  zitadel-network:
    driver: bridge
```

**Start Zitadel:**

```bash
docker-compose -f docker-compose.zitadel.yml up -d

# Wait for Zitadel to initialize (takes ~30 seconds)
docker logs -f zitadel

# Look for: "server is running"
```

**Access Zitadel Console:**
- URL: http://localhost:8080
- First-time setup will prompt for admin account creation

### Step 2: Create Zitadel Project

**Login to Zitadel Console:**

1. Navigate to http://localhost:8080
2. Click **"Create Organization"** (if first time)
   - Organization Name: `Family Hub`
   - Click **Create**

3. Click **"Projects"** in left menu
4. Click **"+ New"**
   - Project Name: `Family Hub`
   - Click **Continue**

### Step 3: Create OAuth Application

**Within the "Family Hub" project:**

1. Click **"Applications"** tab
2. Click **"+ New"**

**Application Settings:**

| Field | Value |
|-------|-------|
| Application Name | `Family Hub Web` |
| Application Type | **Web** |
| Authentication Method | **PKCE** (Proof Key for Code Exchange) |

Click **Continue**

**Redirect URIs:**

| Type | URI |
|------|-----|
| Redirect URI | `http://localhost:4200/auth/callback` |
| Post Logout Redirect URI | `http://localhost:4200` |

Click **Continue**

**Review & Create:**
- Review settings
- Click **Create**

**Important:** Save the following values:

```
Client ID: <COPY_THIS_VALUE>
Client Secret: <COPY_THIS_VALUE>
```

⚠️ **Store Client Secret securely!** It will only be shown once.

### Step 4: Configure Backend (Family Hub API)

**Update `src/api/FamilyHub.Api/appsettings.Development.json`:**

```json
{
  "Zitadel": {
    "Authority": "http://localhost:8080",
    "ClientId": "<YOUR_CLIENT_ID>",
    "ClientSecret": "<YOUR_CLIENT_SECRET>",
    "RedirectUri": "http://localhost:4200/auth/callback",
    "Scopes": "openid profile email",
    "Audience": "family-hub-api"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=familyhub;Username=familyhub;Password=Dev123!"
  }
}
```

**Verify Configuration:**

```bash
cd src/api
dotnet run --project FamilyHub.Api

# Look for:
# [INF] Starting Family Hub API
# [INF] Family Hub API started successfully
```

### Step 5: Test OIDC Discovery

**Verify Zitadel's OpenID Connect configuration:**

```bash
curl http://localhost:8080/.well-known/openid-configuration | jq .

# Expected output includes:
# {
#   "issuer": "http://localhost:8080",
#   "authorization_endpoint": "http://localhost:8080/oauth/v2/authorize",
#   "token_endpoint": "http://localhost:8080/oauth/v2/token",
#   "jwks_uri": "http://localhost:8080/oauth/v2/keys",
#   ...
# }
```

**Verify JWKS (JSON Web Key Set):**

```bash
curl http://localhost:8080/oauth/v2/keys | jq .

# Expected output includes RSA public keys:
# {
#   "keys": [
#     {
#       "kty": "RSA",
#       "use": "sig",
#       "kid": "...",
#       "n": "...",
#       "e": "AQAB"
#     }
#   ]
# }
```

### Step 6: Configure Frontend (Angular)

**Create `src/app/services/zitadel-auth.service.ts`:**

```typescript
import { Injectable } from '@angular/core';
import { Apollo, gql } from 'apollo-angular';
import { Router } from '@angular/router';

const GET_ZITADEL_AUTH_URL = gql`
  query GetZitadelAuthUrl {
    getZitadelAuthUrl {
      authorizationUrl
      codeVerifier
      state
    }
  }
`;

const COMPLETE_ZITADEL_LOGIN = gql`
  mutation CompleteZitadelLogin($input: CompleteZitadelLoginInput!) {
    completeZitadelLogin(input: $input) {
      authenticationResult {
        user {
          id
          email
          emailVerified
        }
        accessToken
        expiresAt
      }
      errors {
        message
        code
      }
    }
  }
`;

@Injectable({
  providedIn: 'root'
})
export class ZitadelAuthService {
  constructor(
    private apollo: Apollo,
    private router: Router
  ) {}

  async initiateLogin() {
    try {
      const result = await this.apollo.query({
        query: GET_ZITADEL_AUTH_URL
      }).toPromise();

      const { authorizationUrl, codeVerifier, state } =
        result.data.getZitadelAuthUrl;

      // Store PKCE verifier and state
      sessionStorage.setItem('pkce_code_verifier', codeVerifier);
      sessionStorage.setItem('oauth_state', state);

      // Redirect to Zitadel
      window.location.href = authorizationUrl;
    } catch (error) {
      console.error('Failed to initiate login:', error);
      throw error;
    }
  }

  async handleCallback(code: string, returnedState: string) {
    try {
      // Validate state (CSRF protection)
      const storedState = sessionStorage.getItem('oauth_state');
      if (returnedState !== storedState) {
        throw new Error('Invalid state parameter - possible CSRF attack');
      }

      // Retrieve code verifier
      const codeVerifier = sessionStorage.getItem('pkce_code_verifier');
      if (!codeVerifier) {
        throw new Error('Missing PKCE code verifier');
      }

      // Complete login
      const result = await this.apollo.mutate({
        mutation: COMPLETE_ZITADEL_LOGIN,
        variables: {
          input: {
            authorizationCode: code,
            codeVerifier: codeVerifier
          }
        }
      }).toPromise();

      const { authenticationResult, errors } =
        result.data.completeZitadelLogin;

      if (errors && errors.length > 0) {
        throw new Error(errors[0].message);
      }

      // Store access token
      localStorage.setItem('access_token', authenticationResult.accessToken);
      localStorage.setItem('token_expires_at', authenticationResult.expiresAt);

      // Clear session storage
      sessionStorage.removeItem('pkce_code_verifier');
      sessionStorage.removeItem('oauth_state');

      return authenticationResult.user;
    } catch (error) {
      console.error('Failed to complete login:', error);
      throw error;
    }
  }

  logout() {
    localStorage.removeItem('access_token');
    localStorage.removeItem('token_expires_at');
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem('access_token');
    const expiresAt = localStorage.getItem('token_expires_at');

    if (!token || !expiresAt) return false;

    return new Date() < new Date(expiresAt);
  }

  getAccessToken(): string | null {
    return localStorage.getItem('access_token');
  }
}
```

**Create auth callback component `src/app/auth/callback/callback.component.ts`:**

```typescript
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ZitadelAuthService } from '../../services/zitadel-auth.service';

@Component({
  selector: 'app-auth-callback',
  template: `
    <div class="flex items-center justify-center min-h-screen">
      <div class="text-center">
        <div class="animate-spin rounded-full h-16 w-16 border-b-2 border-blue-600 mx-auto"></div>
        <p class="mt-4 text-gray-600">Completing login...</p>
      </div>
    </div>
  `
})
export class AuthCallbackComponent implements OnInit {
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: ZitadelAuthService
  ) {}

  async ngOnInit() {
    try {
      const params = this.route.snapshot.queryParams;
      const code = params['code'];
      const state = params['state'];
      const error = params['error'];

      if (error) {
        console.error('OAuth error:', params['error_description']);
        this.router.navigate(['/login'], {
          queryParams: { error: 'oauth_failed' }
        });
        return;
      }

      if (!code || !state) {
        console.error('Missing code or state parameter');
        this.router.navigate(['/login'], {
          queryParams: { error: 'invalid_callback' }
        });
        return;
      }

      const user = await this.authService.handleCallback(code, state);
      console.log('Login successful:', user);

      this.router.navigate(['/dashboard']);
    } catch (error) {
      console.error('Callback handling failed:', error);
      this.router.navigate(['/login'], {
        queryParams: { error: 'login_failed' }
      });
    }
  }
}
```

**Add route in `app-routing.module.ts`:**

```typescript
const routes: Routes = [
  { path: 'auth/callback', component: AuthCallbackComponent },
  // ... other routes
];
```

### Step 7: Test OAuth Flow

**1. Start Backend:**
```bash
cd src/api
dotnet run --project FamilyHub.Api
```

**2. Start Frontend:**
```bash
cd src/web
npm start
# Angular runs on http://localhost:4200
```

**3. Test Login Flow:**

1. Click **"Login with Zitadel"** button in Angular app
2. Should redirect to `http://localhost:8080/login`
3. Create account or login with existing credentials
4. Should redirect back to `http://localhost:4200/auth/callback?code=...&state=...`
5. Angular processes callback and stores access token
6. Should redirect to `/dashboard`

**4. Verify JWT in Browser DevTools:**

```javascript
// In browser console:
localStorage.getItem('access_token')

// Decode JWT (without verification):
const token = localStorage.getItem('access_token');
const [header, payload, signature] = token.split('.');
const decodedPayload = JSON.parse(atob(payload));
console.log(decodedPayload);

// Expected claims:
// {
//   "iss": "http://localhost:8080",
//   "sub": "<zitadel_user_id>",
//   "aud": "family-hub-api",
//   "exp": <timestamp>,
//   "iat": <timestamp>,
//   "email": "user@example.com",
//   ...
// }
```

**5. Test Authenticated API Call:**

```bash
# Get access token from browser localStorage
TOKEN="<YOUR_ACCESS_TOKEN>"

# Call authenticated GraphQL endpoint
curl -X POST http://localhost:5002/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"query":"query { me { id email } }"}'

# Expected response:
# {
#   "data": {
#     "me": {
#       "id": "6dc37d75-f300-4576-aef0-dfdd4f71edbb",
#       "email": "user@example.com"
#     }
#   }
# }
```

---

## Production Setup

### Prerequisites

- Production-ready Zitadel instance (cloud or self-hosted)
- SSL/TLS certificate for your domain
- Azure Key Vault or equivalent secrets management
- Kubernetes cluster (or Docker Compose for smaller deployments)

### Option 1: Zitadel Cloud (Recommended)

**1. Sign up at https://zitadel.com**

**2. Create Organization:**
- Organization Name: `Family Hub Production`

**3. Create Project:**
- Project Name: `Family Hub`

**4. Create Application:**
- Application Name: `Family Hub Web`
- Type: **Web** (PKCE)
- Redirect URIs:
  - `https://familyhub.app/auth/callback`
  - `https://www.familyhub.app/auth/callback`
- Post Logout URIs:
  - `https://familyhub.app`
  - `https://www.familyhub.app`

**5. Configure Custom Domain (Optional but Recommended):**
- Go to **Organization Settings** → **Domain**
- Add custom domain: `auth.familyhub.app`
- Verify DNS records
- Enable SSL

**6. Copy Credentials:**
- Client ID: `<PROD_CLIENT_ID>`
- Client Secret: `<PROD_CLIENT_SECRET>`
- Authority: `https://auth.familyhub.app` (or Zitadel-provided domain)

### Option 2: Self-Hosted Zitadel

**See Zitadel documentation:** https://zitadel.com/docs/self-hosting/deploy/overview

**Requirements:**
- PostgreSQL 12+
- 2 GB RAM minimum
- SSL/TLS certificate
- Reverse proxy (Nginx, Traefik, etc.)

**Docker Compose for Production:**

```yaml
version: '3.8'

services:
  zitadel:
    image: ghcr.io/zitadel/zitadel:v2.43.4
    restart: always
    command: 'start-from-init --masterkey "${ZITADEL_MASTERKEY}" --tlsMode external'
    environment:
      ZITADEL_DATABASE_POSTGRES_HOST: postgres
      ZITADEL_DATABASE_POSTGRES_PORT: 5432
      ZITADEL_DATABASE_POSTGRES_DATABASE: zitadel
      ZITADEL_DATABASE_POSTGRES_USER_USERNAME: ${DB_USER}
      ZITADEL_DATABASE_POSTGRES_USER_PASSWORD: ${DB_PASSWORD}
      ZITADEL_DATABASE_POSTGRES_USER_SSL_MODE: require
      ZITADEL_EXTERNALSECURE: 'true'
      ZITADEL_EXTERNALPORT: 443
      ZITADEL_EXTERNALDOMAIN: auth.familyhub.app
      ZITADEL_TLS_ENABLED: 'false' # TLS terminated by reverse proxy
    depends_on:
      - postgres
    networks:
      - zitadel-network

  postgres:
    image: postgres:16-alpine
    restart: always
    environment:
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      POSTGRES_DB: zitadel
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - zitadel-network

volumes:
  postgres_data:

networks:
  zitadel-network:
```

### Configure Production Backend

**Store secrets in Azure Key Vault:**

```bash
# Create Key Vault
az keyvault create \
  --name familyhub-secrets \
  --resource-group familyhub-prod \
  --location eastus

# Store Zitadel secrets
az keyvault secret set \
  --vault-name familyhub-secrets \
  --name "Zitadel--ClientId" \
  --value "<PROD_CLIENT_ID>"

az keyvault secret set \
  --vault-name familyhub-secrets \
  --name "Zitadel--ClientSecret" \
  --value "<PROD_CLIENT_SECRET>"
```

**Update `appsettings.Production.json`:**

```json
{
  "Zitadel": {
    "Authority": "https://auth.familyhub.app",
    "ClientId": "{{KEYVAULT:Zitadel--ClientId}}",
    "ClientSecret": "{{KEYVAULT:Zitadel--ClientSecret}}",
    "RedirectUri": "https://familyhub.app/auth/callback",
    "Scopes": "openid profile email",
    "Audience": "family-hub-api"
  },
  "ConnectionStrings": {
    "DefaultConnection": "{{KEYVAULT:ConnectionStrings--DefaultConnection}}"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Kubernetes Deployment with External Secrets:**

```yaml
apiVersion: external-secrets.io/v1beta1
kind: SecretStore
metadata:
  name: azure-keyvault
spec:
  provider:
    azurekv:
      authType: WorkloadIdentity
      vaultUrl: "https://familyhub-secrets.vault.azure.net"

---
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: familyhub-secrets
spec:
  secretStoreRef:
    name: azure-keyvault
  target:
    name: familyhub-secrets
  data:
    - secretKey: zitadel-client-id
      remoteRef:
        key: Zitadel--ClientId
    - secretKey: zitadel-client-secret
      remoteRef:
        key: Zitadel--ClientSecret
```

---

## Configuration Reference

### ZitadelSettings Properties

| Property | Description | Example | Required |
|----------|-------------|---------|----------|
| `Authority` | Zitadel instance URL | `http://localhost:8080` | ✅ Yes |
| `ClientId` | OAuth application client ID | `123456@family_hub` | ✅ Yes |
| `ClientSecret` | OAuth application secret | `abc123...` | ✅ Yes |
| `RedirectUri` | OAuth callback URL | `http://localhost:4200/auth/callback` | ✅ Yes |
| `Scopes` | OAuth scopes | `openid profile email` | ✅ Yes |
| `Audience` | JWT audience claim | `family-hub-api` | ✅ Yes |

### Environment Variables (Alternative Configuration)

```bash
# Override appsettings.json values
export Zitadel__Authority="http://localhost:8080"
export Zitadel__ClientId="<CLIENT_ID>"
export Zitadel__ClientSecret="<CLIENT_SECRET>"
export Zitadel__RedirectUri="http://localhost:4200/auth/callback"
export Zitadel__Audience="family-hub-api"
```

### JWT Claims Mapping

| Zitadel Claim | ASP.NET Core Claim Type | Description |
|---------------|-------------------------|-------------|
| `sub` | `ClaimTypes.NameIdentifier` | Zitadel user ID (external) |
| `email` | `ClaimTypes.Email` | User email address |
| `email_verified` | Custom | Email verification status |
| `name` | `ClaimTypes.Name` | User full name |
| `given_name` | `ClaimTypes.GivenName` | User first name |
| `family_name` | `ClaimTypes.Surname` | User last name |
| `picture` | Custom | User profile picture URL |

---

## Testing OAuth Flow

### Manual Testing Checklist

**1. Authorization URL Generation:**

```bash
# Test getZitadelAuthUrl query
curl -X POST http://localhost:5002/graphql \
  -H "Content-Type: application/json" \
  -d '{
    "query": "query { getZitadelAuthUrl { authorizationUrl codeVerifier state } }"
  }' | jq .

# Verify response contains:
# - authorizationUrl with code_challenge, code_challenge_method=S256, state, nonce
# - codeVerifier (base64url string)
# - state (random string)
```

**2. Manual OAuth Flow:**

```bash
# 1. Copy authorizationUrl from above
# 2. Open in browser (or use curl)
# 3. Login with test credentials
# 4. Zitadel redirects to: http://localhost:4200/auth/callback?code=...&state=...
# 5. Copy authorization code

# 6. Complete login
curl -X POST http://localhost:5002/graphql \
  -H "Content-Type: application/json" \
  -d '{
    "query": "mutation($input: CompleteZitadelLoginInput!) { completeZitadelLogin(input: $input) { authenticationResult { accessToken user { email } } errors { message } } }",
    "variables": {
      "input": {
        "authorizationCode": "<CODE_FROM_CALLBACK>",
        "codeVerifier": "<CODE_VERIFIER_FROM_STEP_1>"
      }
    }
  }' | jq .

# Expected: accessToken and user data
```

**3. Verify JWT:**

```bash
# Use jwt.io or jwt-cli
TOKEN="<ACCESS_TOKEN>"

# Decode header
echo $TOKEN | cut -d'.' -f1 | base64 -d | jq .
# Expected: {"alg":"RS256","kid":"...","typ":"JWT"}

# Decode payload
echo $TOKEN | cut -d'.' -f2 | base64 -d | jq .
# Expected: {"iss":"http://localhost:8080","sub":"...","aud":"family-hub-api",...}
```

**4. Test Authenticated API Call:**

```bash
curl -X POST http://localhost:5002/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"query":"query { me { id email } }"}' | jq .

# Expected: User data
```

### Integration Tests

```bash
cd src/api
dotnet test --filter "FullyQualifiedName~ZitadelOAuthFlowTests"

# Expected: 4/4 tests passing
```

---

## Troubleshooting

### Issue: "Zitadel settings are not configured"

**Error:**
```
System.InvalidOperationException: Zitadel settings are not configured
```

**Solution:**
1. Verify `appsettings.Development.json` contains `Zitadel` section
2. Ensure all required fields are filled
3. Restart API: `dotnet run`

---

### Issue: "The token is expired"

**Error:**
```
OnAuthenticationFailed: The token is expired
```

**Solution:**
1. Token lifetime is 1 hour (default)
2. Implement token refresh flow (frontend)
3. Or: Re-login via Zitadel

---

### Issue: "Audience validation failed"

**Error:**
```
IDX10214: Audience validation failed. Audiences: '...'
```

**Solution:**
1. Check `appsettings.json` → `Zitadel.Audience` matches JWT `aud` claim
2. In Zitadel console, verify application audience setting
3. Restart API after config change

---

### Issue: "Signature validation failed"

**Error:**
```
IDX10503: Signature validation failed. Keys tried: '...'
```

**Solution:**
1. Verify Zitadel is running: `curl http://localhost:8080/.well-known/openid-configuration`
2. Check JWKS endpoint: `curl http://localhost:8080/oauth/v2/keys`
3. Clear cached keys: Restart API
4. Ensure `Authority` URL matches issuer in JWT

---

### Issue: "Invalid state parameter"

**Error:**
```
Invalid state parameter - possible CSRF attack
```

**Solution:**
1. Frontend: Verify `state` from callback matches stored value
2. Check session storage: `sessionStorage.getItem('oauth_state')`
3. State expires after use - don't reuse OAuth URLs

---

### Issue: Docker Compose Zitadel not starting

**Symptoms:**
- Container exits immediately
- Logs show database connection errors

**Solution:**
```bash
# Check logs
docker logs zitadel

# Common issues:
# 1. PostgreSQL not ready
docker-compose -f docker-compose.zitadel.yml down
docker-compose -f docker-compose.zitadel.yml up -d

# 2. Port conflict (8080 already in use)
sudo lsof -i :8080
# Kill conflicting process or change port in docker-compose.yml

# 3. Masterkey too short
# Ensure masterkey is exactly 32 characters
```

---

## Security Considerations

### Production Checklist

- [ ] **HTTPS Enabled:** All communication over TLS 1.2+
- [ ] **Client Secret Secured:** Stored in Key Vault, never in source code
- [ ] **HSTS Enabled:** `app.UseHsts()` in ASP.NET Core
- [ ] **CORS Configured:** Only allow production frontend domains
- [ ] **Rate Limiting:** 10 requests/min per IP on OAuth endpoints
- [ ] **Logging:** Monitor failed authentication attempts
- [ ] **Token Expiration:** 1-hour access tokens, refresh token rotation
- [ ] **Zitadel 2FA:** Enforce for admin accounts

### OWASP OAuth 2.0 Compliance

✅ **Implemented:**
- PKCE (S256) - Prevents authorization code interception
- State parameter - CSRF protection
- Nonce parameter - Replay protection
- RS256 JWT validation - Asymmetric signing
- Audience validation - Token scope restriction
- Issuer validation - Trusted authority check
- Lifetime validation - Expired token rejection
- Secure token exchange - Server-side only

⚠️ **Pending:**
- HTTPS in production (required)
- Rate limiting (recommended)

### Rotating Client Secrets

**Zitadel Console:**
1. Navigate to **Applications** → `Family Hub Web`
2. Click **"Regenerate Secret"**
3. Copy new secret immediately
4. Update Key Vault:
   ```bash
   az keyvault secret set \
     --vault-name familyhub-secrets \
     --name "Zitadel--ClientSecret" \
     --value "<NEW_SECRET>"
   ```
5. Restart API pods:
   ```bash
   kubectl rollout restart deployment/familyhub-api
   ```

---

## Additional Resources

**Zitadel Documentation:**
- Official Docs: https://zitadel.com/docs
- OAuth 2.0 Guide: https://zitadel.com/docs/guides/integrate/login/oidc
- PKCE Flow: https://zitadel.com/docs/guides/integrate/login/oidc/authorization-code-pkce

**Family Hub Documentation:**
- OAuth Security Audit: `/tests/FamilyHub.Tests.Integration/Auth/OAUTH_SECURITY_AUDIT.md`
- Completion Summary: `/docs/ZITADEL-OAUTH-COMPLETION-SUMMARY.md`
- Architecture Decision: `/docs/architecture/ADR-002-OAUTH-WITH-ZITADEL.md` (coming soon)

**Standards:**
- OAuth 2.0 RFC: https://datatracker.ietf.org/doc/html/rfc6749
- OpenID Connect: https://openid.net/specs/openid-connect-core-1_0.html
- PKCE RFC: https://datatracker.ietf.org/doc/html/rfc7636

---

**Document Version:** 1.0
**Last Updated:** 2024-12-22
**Maintained By:** Family Hub Team
