# E2E Tests - Family Hub

End-to-end tests for Family Hub using Playwright.

## Prerequisites

### 1. Install Playwright

```bash
# From the frontend directory
cd src/frontend/family-hub-web

# Install Playwright
npm install -D @playwright/test

# Install browsers
npx playwright install
```

### 2. Configure Playwright

Create `playwright.config.ts` in the frontend root:

```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: '../../../e2e',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: 0, // Zero retry policy!
  workers: 1,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],
  webServer: {
    command: 'npm start',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env.CI,
  },
});
```

### 3. Start Services

Before running tests, ensure all services are running:

```bash
# Terminal 1: Docker services (Keycloak + PostgreSQL)
docker-compose up

# Terminal 2: Backend API
cd src/FamilyHub.Api
dotnet run

# Terminal 3: Frontend (automatically started by Playwright if configured)
cd src/frontend/family-hub-web
npm start
```

## Running Tests

### Run all E2E tests

```bash
cd src/frontend/family-hub-web
npx playwright test
```

### Run specific test file

```bash
npx playwright test e2e/auth/oauth-complete-flow.spec.ts
```

### Run with UI mode (debugging)

```bash
npx playwright test --ui
```

### Run in headed mode (see browser)

```bash
npx playwright test --headed
```

### Run specific browser

```bash
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit
```

## Test Structure

```
e2e/
├── auth/
│   └── oauth-complete-flow.spec.ts  # OAuth login flow tests
└── README.md                         # This file
```

## Test Scenarios Covered

### OAuth Login Flow (`oauth-complete-flow.spec.ts`)

1. **Happy Path:**
   - User navigates to app (redirects to login)
   - User clicks "Sign in with Keycloak"
   - User enters credentials in Keycloak
   - Callback processes with progress indicators
   - Dashboard displays user data from backend
   - Family membership shown (if exists)

2. **Backend Sync Failure:**
   - RegisterUser mutation fails
   - Error message displayed
   - Retry button available

3. **Loading State:**
   - Dashboard shows loading skeleton
   - Transitions to user data

4. **Logout:**
   - User clicks logout
   - Redirects to Keycloak/login
   - localStorage cleared

## Test Data

### Keycloak Test User

Create a test user in Keycloak:

1. Navigate to http://localhost:8080/admin
2. Login with admin credentials
3. Go to Users → Add User
4. Set:
   - Username: `test@example.com`
   - Email: `test@example.com`
   - Email Verified: ON
5. Set password in Credentials tab:
   - Password: `test123`
   - Temporary: OFF

## Standards Followed

- **Zero Retry Policy:** Tests must be stable, no retries
- **Multi-Browser:** Tests run on Chromium, Firefox, WebKit
- **API-First:** Use GraphQL mocking when needed
- **Data-testid:** Use `data-testid` for selectors (when available)

## Troubleshooting

### Tests fail with "Navigation timeout"

- Ensure all services are running (Keycloak, Backend, Frontend)
- Check service URLs match configuration
- Increase timeout if backend is slow: `{ timeout: 15000 }`

### Tests fail with "Element not found"

- Run test in headed mode to see what's happening: `--headed`
- Use Playwright Inspector: `PWDEBUG=1 npx playwright test`

### Backend sync fails

- Check browser console in headed mode
- Verify JWT token in localStorage
- Check backend logs for errors

## CI/CD Integration

To run in CI/CD pipeline:

```bash
# Install dependencies
npm ci
npx playwright install --with-deps

# Run tests in CI mode
CI=true npx playwright test

# Generate HTML report
npx playwright show-report
```

## Future Improvements

- [ ] Add Page Object Model for better maintainability
- [ ] Add API testing utilities for setup/teardown
- [ ] Add visual regression testing
- [ ] Add performance testing
- [ ] Add accessibility testing
