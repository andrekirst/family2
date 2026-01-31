# Frontend Development Guide

**Purpose:** Guide for developing Family Hub's Angular frontend with TypeScript, Tailwind CSS, and Playwright E2E testing.

**Tech Stack:** Angular v21, TypeScript 5.x, Tailwind CSS 3.x, Apollo Client 3.x, Playwright 1.48+, RxJS 7.8

---

## Quick Reference

### Project Structure

```
src/frontend/family-hub-web/
├── src/
│   ├── app/
│   │   ├── components/      # Reusable components (atoms, molecules)
│   │   ├── pages/           # Page components (organisms)
│   │   ├── services/        # Business logic services
│   │   ├── guards/          # Route guards
│   │   └── interceptors/    # HTTP interceptors
│   ├── assets/              # Images, icons, fonts
│   └── environments/        # Environment configs
├── e2e/                     # Playwright E2E tests
└── playwright.config.ts     # Playwright configuration
```

---

## Critical Patterns (4)

### 1. Component Architecture (Standalone + Atomic Design)

**All components are standalone** (no NgModules). Use **atomic design** hierarchy.

**Standalone Component:**

```typescript
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,  // Required!
  imports: [CommonModule, RouterModule],  // Declare dependencies
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent {
  isCollapsed = signal(false);

  toggleSidebar() {
    this.isCollapsed.update(value => !value);
  }
}
```

**Atomic Design Hierarchy:**

- **Atoms:** Button, Input, Icon (basic building blocks)
- **Molecules:** FormField, SearchBar (atoms combined)
- **Organisms:** Sidebar, Header, Card (complex components)
- **Templates:** PageLayout (page structure without data)
- **Pages:** DashboardPage, FamilyPage (complete pages with data)

**File Organization:**

```
app/
├── components/
│   ├── atoms/
│   │   ├── button/
│   │   │   ├── button.component.ts
│   │   │   ├── button.component.html
│   │   │   └── button.component.css
│   │   └── input/
│   ├── molecules/
│   │   └── form-field/
│   └── organisms/
│       └── sidebar/
└── pages/
    ├── dashboard/
    └── family/
```

---

### 2. Apollo GraphQL (Typed Queries)

**Use Apollo Client** for GraphQL with typed operations.

**Setup (app.config.ts):**

```typescript
import { provideHttpClient } from '@angular/common/http';
import { provideApollo } from 'apollo-angular';
import { HttpLink } from 'apollo-angular/http';
import { InMemoryCache } from '@apollo/client/core';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    provideApollo(() => {
      const httpLink = inject(HttpLink);
      return {
        link: httpLink.create({ uri: environment.apiUrl }),
        cache: new InMemoryCache(),
      };
    }),
  ],
};
```

**GraphQL Query:**

```typescript
import { gql, Apollo } from 'apollo-angular';

const GET_CURRENT_FAMILY = gql`
  query GetCurrentFamily {
    currentFamily {
      id
      name
      members {
        id
        email
        role
      }
    }
  }
`;

@Component({
  // ...
})
export class FamilyComponent {
  private apollo = inject(Apollo);

  family$ = this.apollo.query({
    query: GET_CURRENT_FAMILY
  }).pipe(
    map(result => result.data.currentFamily)
  );
}
```

**GraphQL Mutation:**

```typescript
const CREATE_FAMILY = gql`
  mutation CreateFamily($input: CreateFamilyInput!) {
    createFamily(input: $input) {
      familyId
      name
    }
  }
`;

@Component({
  // ...
})
export class CreateFamilyComponent {
  private apollo = inject(Apollo);

  createFamily(name: string) {
    this.apollo.mutate({
      mutation: CREATE_FAMILY,
      variables: {
        input: { name }
      }
    }).subscribe({
      next: (result) => {
        console.log('Family created:', result.data.createFamily);
      },
      error: (error) => {
        console.error('Error creating family:', error);
      }
    });
  }
}
```

**Error Handling:**

```typescript
family$ = this.apollo.query({
  query: GET_CURRENT_FAMILY
}).pipe(
  map(result => result.data.currentFamily),
  catchError(error => {
    console.error('GraphQL Error:', error);
    return of(null);  // Return fallback value
  })
);
```

---

### 3. Playwright E2E Testing (API-First)

**Zero-retry policy:** All tests must pass reliably. Use **API-first** approach.

**Test Structure:**

```typescript
import { test, expect } from '@playwright/test';

test('should create family via API and verify in UI', async ({ page }) => {
  // 1. Create via GraphQL API (fast, reliable)
  const response = await page.request.post('http://localhost:7000/graphql', {
    data: {
      query: `
        mutation CreateFamily($input: CreateFamilyInput!) {
          createFamily(input: $input) {
            familyId
            name
          }
        }
      `,
      variables: {
        input: { name: 'Test Family' }
      }
    }
  });

  const data = await response.json();
  expect(data.data.createFamily.familyId).toBeTruthy();

  // 2. Spot-check UI (optional, lightweight)
  await page.goto('/family');
  await expect(page.getByText('Test Family')).toBeVisible();
});
```

