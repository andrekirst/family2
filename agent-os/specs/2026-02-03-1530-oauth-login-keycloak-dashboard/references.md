# References for OAuth Login with Keycloak → Dashboard

## Similar Implementations

### Backend: RegisterUser Mutation Pattern

- **Location:** `src/FamilyHub.Api/Features/Auth/GraphQL/AuthMutations.cs`
- **Relevance:** Shows how to extract OAuth claims from JWT and create/update user
- **Key patterns:**
  - `[Authorize]` attribute requires JWT Bearer token
  - Extracts claims from `ClaimsPrincipal` (sub, email, name, email_verified)
  - Converts primitives to Vogen value objects (Email, UserName, ExternalUserId)
  - Sends command via Wolverine command bus
  - Returns UserDto after querying by UserId

**Example:**

```csharp
[Authorize]
public async Task<UserDto> RegisterUser(
    ClaimsPrincipal claimsPrincipal,
    [Service] ICommandBus commandBus)
{
    var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value;
    var emailString = claimsPrincipal.FindFirst("email")?.Value;
    // ... extract other claims ...

    var command = new RegisterUserCommand(email, name, externalUserId, emailVerified);
    var result = await commandBus.SendAsync<RegisterUserResult>(command, ct);

    return UserMapper.ToDto(registeredUser);
}
```

---

### Frontend: OAuth PKCE Flow

- **Location:** `src/frontend/family-hub-web/src/app/core/auth/auth.service.ts`
- **Relevance:** Complete OAuth 2.0 Authorization Code Flow with PKCE implementation
- **Key patterns:**
  - Generate code verifier (256-char random string)
  - Generate code challenge (SHA256 hash, base64url-encoded)
  - Generate state parameter (CSRF protection)
  - Store verifier and state in sessionStorage
  - Redirect to Keycloak `/protocol/openid-connect/auth`
  - Exchange code + verifier for tokens via POST
  - Store tokens in localStorage
  - Decode JWT to extract user profile

**Token Exchange Example:**

```typescript
const response = await fetch(tokenEndpoint, {
  method: 'POST',
  headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
  body: new URLSearchParams({
    grant_type: 'authorization_code',
    code: code,
    redirect_uri: this.redirectUri,
    client_id: this.clientId,
    code_verifier: codeVerifier
  })
});

const tokens: AuthTokens = await response.json();
localStorage.setItem('access_token', tokens.access_token);
```

---

### Frontend: OAuth Callback Component

- **Location:** `src/frontend/family-hub-web/src/app/features/auth/callback/callback.component.ts`
- **Relevance:** Shows how to handle OAuth callback and display loading state
- **Key patterns:**
  - Extract `code` and `state` from query parameters
  - Call `authService.handleCallback(code, state)`
  - Show error UI if callback fails
  - Provide retry button

**Component Structure:**

```typescript
ngOnInit() {
  this.route.queryParams.subscribe(async (params) => {
    const code = params['code'];
    const state = params['state'];

    if (!code || !state) {
      this.error = 'Missing authorization code or state parameter';
      return;
    }

    try {
      await this.authService.handleCallback(code, state);
    } catch (err: any) {
      this.error = err.message;
    }
  });
}
```

---

### Configuration: Keycloak Realm

- **Location:** `keycloak-realms/familyhub-realm.json`
- **Relevance:** Complete Keycloak realm configuration with OAuth clients
- **Key patterns:**
  - `familyhub-api` client (confidential, service account)
  - `familyhub-web` client (public, PKCE enabled, S256 code challenge)
  - Realm roles: family-owner, family-admin, family-member, family-child
  - Redirect URIs configured for localhost and production
  - Access token lifespan: 900 seconds (15 minutes)
  - Brute force protection enabled

---

## Code Patterns to Follow

### 1. Angular Signals for Reactive State

**Pattern:**

```typescript
import { signal } from '@angular/core';

export class UserService {
  currentUser = signal<CurrentUser | null>(null);
  isLoading = signal(false);

  async fetchCurrentUser() {
    this.isLoading.set(true);
    try {
      const result = await this.apollo.query({ query: GET_CURRENT_USER }).toPromise();
      this.currentUser.set(result.data.currentUser);
    } finally {
      this.isLoading.set(false);
    }
  }
}
```

### 2. Apollo Auth Link for Bearer Tokens

**Pattern:**

```typescript
import { setContext } from '@apollo/client/link/context';

const authLink = setContext((_, { headers }) => {
  const token = localStorage.getItem('access_token');
  return {
    headers: {
      ...headers,
      authorization: token ? `Bearer ${token}` : '',
    }
  };
});

// Combine with http link
ApolloLink.from([authLink, httpLink.create({ uri: environment.apiUrl })])
```

### 3. GraphQL Query Without Input Variables

**Pattern:**

```typescript
export const GET_CURRENT_USER_QUERY = gql`
  query GetCurrentUser {
    currentUser {
      id email name emailVerified
      family { id name }
    }
  }
`;

// No variables needed - backend extracts from JWT!
this.apollo.query({ query: GET_CURRENT_USER_QUERY })
```

---

## Architecture Patterns

### Domain-Driven Design (DDD)

- **Aggregate Roots:** User (with UserId as identity)
- **Value Objects:** Email, UserName, ExternalUserId (Vogen)
- **Domain Events:** UserRegisteredEvent, UserFamilyAssignedEvent
- **Repositories:** IUserRepository with GetByExternalIdAsync

### CQRS with Wolverine

- **Commands:** RegisterUserCommand, UpdateLastLoginCommand
- **Queries:** GetCurrentUserQuery, GetUserByIdQuery
- **Handlers:** Static `Handle` methods discovered by convention
- **Validation:** FluentValidation validators for all commands

### Multi-Tenancy with RLS

- **Middleware:** PostgresRlsMiddleware sets session variables
- **Variables:** `app.current_user_id`, `app.current_family_id`
- **Policies:** Row-level security policies enforce isolation
- **Pattern:** Extract user from JWT → query database → set session vars

---

## Documentation References

### Architecture Decision Records

- **ADR-002:** OAuth with Zitadel (to be amended for Keycloak)
- **ADR-003:** GraphQL Input→Command Pattern

### Development Guides

- **Backend Development:** `docs/guides/BACKEND_DEVELOPMENT.md`
- **Frontend Development:** `docs/guides/FRONTEND_DEVELOPMENT.md`

### Standards

- **Backend:** `agent-os/standards/backend/`
- **Frontend:** `agent-os/standards/frontend/`
- **Database:** `agent-os/standards/database/`
- **Testing:** `agent-os/standards/testing/`
