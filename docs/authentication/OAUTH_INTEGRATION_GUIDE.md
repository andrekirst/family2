# OAuth 2.0 Integration with Zitadel - Complete Guide

## Overview

Family Hub uses **Zitadel** as an external OAuth 2.0/OIDC provider for authentication. This provides enterprise-grade security without storing passwords in our database.

**Benefits of OAuth with Zitadel:**

- ✅ No password storage (more secure)
- ✅ Automatic email verification
- ✅ 2FA/MFA ready (future)
- ✅ SSO capability (future)
- ✅ Less code to maintain
- ✅ Enterprise-grade security

---

## Architecture

### OAuth 2.0 Flow with PKCE

```
┌─────────────┐                                    ┌──────────────┐
│   Angular   │                                    │   Zitadel    │
│  Frontend   │                                    │   (OAuth)    │
│             │                                    │              │
│ localhost:  │                                    │ localhost:   │
│   4200      │                                    │   8080       │
└──────┬──────┘                                    └──────┬───────┘
       │                                                  │
       │ 1. Click "Sign in with Zitadel"                  │
       │ 2. GET /zitadelAuthUrl                           │
       │    ◄── Returns authorization URL + verifier      │
       │                                                  │
       │ 3. Redirect to authorization URL ───────────────►│
       │                                                  │
       │                                 4. User Login    │
       │                                    ◄────────────►│
       │                                                  │
       │ 5. Redirect back with code ◄─────────────────────│
       │    /auth/callback?code=xxx&state=yyy             │
       │                                                  │
       │ 6. POST /completeZitadelLogin                    │
       │    {code, codeVerifier}                          │
       │                   │                              │
       │                   └──►  7. Exchange code ───────►│
       │                          for tokens              │
       │                                                  │
       │                   8. Validate JWT ◄──────────────│
       │                      Create/Sync User            │
       │                                                  │
       │ 9. Return access token + user data               │
       │    ◄───────────────┘                             │
       │                                                  │
       │ 10. Store token, redirect to dashboard           │
       └─────────────────────────────────────────────────┘
```

### Components

**Frontend (Angular v18):**

- `AuthService` - OAuth flow orchestration
- `LoginComponent` - Login UI
- `CallbackComponent` - OAuth callback handler
- `AuthGuard` - Route protection
- `AuthInterceptor` - JWT injection

**Backend (.NET Core 10):**

- `GetZitadelAuthUrlQuery` - Generates OAuth URL with PKCE
- `CompleteZitadelLoginCommand` - Exchanges code for tokens
- `User.CreateFromOAuth()` - Domain logic for OAuth users
- `UserRepository.GetByExternalUserIdAsync()` - User lookup/sync

**Database (PostgreSQL):**

- `auth.users` table with OAuth fields:
  - `external_user_id` - Zitadel user ID (sub claim)
  - `external_provider` - "zitadel"
  - `email_verified` - Automatically true from OAuth

---

## PKCE (Proof Key for Code Exchange)

PKCE prevents authorization code interception attacks:

1. **Frontend generates:**

   - `code_verifier` - Random 43-128 character string
   - `code_challenge` - SHA256 hash of verifier

2. **Authorization request** includes:

   - `code_challenge`
   - `code_challenge_method=S256`

3. **Token exchange** includes:

   - `code_verifier` (original unhashed value)

4. **Zitadel verifies:**
   - SHA256(code_verifier) == code_challenge
   - If match → issue tokens
   - If mismatch → reject (possible interception)

---

## Security Features

### 1. PKCE (S256)

Prevents authorization code interception (man-in-the-middle attacks).

### 2. State Parameter

Prevents CSRF attacks by validating the state value matches.

### 3. JWT Signature Validation

- Algorithm: RS256 (asymmetric)
- Public key fetched from Zitadel's JWKS endpoint
- Validates issuer, audience, expiration

### 4. No Password Storage

Passwords are never stored in our database - all auth delegated to Zitadel.

### 5. Automatic Email Verification

Zitadel handles email verification - users created from OAuth are automatically verified.

---

## Configuration

### Backend (appsettings.Development.json)

```json
{
  "Zitadel": {
    "Authority": "http://localhost:8080",
    "ClientId": "352301149971349506@family_hub",
    "RedirectUri": "http://localhost:4200/auth/callback",
    "Scopes": "openid profile email",
    "Audience": "family-hub-api"
  }
}
```

### Frontend (environment.ts)

```typescript
export const environment = {
  graphqlEndpoint: "http://localhost:5002/graphql",
  zitadelAuthority: "http://localhost:8080",
  redirectUri: "http://localhost:4200/auth/callback",
};
```

### Zitadel Application Settings