**With Fixtures (Auth, GraphQL):**

```typescript
import { test, expect } from '../fixtures/auth.fixture';

test('should access protected route', async ({ authenticatedPage }) => {
  await authenticatedPage.goto('/dashboard');
  await expect(authenticatedPage).toHaveURL(/dashboard/);
  await expect(authenticatedPage.getByText('Welcome')).toBeVisible();
});
```

**Vogen Mirrors (Type-Safe Test Data):**

```typescript
import { Email, FamilyName } from '../support/vogen-mirrors';

test('should validate email format', () => {
  // Valid
  const email = Email.from('user@example.com');
  expect(email.value).toBe('user@example.com');

  // Invalid (throws)
  expect(() => Email.from('invalid')).toThrow('Invalid email format');
});

test('should create family with valid name', async ({ graphqlClient }) => {
  const familyName = FamilyName.from('Smith Family');

  const result = await graphqlClient.mutate(CREATE_FAMILY_MUTATION, {
    name: familyName.toString()
  });

  expect(result.data.createFamily.name).toBe('Smith Family');
});
```

**See:** [TESTING_WITH_PLAYWRIGHT.md](../../docs/development/TESTING_WITH_PLAYWRIGHT.md)

---

### 4. OAuth Implementation (PKCE Flow)

**PKCE + State parameter** OAuth 2.0 flow with Zitadel.

**Environment Config:**

```typescript
// environment.ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7000/graphql',
  zitadel: {
    issuer: 'http://localhost:8080',
    clientId: 'YOUR_CLIENT_ID',
    redirectUri: 'http://localhost:4200/callback',
    postLogoutRedirectUri: 'http://localhost:4200',
    scope: 'openid profile email'
  }
};
```

**Auth Service (Signal-Based):**

```typescript
import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private accessToken = signal<string | null>(null);
  isAuthenticated = signal(false);

  login() {
    // Generate PKCE code verifier and challenge
    const codeVerifier = this.generateCodeVerifier();
    const codeChallenge = await this.generateCodeChallenge(codeVerifier);
    const state = this.generateState();

    // Store for callback
    sessionStorage.setItem('code_verifier', codeVerifier);
    sessionStorage.setItem('state', state);

    // Redirect to Zitadel
    const authUrl = `${environment.zitadel.issuer}/oauth/v2/authorize?` +
      `client_id=${environment.zitadel.clientId}&` +
      `redirect_uri=${environment.zitadel.redirectUri}&` +
      `response_type=code&` +
      `scope=${environment.zitadel.scope}&` +
      `code_challenge=${codeChallenge}&` +
      `code_challenge_method=S256&` +
      `state=${state}`;

    window.location.href = authUrl;
  }

  async handleCallback(code: string, state: string) {
    // Validate state
    const storedState = sessionStorage.getItem('state');
    if (state !== storedState) {
      throw new Error('Invalid state parameter');
    }

    // Exchange code for tokens
    const codeVerifier = sessionStorage.getItem('code_verifier')!;
    const response = await fetch(`${environment.zitadel.issuer}/oauth/v2/token`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: new URLSearchParams({
        grant_type: 'authorization_code',
        code,
        redirect_uri: environment.zitadel.redirectUri,
        client_id: environment.zitadel.clientId,
        code_verifier: codeVerifier
      })
    });

    const tokens = await response.json();
    this.accessToken.set(tokens.access_token);
    this.isAuthenticated.set(true);

    // Store tokens
    localStorage.setItem('access_token', tokens.access_token);
    localStorage.setItem('refresh_token', tokens.refresh_token);
  }

  private generateCodeVerifier(): string {
    const array = new Uint8Array(32);
    crypto.getRandomValues(array);
    return this.base64UrlEncode(array);
  }

  private async generateCodeChallenge(verifier: string): Promise<string> {
    const encoder = new TextEncoder();
    const data = encoder.encode(verifier);
    const hash = await crypto.subtle.digest('SHA-256', data);
    return this.base64UrlEncode(new Uint8Array(hash));
  }

  private generateState(): string {
    const array = new Uint8Array(16);
    crypto.getRandomValues(array);
    return this.base64UrlEncode(array);
  }

  private base64UrlEncode(array: Uint8Array): string {
    return btoa(String.fromCharCode(...array))
      .replace(/\+/g, '-')
      .replace(/\//g, '_')
      .replace(/=/g, '');
  }
}
```

**HTTP Interceptor (Add Token):**

```typescript
import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('access_token');

  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req);
};
```

**Route Guard:**

```typescript
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  return router.parseUrl('/login');
};
```

