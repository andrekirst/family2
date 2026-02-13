# Google Account Linking — Shaping Notes

## Scope

As a family member, I want to link my Google account to:

1. Sign in with Google (via Keycloak Identity Brokering)
2. Use Google APIs like Calendar on my behalf

### In Scope

- Keycloak Identity Brokering configuration for Google sign-in
- In-app OAuth2 flow for Google API access with calendar scopes
- Encrypted token storage (AES-256-GCM) in PostgreSQL
- User-level linking (each family member links their own Google account)
- GoogleAccountLink aggregate with full lifecycle (create, refresh, revoke)
- GraphQL API for status queries + unlink mutation
- REST endpoint for OAuth callback (required by Google OAuth redirect flow)
- Settings page with link/unlink UI
- Background token refresh service
- Unit tests for aggregate, handlers, encryption

### Out of Scope

- Actual Google Calendar sync (separate feature, uses this as foundation)
- Other Google APIs beyond Calendar scope request
- Other OAuth providers (Apple, Microsoft) — designed for extensibility but not implemented
- Family-level Google account linking
- Recurring event sync or bidirectional sync

## Decisions

1. **New module** (`GoogleIntegrationModule`) rather than extending Auth or Calendar
   - Auth handles Keycloak identity, not third-party API tokens
   - Calendar shouldn't own provider-specific logic
   - Clean separation for future providers

2. **Keycloak Identity Broker** for "Sign in with Google"
   - Zero app code changes, config-only in Keycloak realm template
   - Users see "Sign in with Google" on the Keycloak login page
   - Backend receives the same JWT claims regardless of login method

3. **Separate in-app OAuth flow** for Google API access
   - Keycloak brokered tokens have limited scope control
   - Need calendar-specific scopes that Keycloak doesn't request
   - Full control over token lifecycle (refresh, revoke, encrypt)

4. **Encrypted token storage in PostgreSQL** (AES-256-GCM)
   - Self-contained, no external secrets manager dependency
   - Key from environment variables in production
   - Unique nonce per encryption operation

5. **REST callback endpoint** (deviation from pure-GraphQL)
   - OAuth redirects require HTTP redirect endpoints, not GraphQL
   - Thin controller delegates to command bus immediately
   - Only endpoint: `GET /api/google/callback`

6. **On-demand + background token refresh**
   - On-demand avoids unnecessary refreshes for inactive users
   - Background prevents slow API calls from refresh latency
   - Combined approach provides best UX

## Context

- **Visuals:** None
- **References:** Auth module (OIDC patterns, User aggregate, ExternalUserId)
- **Product alignment:** Calendar spec listed "External calendar sync (Google, Apple)" as future work — this is the prerequisite

## Standards Applied

- **user-context** — Access current user via IUserService + ClaimNames for authorization
- **permission-system** — No new permissions needed (user can only manage their own linked account)
- **secure-token-pattern** — Inspiration for crypto approach, but Google tokens use AES encryption not SHA256 hashing
- **graphql-input-command** — Input->Command pattern for UnlinkGoogleAccount, RefreshGoogleToken mutations
- **vogen-value-objects** — Type-safe VOs for GoogleAccountLinkId, GoogleAccountId, etc.
- **domain-events** — GoogleAccountLinkedEvent, GoogleAccountUnlinkedEvent for audit trail
- **ef-core-migrations** — EF Core config in Data/ folder, migration for google_integration schema
- **angular-components** — Standalone components with signals for Settings feature
- **apollo-graphql** — Typed GraphQL operations for Google integration queries/mutations
