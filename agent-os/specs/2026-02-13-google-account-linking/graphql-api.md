# GraphQL API — Google Account Linking

## Schema

```graphql
# Namespace types (added to Root)
extend type RootQuery {
  googleIntegration: GoogleIntegrationQuery! @authorize
}

extend type RootMutation {
  googleIntegration: GoogleIntegrationMutation! @authorize
}

# Query namespace
type GoogleIntegrationQuery {
  """Returns all linked Google accounts for the current user."""
  linkedAccounts: [LinkedAccountDto!]!

  """Returns Google Calendar sync status for the current user."""
  calendarSyncStatus: GoogleCalendarSyncStatusDto!

  """Returns the Google OAuth consent URL for initiating account linking."""
  authUrl: String!
}

# Mutation namespace
type GoogleIntegrationMutation {
  """Unlinks the current user's Google account. Revokes tokens at Google."""
  unlink: Boolean!

  """Manually refreshes the Google access token."""
  refreshToken: RefreshTokenResult!
}

# DTOs
type LinkedAccountDto {
  googleAccountId: String!
  googleEmail: String!
  status: String!
  grantedScopes: String!
  lastSyncAt: DateTime
  createdAt: DateTime!
}

type GoogleCalendarSyncStatusDto {
  isLinked: Boolean!
  lastSyncAt: DateTime
  hasCalendarScope: Boolean!
  status: String!
  errorMessage: String
}

type RefreshTokenResult {
  success: Boolean!
  newExpiresAt: DateTime
}
```

## Queries

### GetLinkedAccounts

Returns all linked Google accounts for the authenticated user.

```
Query: GetLinkedAccountsQuery(UserId)
Handler: Fetches GoogleAccountLink by UserId, maps to LinkedAccountDto
```

### GetGoogleCalendarSyncStatus

Returns sync status specifically for Google Calendar integration.

```
Query: GetGoogleCalendarSyncStatusQuery(UserId)
Handler: Checks if link exists, if calendar scope is granted, last sync time
```

### GetGoogleAuthUrl

Generates the Google OAuth consent URL with PKCE and state parameters.

```
Query: GetGoogleAuthUrlQuery(UserId)
Handler: Creates OAuthState record, builds Google consent URL with:
  - client_id from config
  - redirect_uri: /api/google/callback
  - scope: openid email profile https://www.googleapis.com/auth/calendar.readonly https://www.googleapis.com/auth/calendar.events
  - state: encrypted state token
  - code_challenge: PKCE challenge
  - access_type: offline (for refresh token)
  - prompt: consent (force consent screen for refresh token)
```

## Mutations

### UnlinkGoogleAccount

```
Input: (none - user-scoped)
Command: UnlinkGoogleAccountCommand(UserId)
Handler:
  1. Load GoogleAccountLink by UserId
  2. Call Google revocation endpoint with access token
  3. Hard-delete the link from DB
  4. Raise GoogleAccountUnlinkedEvent
Result: Boolean (success)
```

### RefreshGoogleToken

```
Input: (none - user-scoped)
Command: RefreshGoogleTokenCommand(UserId)
Handler:
  1. Load GoogleAccountLink by UserId
  2. Decrypt refresh token
  3. Call Google token endpoint for new access token
  4. Encrypt new access token
  5. Update aggregate (new token, new expiry)
  6. Raise GoogleTokenRefreshedEvent
Result: RefreshTokenResult { Success, NewExpiresAt }
```

## REST Endpoint (OAuth Callback)

OAuth redirects cannot work through GraphQL, so a thin REST controller handles the callback.

### GET /api/google/callback

```
Parameters: code (string), state (string)
Flow:
  1. Validate state against OAuthState table (check exists + not expired)
  2. Extract user_id from state record
  3. Exchange authorization code for tokens at Google token endpoint
  4. Encrypt access + refresh tokens
  5. Create GoogleAccountLink aggregate
  6. Delete OAuthState record
  7. Redirect to frontend: /settings?google_linked=true
Error: Redirect to frontend: /settings?google_error={message}
```

## Frontend Operations

```typescript
// google-integration.operations.ts

const GET_LINKED_ACCOUNTS = gql`
  query GetLinkedAccounts {
    googleIntegration {
      linkedAccounts {
        googleAccountId
        googleEmail
        status
        grantedScopes
        lastSyncAt
        createdAt
      }
    }
  }
`;

const GET_CALENDAR_SYNC_STATUS = gql`
  query GetCalendarSyncStatus {
    googleIntegration {
      calendarSyncStatus {
        isLinked
        lastSyncAt
        hasCalendarScope
        status
        errorMessage
      }
    }
  }
`;

const GET_GOOGLE_AUTH_URL = gql`
  query GetGoogleAuthUrl {
    googleIntegration {
      authUrl
    }
  }
`;

const UNLINK_GOOGLE_ACCOUNT = gql`
  mutation UnlinkGoogleAccount {
    googleIntegration {
      unlink
    }
  }
`;

const REFRESH_GOOGLE_TOKEN = gql`
  mutation RefreshGoogleToken {
    googleIntegration {
      refreshToken {
        success
        newExpiresAt
      }
    }
  }
`;
```
