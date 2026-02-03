# OAuth Login with Keycloak → Dashboard — Shaping Notes

## Scope

**What we're building:**

- User logs in via OAuth 2.0 with Keycloak
- After successful authentication, user is synced with backend database
- Dashboard displays user profile and family membership from backend

**User story:**
> "As a user I want to login. When successfully logged in so I want to see the dashboard"

## Decisions

### OAuth Provider: Keycloak (Not Zitadel)

- **Decision:** Use Keycloak despite ADR-002 documenting Zitadel
- **Rationale:** Keycloak realm configuration already exists (`keycloak-realms/familyhub-realm.json`)
- **Action:** Update ADR-002 with amendment after implementation

### Implementation Approach: Enhance (Not Rewrite)

- **User Request:** "Start completely fresh"
- **Discovery:** Existing OAuth implementation is production-quality
- **Decision:** Keep existing architecture, add missing backend sync layer
- **Rationale:** Pragmatic - only one GraphQL call is missing

### Service Separation: AuthService vs UserService

- **AuthService:** OAuth tokens, PKCE flow, session management
- **UserService:** Backend user state, family membership, profile data
- **Rationale:** Single Responsibility Principle

## Context

### Visuals

**Provided:** None
**Approach:** Use best practices for OAuth login flow UI

### References

**Existing Code Studied:**

- `src/FamilyHub.Api/Features/Auth/GraphQL/AuthMutations.cs` - RegisterUser mutation pattern
- `src/frontend/family-hub-web/src/app/core/auth/auth.service.ts` - OAuth PKCE flow
- `src/frontend/family-hub-web/src/app/features/auth/callback/callback.component.ts` - OAuth callback handling
- `keycloak-realms/familyhub-realm.json` - Keycloak OAuth configuration

### Product Alignment

**No product folder exists** - N/A

## Standards Applied

### Backend

- **graphql-input-command** - Input DTOs separate from Commands
- **vogen-value-objects** - Type-safe value objects (UserId, Email, External UserId)
- **domain-events** - UserRegisteredEvent pattern

### Frontend

- **angular-components** - Standalone components with Angular Signals
- **apollo-graphql** - Apollo Client with typed GraphQL operations

### Database

- **ef-core-migrations** - Schema separation (auth schema)
- **rls-policies** - Row-level security for multi-tenancy

### Testing

- **playwright-e2e** - Zero retry policy, multi-browser support

## The Missing Link

```
Current Flow:
Login → Keycloak → Callback → Exchange code for tokens → Navigate to Dashboard
                                                               ↓
                                                     (Dashboard has NO backend data)

Target Flow:
Login → Keycloak → Callback → Exchange tokens → RegisterUser mutation → Navigate to Dashboard
                                                         ↓                          ↓
                                              (User synced)              GetCurrentUser query
                                                                                   ↓
                                                                    (Shows backend user + family)
```

**Fix:** Add `await userService.registerUser()` in CallbackComponent after token exchange.