**See:** [docs/authentication/OAUTH_INTEGRATION_GUIDE.md](../../docs/authentication/OAUTH_INTEGRATION_GUIDE.md)

---

## Testing Patterns

### Component Tests (Jasmine + Karma)

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SidebarComponent } from './sidebar.component';

describe('SidebarComponent', () => {
  let component: SidebarComponent;
  let fixture: ComponentFixture<SidebarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SidebarComponent]  // Standalone component
    }).compileComponents();

    fixture = TestBed.createComponent(SidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle sidebar', () => {
    expect(component.isCollapsed()).toBe(false);
    component.toggleSidebar();
    expect(component.isCollapsed()).toBe(true);
  });
});
```

### E2E Tests (Playwright)

**Run Tests:**

```bash
# UI mode (interactive)
npm run e2e

# Headless (CI)
npm run e2e:headless

# Specific browser
npm run e2e:chromium
npm run e2e:firefox
npm run e2e:webkit
```

---

## Accessibility

**WCAG 2.1 AA Compliance** with @axe-core/playwright.

**Automated Checks:**

```typescript
import { test } from '@playwright/test';
import { injectAxe, checkA11y } from '@axe-core/playwright';

test('should have no accessibility violations', async ({ page }) => {
  await page.goto('/dashboard');
  await injectAxe(page);
  await checkA11y(page, null, {
    detailedReport: true
  });
});
```

**Manual Checks:**

- Keyboard navigation (Tab, Enter, Space, Arrow keys)
- Screen reader compatibility (NVDA, JAWS, VoiceOver)
- Color contrast (4.5:1 minimum)
- Focus states visible
- ARIA labels on interactive elements

---

## Common Frontend Tasks

### Create New Component

```bash
ng generate component components/atoms/button --standalone
```

### Add Tailwind Classes

```html
<!-- Use Tailwind utility classes -->
<button class="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600">
  Click Me
</button>
```

### Handle Form Input

```typescript
import { FormsModule } from '@angular/forms';

@Component({
  imports: [FormsModule],
  template: `
    <input [(ngModel)]="name" placeholder="Enter name" />
    <button (click)="submit()">Submit</button>
  `
})
export class FormComponent {
  name = '';

  submit() {
    console.log('Name:', this.name);
  }
}
```

---

## Debugging

**Common Issues:**

- **Module not found:** Run `npm install`
- **CORS error:** Check backend CORS configuration
- **OAuth redirect loop:** Verify redirect URI matches Zitadel config
- **Component not rendering:** Check `standalone: true` and `imports` array

**See:** [DEBUGGING_GUIDE.md](../../docs/development/DEBUGGING_GUIDE.md)

---

## Educational Insights

**Frontend-Specific Examples:**

```
★ Insight ─────────────────────────────────────
1. Standalone components reduce bundle size via tree-shaking
2. Angular Signals provide reactive state without RxJS boilerplate
3. Atomic design promotes component reusability and consistency
─────────────────────────────────────────────────
```

```
★ Insight ─────────────────────────────────────
1. Apollo cache policies determine query refetch behavior
2. GraphQL typed operations catch errors at compile time
3. Optimistic UI updates improve perceived performance
─────────────────────────────────────────────────
```

```
★ Insight ─────────────────────────────────────
1. Playwright's API-first testing catches backend issues before UI tests
2. Zero-retry policy forces fixing flaky tests immediately
3. Vogen mirrors ensure frontend test data matches backend validation
─────────────────────────────────────────────────
```

---

## Related Documentation

- **Angular Component Specs:** [docs/frontend/angular-component-specs.md](../../docs/frontend/angular-component-specs.md)
- **OAuth Integration:** [docs/authentication/OAUTH_INTEGRATION_GUIDE.md](../../docs/authentication/OAUTH_INTEGRATION_GUIDE.md)
- **Testing Guide:** [docs/development/TESTING_WITH_PLAYWRIGHT.md](../../docs/development/TESTING_WITH_PLAYWRIGHT.md)
- **ADR-004:** [Playwright Migration](../../docs/architecture/ADR-004-PLAYWRIGHT-MIGRATION.md)

---

**Last Updated:** 2026-01-09
**Derived from:** Root CLAUDE.md v5.0.0
**Canonical Sources:**

- docs/frontend/angular-component-specs.md (Component architecture)
- docs/authentication/OAUTH_INTEGRATION_GUIDE.md (OAuth PKCE flow)
- docs/development/TESTING_WITH_PLAYWRIGHT.md (E2E testing patterns)
- docs/architecture/ADR-004-PLAYWRIGHT-MIGRATION.md

**Sync Checklist:**

- [ ] Component patterns match angular-component-specs.md
- [ ] OAuth flow matches OAUTH_INTEGRATION_GUIDE.md
- [ ] Playwright patterns match TESTING_WITH_PLAYWRIGHT.md
- [ ] Zero-retry policy enforced per ADR-004