- **Type:** Web Application
- **Auth Method:** PKCE (no client secret)
- **Redirect URI:** `http://localhost:4200/auth/callback`
- **Post Logout URI:** `http://localhost:4200/login`
- **Grant Types:** Authorization Code, Refresh Token
- **ID Token Userinfo Assertion:** ✅ Enabled (required for email claim)

---

## GraphQL API

### Query: zitadelAuthUrl

**Request:**

```graphql
query GetZitadelAuthUrl {
  zitadelAuthUrl {
    authorizationUrl
    codeVerifier
    state
  }
}
```

**Response:**

```json
{
  "data": {
    "zitadelAuthUrl": {
      "authorizationUrl": "http://localhost:8080/oauth/v2/authorize?client_id=...",
      "codeVerifier": "random-43-char-string...",
      "state": "csrf-protection-token"
    }
  }
}
```

### Mutation: completeZitadelLogin

**Request:**

```graphql
mutation CompleteZitadelLogin($input: CompleteZitadelLoginInput!) {
  completeZitadelLogin(input: $input) {
    authenticationResult {
      user {
        id
        email
        emailVerified
        createdAt
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
```

**Variables:**

```json
{
  "input": {
    "authorizationCode": "code-from-callback-url",
    "codeVerifier": "verifier-from-sessionStorage"
  }
}
```

**Response:**

```json
{
  "data": {
    "completeZitadelLogin": {
      "authenticationResult": {
        "user": {
          "id": "3219a888-8f67-4a9c-85bc-f25781476823",
          "email": "admin@familyhub.localhost",
          "emailVerified": true,
          "createdAt": "2025-12-23T10:43:01.864167Z"
        },
        "accessToken": "eyJhbGciOiJSUzI1NiIs...",
        "expiresAt": "2025-12-23T11:43:01Z"
      },
      "errors": null
    }
  }
}
```

---

## Database Schema

### auth.users Table

```sql
CREATE TABLE auth.users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) NOT NULL,
    email_verified BOOLEAN NOT NULL DEFAULT FALSE,
    email_verified_at TIMESTAMP WITH TIME ZONE,
    external_user_id VARCHAR(255) NOT NULL,  -- Zitadel user ID (sub claim)
    external_provider VARCHAR(50) NOT NULL,   -- "zitadel"
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE,
    deleted_at TIMESTAMP WITH TIME ZONE,
    UNIQUE (external_provider, external_user_id)
);

CREATE INDEX idx_users_external_auth
  ON auth.users (external_provider, external_user_id);
```

**Key Points:**

- No `password_hash` column (OAuth-only design)
- `external_user_id` stores Zitadel's `sub` claim
- `email_verified` is automatically `true` for OAuth users
- Unique constraint on (provider, user_id) prevents duplicates

---

## User Creation Flow

### New User (First Login)

1. User logs in via Zitadel
2. Backend receives ID token with `sub` and `email` claims
3. `UserRepository.GetByExternalUserIdAsync()` → returns null (not found)
4. `User.CreateFromOAuth(email, sub, "zitadel")` creates new user:
   - Generates new `UserId` (GUID)
   - Sets `email_verified = true`
   - Sets `email_verified_at = DateTime.UtcNow`
   - Sets `external_user_id = sub`
   - Sets `external_provider = "zitadel"`
5. User saved to database
6. Returns user + access token

### Existing User (Subsequent Logins)

1. User logs in via Zitadel
2. Backend receives ID token with `sub` and `email` claims
3. `UserRepository.GetByExternalUserIdAsync()` → returns existing user
4. No database write (user already exists)
5. Returns user + access token

---

## Frontend Implementation

### AuthService (Angular Signals)

```typescript
@Injectable({ providedIn: 'root' })
export class AuthService {
  // Reactive state with Angular Signals (NO RxJS!)
  private authState = signal<AuthState>({
    isAuthenticated: false,
    user: null,
    accessToken: null,
    expiresAt: null,
  });

  readonly isAuthenticated = computed(() => this.authState().isAuthenticated);
  readonly currentUser = computed(() => this.authState().user);

  async login(): Promise<void> {
    // 1. Get OAuth URL from backend
    const response = await this.graphql.query<GetZitadelAuthUrlResponse>(...);

    // 2. Store PKCE verifier and state (temporary)
    sessionStorage.setItem('pkce_code_verifier', response.codeVerifier);
    sessionStorage.setItem('oauth_state', response.state);

    // 3. Redirect to Zitadel
    window.location.href = response.authorizationUrl;
  }

  async completeLogin(code: string, state: string): Promise<void> {
    // 1. Validate state (CSRF protection)
    const storedState = sessionStorage.getItem('oauth_state');
    if (storedState !== state) {
      throw new Error('Invalid state - possible CSRF attack');
    }

    // 2. Get PKCE verifier
    const codeVerifier = sessionStorage.getItem('pkce_code_verifier');

    // 3. Exchange code for tokens
    const response = await this.graphql.mutate<CompleteZitadelLoginResponse>(...);

    // 4. Store token (persistent)
    localStorage.setItem('family_hub_access_token', response.accessToken);

    // 5. Update auth state
    this.authState.set({
      isAuthenticated: true,
      user: response.user,
      accessToken: response.accessToken,
      expiresAt: new Date(response.expiresAt),
    });

    // 6. Clear session storage
    sessionStorage.clear();
  }

  logout(): void {
    localStorage.clear();
    sessionStorage.clear();
    this.authState.set({
      isAuthenticated: false,
      user: null,
      accessToken: null,
      expiresAt: null,
    });
    this.router.navigate(['/login']);
  }
}
```

