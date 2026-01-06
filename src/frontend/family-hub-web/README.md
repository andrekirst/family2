# Family Hub Web - Angular v18 Frontend

OAuth 2.0 authentication frontend for Family Hub, integrated with Zitadel.

## Tech Stack

- **Angular:** v18 (standalone components)
- **TypeScript:** 5.x
- **Tailwind CSS:** 3.4+
- **GraphQL:** @apollo/client
- **State Management:** Angular Signals (NO RxJS for auth state)

## Architecture

### Standalone Components

- No NgModules - fully standalone architecture
- Lazy loading with loadComponent()

### State Management

- **Angular Signals** for reactive auth state
- Computed signals for derived state

### Atomic Design Pattern

```
atoms/       → Button, Spinner
molecules/   → Form fields, Cards (future)
organisms/   → Navbar, Sidebar (future)
templates/   → Page layouts (future)
pages/       → Login, Dashboard, etc.
```

## OAuth 2.0 Flow

1. User clicks "Sign in with Zitadel"
2. Frontend calls `zitadelAuthUrl` GraphQL query
3. Store `codeVerifier` in sessionStorage
4. Redirect to Zitadel OAuth UI
5. User authenticates
6. Zitadel redirects to `/auth/callback?code=...&state=...`
7. Callback validates state (CSRF protection)
8. Callback calls `completeZitadelLogin` mutation
9. Store JWT in localStorage
10. Redirect to dashboard

## Security

- **PKCE (S256):** Proof Key for Code Exchange
- **State Parameter:** CSRF protection
- **JWT Storage:** localStorage for persistence
- **HTTP Interceptor:** Automatic Bearer token injection
- **Auth Guard:** Protected route enforcement

## Development

### Prerequisites

```bash
Node.js 18+
npm 9+
```

### Install Dependencies

```bash
npm install
```

### Run Development Server

```bash
ng serve
# Navigate to http://localhost:4200
```

### Backend API

Ensure backend is running at <http://localhost:5002/graphql>

### Build for Production

```bash
ng build --configuration production
```

## Project Structure

```
src/app/
├── core/
│   ├── guards/
│   │   └── auth.guard.ts
│   ├── interceptors/
│   │   └── auth.interceptor.ts
│   ├── models/
│   │   └── auth.models.ts
│   └── services/
│       ├── auth.service.ts
│       └── graphql.service.ts
├── features/
│   ├── auth/
│   │   └── components/
│   │       ├── login/
│   │       └── callback/
│   └── dashboard/
│       └── dashboard.component.ts
├── shared/
│   └── components/
│       └── atoms/
│           ├── button/
│           └── spinner/
├── app.component.ts
├── app.config.ts
└── app.routes.ts
```

## Environment Configuration

**environment.ts (development):**

```typescript
export const environment = {
  production: false,
  graphqlEndpoint: 'http://localhost:5002/graphql',
  zitadelAuthority: 'http://localhost:8080',
  redirectUri: 'http://localhost:4200/auth/callback',
};
```

**environment.prod.ts (production):**

```typescript
export const environment = {
  production: true,
  graphqlEndpoint: 'https://api.familyhub.com/graphql',
  zitadelAuthority: 'https://auth.familyhub.com',
  redirectUri: 'https://app.familyhub.com/auth/callback',
};
```

## Troubleshooting

### "Invalid state parameter" Error

- CSRF protection triggered
- Clear sessionStorage and try again

### Token Not Persisting

- Check localStorage for `family_hub_access_token`
- Verify token expiration date

### GraphQL Errors

- Ensure backend is running at <http://localhost:5002/graphql>
- Check browser console for detailed error messages

### Redirect Loop

- Clear all browser storage (localStorage + sessionStorage)
- Restart development server

## Testing

### Build Verification

```bash
ng build --configuration development
```

### Manual Testing Flow

1. Navigate to <http://localhost:4200>
2. Should redirect to `/login`
3. Click "Sign in with Zitadel"
4. Complete authentication in Zitadel UI
5. Should redirect to `/dashboard`
6. Verify user info displayed
7. Click "Sign Out"
8. Should redirect to `/login`

## Implementation Status

✅ **Phase 1: Project Setup** - Complete

- Angular v18 project created
- Tailwind CSS configured with design tokens
- Environment configuration files

✅ **Phase 2: Core Services** - Complete

- Auth State Models
- GraphQL Service
- AuthService with Signals
- Auth Guard
- HTTP Interceptor

✅ **Phase 3: Atomic Components** - Complete

- Button component
- Spinner component

✅ **Phase 4: Auth Components** - Complete

- Login component
- OAuth Callback component

✅ **Phase 5: Dashboard & Routing** - Complete

- Dashboard component
- App routing configured
- App configuration with HTTP client

⚠️ **Phase 6: Testing & Documentation** - Partial

- ✅ Build verification (0 errors)
- ✅ README documentation
- ⏳ Unit tests (pending)
- ⏳ E2E tests (pending)

## Next Steps

- Unit tests for AuthService
- E2E tests with Cypress/Playwright
- Calendar module integration
- Task management features
- Family groups and invitations
- Event chain automation UI
- Mobile responsive design improvements

## Related Documentation

- [Backend OAuth Integration](../../docs/architecture/ADR-002-OAUTH-WITH-ZITADEL.md)
- [Design System](../../docs/design-system.md)
- [Architecture Overview](../../docs/domain-model-microservices-map.md)

---

**Status:** ✅ MVP Complete - OAuth Authentication Functional
**Version:** 1.0.0
**Last Updated:** 2025-12-22
**Built with:** [Angular CLI](https://github.com/angular/angular-cli) version 18.2.7
