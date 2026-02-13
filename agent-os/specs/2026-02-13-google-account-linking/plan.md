# Google Account Linking — Plan

## Summary

Enable family members to link their Google account for two purposes:

1. **Sign in with Google** via Keycloak Identity Brokering (configuration-only)
2. **Google API access** (starting with Calendar) via a dedicated in-app OAuth2 flow with encrypted token storage

## Architecture

- **New module:** `GoogleIntegrationModule` at `Features/GoogleIntegration/`
- **Keycloak broker** for Google sign-in (no app code changes)
- **Separate OAuth flow** for Google API access with calendar-specific scopes
- **Encrypted tokens** (AES-256-GCM) in PostgreSQL `google_integration` schema
- **User-level** linking (each family member links their own account)

## Domain Model

### GoogleAccountLink Aggregate

```
GoogleAccountLink : AggregateRoot<GoogleAccountLinkId>
  UserId                    FK to auth.users
  GoogleAccountId           Google "sub" claim (unique)
  GoogleEmail               Email VO
  EncryptedAccessToken      AES-256-GCM ciphertext
  EncryptedRefreshToken     AES-256-GCM ciphertext
  AccessTokenExpiresAt      DateTime
  GrantedScopes             space-separated scope list
  Status                    Active | Revoked | Expired | Error
  LastSyncAt, LastError
  CreatedAt, UpdatedAt
```

### Value Objects (Vogen)

`GoogleAccountLinkId`, `GoogleAccountId`, `EncryptedToken`, `GoogleScopes`, `GoogleLinkStatus`

### Domain Events

- `GoogleAccountLinkedEvent`
- `GoogleAccountUnlinkedEvent`
- `GoogleTokenRefreshedEvent`
- `GoogleTokenRefreshFailedEvent`

## Google OAuth Flow

```
Frontend: Click "Link Google" -> GraphQL query getAuthUrl -> redirect to Google
Google: User consents -> redirects to GET /api/google/callback?code=...&state=...
Backend: Validate state -> exchange code -> encrypt tokens -> persist -> redirect to frontend
Frontend: /settings?google_linked=true -> show success
```

REST endpoint required for OAuth callback (OAuth redirects can't work through GraphQL).

## GraphQL API

```graphql
type RootQuery {
  googleIntegration: GoogleIntegrationQuery!
}
type GoogleIntegrationQuery {
  linkedAccounts: [LinkedAccountDto!]!
  calendarSyncStatus: GoogleCalendarSyncStatusDto!
  authUrl: String!
}

type RootMutation {
  googleIntegration: GoogleIntegrationMutation!
}
type GoogleIntegrationMutation {
  unlink: Boolean!
  refreshToken: RefreshTokenResult!
}
```

## Frontend

New `/settings` feature with:

- `user-settings` page component
- `google-link` component (link/unlink UI with status)
- `integrations-panel` component

## Database

Schema: `google_integration`

- **`google_account_links`** table with encrypted token columns, RLS on user_id
- **`oauth_states`** ephemeral table for CSRF protection (10-min TTL)

## Token Refresh Strategy

- **On-demand (primary):** Refresh if expired or within 5 minutes of expiry before API calls
- **Background (secondary):** HostedService every 30 min, proactively refreshes tokens expiring within 15 min

## Security

- AES-256-GCM encryption at rest, key from environment variables
- Cryptographic `state` parameter with 10-min expiry encoding user ID
- Token revocation at Google on unlink + hard delete from DB
- Minimal scopes: `calendar.readonly` + `calendar.events`
- Tokens never in logs

## Tasks

1. Save spec documentation
2. Backend domain model (aggregate, VOs, events, repository interface)
3. Token encryption service (AES-256-GCM)
4. Backend persistence (EF Core config, repository, migration)
5. Google OAuth service (consent URL, token exchange, refresh, revoke)
6. Application layer (commands, queries, handlers, validators)
7. GraphQL namespace types + REST callback controller
8. Module registration (GoogleIntegrationModule, Program.cs, appsettings)
9. Background token refresh service (HostedService)
10. Keycloak Google Identity Brokering (realm template config)
11. Backend unit tests (aggregate, handlers, encryption)
12. Frontend service layer (routes, providers, GraphQL ops, models, service)
13. Frontend components (settings page, google-link, integrations-panel)
14. E2E tests (Playwright)

## Success Criteria

- User can sign in with Google via Keycloak login page
- User can link Google account from Settings page
- Google OAuth tokens stored encrypted in PostgreSQL
- User can unlink Google account (tokens revoked at Google + deleted)
- GraphQL queries return linked account status
- All existing 77 tests still pass + new GoogleIntegration tests pass
- Token refresh works on-demand and via background service

## Shared Files Modified

| File | Change |
|------|--------|
| `Program.cs` | +RegisterModule, +AddControllers, +MapControllers |
| `AppDbContext.cs` | +2 DbSets |
| `RootQuery.cs` | +GoogleIntegration() method |
| `RootMutation.cs` | +GoogleIntegration() method |
| `FamilyHub.slnx` | +test project |
| `app.routes.ts` | +settings route |
| `app.config.ts` | +provideSettingsFeature() |
| `sidebar.component.ts` | +Settings nav item |