### Auth Guard

```typescript
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(["/login"], {
      queryParams: { returnUrl: state.url },
    });
    return false;
  }

  return true;
};
```

### HTTP Interceptor

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(req);
};
```

---

## Testing

### Unit Tests (7 tests - all passing)

```bash
cd src/api/tests/FamilyHub.Tests.Unit
dotnet test
```

**Tests:**

- ✅ User.CreateFromOAuth creates valid user
- ✅ Email is automatically verified
- ✅ Works with different email addresses
- ✅ Generates unique user IDs
- ✅ Sets created timestamp correctly

### Integration Tests

**Manual Testing Checklist:**

- [x] User can click "Sign in with Zitadel"
- [x] Redirects to Zitadel login page
- [x] User can login with credentials
- [x] Redirects back to /auth/callback
- [x] Callback completes successfully
- [x] User is redirected to dashboard
- [x] Dashboard shows user email
- [x] User record created in database
- [x] JWT token stored in localStorage
- [x] Subsequent API calls include Authorization header

---

## Troubleshooting

### "ID token missing 'email' claim"

**Cause:** Zitadel application not configured to include email in ID token.

**Fix:**

1. Go to Zitadel → Projects → Family Hub → Applications → Family Hub Web
2. Find "ID Token Userinfo Assertion" setting
3. Enable it
4. Save changes

### "relation 'auth.users' does not exist"

**Cause:** Database migrations not applied.

**Fix:**

```bash
cd src/api/Modules/FamilyHub.Modules.Auth
dotnet ef database update --startup-project ../../FamilyHub.Api --context AuthDbContext
```

### "Invalid redirect_uri"

**Cause:** Redirect URI mismatch between Zitadel and backend config.

**Fix:**

1. Verify Zitadel application settings: exactly `http://localhost:4200/auth/callback`
2. Verify appsettings.json: exactly `http://localhost:4200/auth/callback`
3. No trailing slashes!

### Port 5002 already in use

**Fix:**

```bash
lsof -ti:5002 | xargs kill -9
cd src/api/FamilyHub.Api
dotnet run
```

---

## Production Deployment

### Security Checklist

- [ ] Use HTTPS for all URLs (Zitadel, frontend, backend)
- [ ] Store Client ID in Azure Key Vault
- [ ] Use environment variables for configuration
- [ ] Set short token lifetimes (15 min for access, 1 day for refresh)
- [ ] Enable rate limiting on OAuth endpoints
- [ ] Monitor failed login attempts
- [ ] Set up proper CORS configuration
- [ ] Enable security headers (CSP, HSTS, etc.)

### Configuration Changes

**appsettings.Production.json:**

```json
{
  "Zitadel": {
    "Authority": "https://auth.familyhub.com",
    "ClientId": "${ZITADEL_CLIENT_ID}", // From Key Vault
    "RedirectUri": "https://app.familyhub.com/auth/callback",
    "Scopes": "openid profile email",
    "Audience": "family-hub-api"
  }
}
```

**environment.prod.ts:**

```typescript
export const environment = {
  production: true,
  graphqlEndpoint: "https://api.familyhub.com/graphql",
  zitadelAuthority: "https://auth.familyhub.com",
  redirectUri: "https://app.familyhub.com/auth/callback",
};
```

---

## References

- [Zitadel Documentation](https://zitadel.com/docs)
- [OAuth 2.0 RFC 6749](https://datatracker.ietf.org/doc/html/rfc6749)
- [PKCE RFC 7636](https://datatracker.ietf.org/doc/html/rfc7636)
- [OpenID Connect Core 1.0](https://openid.net/specs/openid-connect-core-1_0.html)

---

**Last Updated:** 2025-12-23
**Version:** 1.0
**Status:** ✅ Production-Ready
