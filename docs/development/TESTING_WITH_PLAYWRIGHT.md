# E2E Testing with Playwright

**Purpose:** Guide for writing, running, and debugging end-to-end tests with Playwright in Family Hub.

**Migration:** Migrated from Cypress (January 2026). See [ADR-004](docs/architecture/ADR-004-PLAYWRIGHT-MIGRATION.md) for rationale.

---

## Overview

Family Hub uses **Playwright** for E2E testing with these principles:

- **Zero-retry policy:** Forces fixing root causes (no flaky tests)
- **Cross-browser:** Chromium, Firefox, WebKit (iOS Safari)
- **API-first testing:** Verify event chains via GraphQL + RabbitMQ
- **Real backend:** Testcontainers.NET for PostgreSQL isolation
- **Desktop-first:** 1280x720 viewport (mobile deferred to Phase 2)

---

## Quick Start

### Running Tests

```bash
# Navigate to frontend directory
cd src/frontend/family-hub-web

# Run tests in UI mode (interactive, recommended for development)
npm run e2e

# Run tests in headless mode (CI-style)
npm run e2e:headless

# Run specific browser
npm run e2e:chromium
npm run e2e:firefox
npm run e2e:webkit

# Run specific test file
npx playwright test e2e/tests/family-creation.spec.ts

# Debug mode (opens debugger)
npm run e2e:debug

# View HTML report
npm run e2e:report
```

### Test Structure

```
src/frontend/family-hub-web/e2e/
├── fixtures/                  # Reusable test fixtures
│   ├── auth.fixture.ts        # OAuth mocking
│   ├── graphql.fixture.ts     # GraphQL interception
│   └── rabbitmq.fixture.ts    # Event verification
├── support/                   # Test utilities
│   ├── api-helpers.ts         # GraphQL client
│   ├── vogen-mirrors.ts       # TypeScript value objects
│   └── constants.ts           # Test data constants
├── tests/                     # Test specifications
│   ├── family-creation.spec.ts
│   ├── accessibility.spec.ts
│   ├── event-chains.spec.ts
│   └── cross-browser.spec.ts
├── global-setup.ts            # Testcontainers setup
├── global-teardown.ts         # Testcontainers cleanup
└── playwright.config.ts       # Configuration
```

---

## Configuration

### playwright.config.ts

```typescript
export default defineConfig({
  testDir: './e2e',

  // Zero tolerance for flaky tests
  fullyParallel: false,    // Sequential execution (shared backend)
  retries: 0,              // Forces fixing root causes
  workers: 1,              // Single worker (Testcontainers backend)

  // Reporters
  reporter: [
    ['html', { open: 'never', outputFolder: 'playwright-report' }],
    ['junit', { outputFile: 'playwright-report/junit.xml' }],
    ['list']
  ],

  // Global settings
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    actionTimeout: 10000,
    navigationTimeout: 30000,
  },

  // Cross-browser projects
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'], viewport: { width: 1280, height: 720 } } },
    { name: 'firefox',  use: { ...devices['Desktop Firefox'], viewport: { width: 1280, height: 720 } } },
    { name: 'webkit',   use: { ...devices['Desktop Safari'], viewport: { width: 1280, height: 720 } } }
  ],

  // Testcontainers lifecycle
  globalSetup: './e2e/global-setup.ts',
  globalTeardown: './e2e/global-teardown.ts',

  // Start Angular dev server
  webServer: {
    command: 'npm run start',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
  },
});
```

**Key Configurations:**

- `retries: 0` - Zero-retry policy (flaky tests must be fixed)
- `workers: 1` - Sequential execution (shared Testcontainers backend)
- `fullyParallel: false` - No parallel test execution
- `trace: 'on-first-retry'` - Debugging traces (since retries = 0, only on manual retry)

---

## Fixtures

Playwright fixtures replace Cypress custom commands with dependency injection.

### Auth Fixture (OAuth Mocking)

**File:** `e2e/fixtures/auth.fixture.ts`

```typescript
import { test as base, Page } from '@playwright/test';

export type AuthFixture = {
  mockOAuthLogin: () => Promise<void>;
  authenticatedPage: Page;
};

export const test = base.extend<AuthFixture>({
  mockOAuthLogin: async ({ page }, use) => {
    const mockLogin = async () => {
      await page.addInitScript(() => {
        localStorage.setItem('access_token', 'mock_access_token_12345');
        localStorage.setItem('id_token', 'mock_id_token_12345');
        localStorage.setItem('refresh_token', 'mock_refresh_token_12345');
        localStorage.setItem('token_expiry', (Date.now() + 3600000).toString());
      });
    };
    await use(mockLogin);
  },

  authenticatedPage: async ({ page, mockOAuthLogin }, use) => {
    await mockOAuthLogin();
    await use(page);
  },
});

export { expect } from '@playwright/test';
```

**Usage:**

```typescript
import { test, expect } from '../fixtures/auth.fixture';

test('should access protected route', async ({ authenticatedPage }) => {
  await authenticatedPage.goto('/dashboard');
  await expect(authenticatedPage).toHaveURL(/dashboard/);
});
```

### GraphQL Fixture (Request Interception)

**File:** `e2e/fixtures/graphql.fixture.ts`

```typescript
import { test as base, Page, Route } from '@playwright/test';

export type GraphQLFixture = {
  interceptGraphQL: (operationName: string, mockData: any) => Promise<void>;
};

export const test = base.extend<GraphQLFixture>({
  interceptGraphQL: async ({ page }, use) => {
    const intercept = async (operationName: string, mockData: any) => {
      await page.route('**/graphql', async (route: Route) => {
        const request = route.request();
        const postData = request.postDataJSON();

        if (postData?.operationName === operationName) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({ data: mockData }),
          });
        } else {
          await route.continue();
        }
      });
    };
    await use(intercept);
  },
});
```

**Usage:**

```typescript
import { test, expect } from '../fixtures/graphql.fixture';

test('should mock GraphQL query', async ({ page, interceptGraphQL }) => {
  await interceptGraphQL('GetCurrentFamily', {
    family: { id: '123', name: 'Test Family' }
  });

  await page.goto('/family');
  await expect(page.getByText('Test Family')).toBeVisible();
});
```

### RabbitMQ Fixture (Event Verification)

**File:** `e2e/fixtures/rabbitmq.fixture.ts`

```typescript
import { test as base } from '@playwright/test';
import * as amqp from 'amqplib';

export type RabbitMQFixture = {
  rabbitmq: {
    waitForMessage: (predicate: (msg: any) => boolean, timeout: number) => Promise<any>;
    close: () => Promise<void>;
  };
};

export const test = base.extend<RabbitMQFixture>({
  rabbitmq: async ({}, use) => {
    const connection = await amqp.connect('amqp://familyhub:Dev123!@localhost:5672');
    const channel = await connection.createChannel();
    const queue = 'test-events';

    await channel.assertQueue(queue, { durable: false });

    const waitForMessage = async (predicate: (msg: any) => boolean, timeout: number): Promise<any> => {
      return new Promise((resolve, reject) => {
        const timer = setTimeout(() => reject(new Error('Timeout waiting for message')), timeout);

        channel.consume(queue, (msg) => {
          if (msg) {
            const content = JSON.parse(msg.content.toString());
            if (predicate(content)) {
              clearTimeout(timer);
              channel.ack(msg);
              resolve(content);
            }
          }
        });
      });
    };

    const close = async () => {
      await channel.close();
      await connection.close();
    };

    await use({ waitForMessage, close });
    await close();
  },
});
```

**Usage:**

```typescript
import { test, expect } from '../fixtures/rabbitmq.fixture';

test('should publish event to RabbitMQ', async ({ rabbitmq, graphqlClient }) => {
  // Create family via API
  await graphqlClient.mutate(CREATE_FAMILY_MUTATION, { name: 'New Family' });

  // Verify event published
  const event = await rabbitmq.waitForMessage(
    (msg) => msg.eventType === 'FamilyCreatedEvent',
    5000
  );

  expect(event.familyName).toBe('New Family');
});
```

---

## Vogen TypeScript Mirrors

**Purpose:** Mirror backend Vogen value objects in TypeScript for type-safe test data generation.

**File:** `e2e/support/vogen-mirrors.ts`

### Email Value Object

```typescript
export class Email {
  private static readonly EMAIL_REGEX = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
  private static readonly MAX_LENGTH = 320;

  private constructor(public readonly value: string) {}

  static from(value: string): Email {
    const normalized = value.trim().toLowerCase();

    if (!normalized) {
      throw new Error('Email cannot be empty.');
    }

    if (normalized.length > Email.MAX_LENGTH) {
      throw new Error(`Email cannot exceed ${Email.MAX_LENGTH} characters.`);
    }

    if (!Email.EMAIL_REGEX.test(normalized)) {
      throw new Error('Invalid email format.');
    }

    return new Email(normalized);
  }

  toString(): string {
    return this.value;
  }
}
```

### FamilyName Value Object

```typescript
export class FamilyName {
  private static readonly MAX_LENGTH = 100;

  private constructor(public readonly value: string) {}

  static from(value: string): FamilyName {
    const trimmed = value.trim();

    if (!trimmed) {
      throw new Error('Family name cannot be empty.');
    }

    if (trimmed.length > FamilyName.MAX_LENGTH) {
      throw new Error(`Family name cannot exceed ${FamilyName.MAX_LENGTH} characters.`);
    }

    return new FamilyName(trimmed);
  }

  toString(): string {
    return this.value;
  }
}
```

### Usage in Tests

```typescript
import { Email, FamilyName } from '../support/vogen-mirrors';

test('should validate email format', () => {
  // Valid emails
  const email = Email.from('user@example.com');
  expect(email.value).toBe('user@example.com');

  // Invalid emails (throws)
  expect(() => Email.from('')).toThrow('Email cannot be empty');
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

---

## API-First Testing Approach

**Philosophy:** Test backend logic directly via GraphQL API, then spot-check UI.

### Pattern

```typescript
test('event chain: doctor appointment → calendar event', async ({ graphqlClient, rabbitmq, page }) => {
  // 1. Create via GraphQL API (not UI clicks)
  const appointmentResult = await graphqlClient.mutate(CREATE_APPOINTMENT_MUTATION, {
    doctorName: 'Dr. Smith',
    date: '2026-02-15T10:00:00Z',
  });
  const appointmentId = appointmentResult.data.createAppointment.id;

  // 2. Verify RabbitMQ event published
  const event = await rabbitmq.waitForMessage(
    (msg) => msg.eventType === 'HealthAppointmentScheduled' && msg.appointmentId === appointmentId,
    5000
  );
  expect(event).toBeDefined();

  // 3. Query backend to verify calendar event created
  const calendarResult = await graphqlClient.query(GET_CALENDAR_EVENTS, {
    startDate: '2026-02-01',
    endDate: '2026-02-28',
  });
  const calendarEvent = calendarResult.data.calendarEvents.find(
    (e) => e.title.includes('Dr. Smith')
  );
  expect(calendarEvent).toBeDefined();

  // 4. Spot-check UI (optional, lightweight)
  await page.goto('/calendar');
  await expect(page.getByText('Doctor: Dr. Smith')).toBeVisible();
});
```

**Benefits:**

- Fast (no UI navigation)
- Reliable (no UI timing issues)
- Comprehensive (tests backend event chains directly)
- UI verification is optional fallback

---

## Page Object Model

**Use sparingly** - prefer API-first testing. Use Page Objects only for complex UI interactions.

### Example: Family Creation Page

**File:** `e2e/pages/family-creation.page.ts`

```typescript
import { Page, Locator } from '@playwright/test';

export class FamilyCreationPage {
  readonly page: Page;
  readonly familyNameInput: Locator;
  readonly submitButton: Locator;
  readonly successMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    this.familyNameInput = page.getByLabel('Family Name');
    this.submitButton = page.getByRole('button', { name: 'Create Family' });
    this.successMessage = page.getByText('Family created successfully');
  }

  async goto() {
    await this.page.goto('/family/create');
  }

  async createFamily(name: string) {
    await this.familyNameInput.fill(name);
    await this.submitButton.click();
  }

  async waitForSuccess() {
    await this.successMessage.waitFor({ state: 'visible', timeout: 5000 });
  }
}
```

**Usage:**

```typescript
import { test, expect } from '@playwright/test';
import { FamilyCreationPage } from '../pages/family-creation.page';

test('should create family via UI', async ({ page }) => {
  const familyPage = new FamilyCreationPage(page);

  await familyPage.goto();
  await familyPage.createFamily('Test Family');
  await familyPage.waitForSuccess();

  await expect(familyPage.successMessage).toBeVisible();
});
```

---

## Zero-Retry Policy

**Philosophy:** Flaky tests must be fixed immediately. No retries allowed.

### Configuration

```typescript
// playwright.config.ts
export default defineConfig({
  retries: 0,  // Zero tolerance for flaky tests
});
```

### When Tests Fail

1. **Investigate immediately** - Don't ignore intermittent failures
2. **Reproduce locally** - Run test 10+ times to confirm flakiness
3. **Identify root cause:**
   - Timing issues (missing `waitFor`)
   - Test isolation (shared state between tests)
   - Network issues (slow GraphQL responses)
   - Race conditions (parallel operations)

### Common Fixes

#### Problem: Element not found

**❌ Bad (adds arbitrary wait):**

```typescript
await page.waitForTimeout(2000);  // Anti-pattern!
await page.click('button');
```

**✅ Good (wait for specific condition):**

```typescript
await page.getByRole('button', { name: 'Submit' }).waitFor({ state: 'visible' });
await page.getByRole('button', { name: 'Submit' }).click();
```

#### Problem: Race condition with GraphQL

**❌ Bad (hope it finishes in time):**

```typescript
await graphqlClient.mutate(CREATE_FAMILY_MUTATION);
await page.reload();  // Might reload before mutation completes
```

**✅ Good (wait for result):**

```typescript
const result = await graphqlClient.mutate(CREATE_FAMILY_MUTATION);
expect(result.data.createFamily.id).toBeDefined();
await page.reload();
```

#### Problem: Test pollution (shared state)

**❌ Bad (tests depend on execution order):**

```typescript
test('test 1', async () => {
  // Creates family "Test Family"
});

test('test 2', async () => {
  // Assumes "Test Family" exists from test 1
});
```

**✅ Good (each test is independent):**

```typescript
test('test 1', async ({ graphqlClient }) => {
  // Create family for this test
  await graphqlClient.mutate(CREATE_FAMILY_MUTATION, { name: 'Test 1 Family' });
  // Test logic
});

test('test 2', async ({ graphqlClient }) => {
  // Create family for this test
  await graphqlClient.mutate(CREATE_FAMILY_MUTATION, { name: 'Test 2 Family' });
  // Test logic
});
```

---

## Accessibility Testing

Family Hub uses **@axe-core/playwright** for automated accessibility checks (WCAG 2.1 AA compliance).

### Setup

```typescript
import { test, expect } from '@playwright/test';
import { injectAxe, checkA11y } from '@axe-core/playwright';

test('should have no accessibility violations', async ({ page }) => {
  await page.goto('/dashboard');

  // Inject axe-core
  await injectAxe(page);

  // Run accessibility checks
  await checkA11y(page, null, {
    detailedReport: true,
    detailedReportOptions: { html: true },
  });
});
```

### Scoped Checks

```typescript
// Check specific component
await checkA11y(page, '.sidebar-component');

// Check with specific rules
await checkA11y(page, null, {
  rules: {
    'color-contrast': { enabled: true },
    'label': { enabled: true },
  },
});

// Exclude specific elements
await checkA11y(page, null, {
  exclude: [['.third-party-widget']],
});
```

### Common Violations

| Violation | Fix |
|-----------|-----|
| Missing alt text | Add `alt="description"` to `<img>` tags |
| Low color contrast | Increase contrast to 4.5:1 minimum |
| Missing form labels | Add `<label>` or `aria-label` to inputs |
| Missing ARIA roles | Add appropriate `role` attributes |
| Keyboard navigation | Ensure `tabindex` and focus states |

---

## Debugging Flaky Tests

### 1. UI Mode (Interactive Debugging)

```bash
npm run e2e  # Opens Playwright UI mode
```

**Features:**

- Step through test execution
- See screenshots at each step
- Time travel through test
- Inspect DOM at any point

### 2. Debug Mode (Breakpoints)

```bash
npm run e2e:debug

# Or in specific test
npx playwright test --debug e2e/tests/family-creation.spec.ts
```

Add breakpoints in code:

```typescript
test('debug test', async ({ page }) => {
  await page.goto('/dashboard');
  await page.pause();  // Pauses execution here
  // Continue debugging interactively
});
```

### 3. Trace Viewer

When test fails, traces are captured:

```bash
npx playwright show-trace playwright-results/trace.zip
```

**Trace includes:**

- Screenshots at each action
- Network requests
- Console logs
- DOM snapshots

### 4. Verbose Logging

```typescript
test('debug test', async ({ page }) => {
  // Log all navigation
  page.on('request', req => console.log('>>', req.method(), req.url()));
  page.on('response', res => console.log('<<', res.status(), res.url()));

  // Log console messages
  page.on('console', msg => console.log('Console:', msg.text()));
});
```

### 5. Run Test Multiple Times

```bash
# Run test 10 times to reproduce flakiness
for i in {1..10}; do
  npx playwright test e2e/tests/family-creation.spec.ts
  if [ $? -ne 0 ]; then
    echo "Failed on iteration $i"
    break
  fi
done
```

---

## Test Data Management

### Constants File

**File:** `e2e/support/constants.ts`

```typescript
export const TEST_DATA = {
  users: {
    testUser: {
      email: 'test@example.com',
      password: 'TestPassword123!',
    },
  },
  families: {
    testFamily: {
      name: 'Test Family',
    },
  },
};

export const STORAGE_KEYS = {
  ACCESS_TOKEN: 'access_token',
  ID_TOKEN: 'id_token',
  REFRESH_TOKEN: 'refresh_token',
  TOKEN_EXPIRY: 'token_expiry',
};

export const GRAPHQL_ENDPOINTS = {
  LOCAL: 'http://localhost:7000/graphql',
  CI: process.env.GRAPHQL_URL || 'http://localhost:7000/graphql',
};
```

### API Helpers

**File:** `e2e/support/api-helpers.ts`

```typescript
import { request } from '@playwright/test';

export class GraphQLClient {
  private baseURL: string;
  private accessToken?: string;

  constructor(baseURL: string, accessToken?: string) {
    this.baseURL = baseURL;
    this.accessToken = accessToken;
  }

  async query(query: string, variables?: any) {
    const context = await request.newContext();
    const response = await context.post(this.baseURL, {
      headers: {
        'Content-Type': 'application/json',
        ...(this.accessToken && { Authorization: `Bearer ${this.accessToken}` }),
      },
      data: { query, variables },
    });

    return response.json();
  }

  async mutate(mutation: string, variables?: any) {
    return this.query(mutation, variables);
  }
}
```

---

## CI/CD Integration

### GitHub Actions Workflow

```yaml
name: Playwright E2E Tests

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]
  workflow_dispatch:  # Manual trigger only

jobs:
  e2e-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'

      - name: Install dependencies
        run: |
          cd src/frontend/family-hub-web
          npm ci

      - name: Install Playwright browsers
        run: |
          cd src/frontend/family-hub-web
          npx playwright install --with-deps

      - name: Start Docker services
        run: |
          cd infrastructure/docker
          docker-compose up -d

      - name: Run Playwright tests
        run: |
          cd src/frontend/family-hub-web
          npm run e2e:headless

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-report
          path: src/frontend/family-hub-web/playwright-report/
          retention-days: 30

      - name: Upload traces
        if: failure()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-traces
          path: src/frontend/family-hub-web/playwright-results/
          retention-days: 7
```

---

## Best Practices

### ✅ DO

- Use API-first testing for event chain verification
- Create TypeScript mirrors for Vogen value objects
- Use fixtures for authentication and GraphQL mocking
- Write independent, isolated tests
- Use descriptive test names (`should create family with valid name`)
- Use Playwright auto-waiting (avoid manual waits)
- Fix flaky tests immediately (zero-retry policy)
- Run accessibility checks with @axe-core/playwright

### ❌ DON'T

- Use `waitForTimeout()` (anti-pattern, creates flakiness)
- Share state between tests (causes test pollution)
- Ignore intermittent failures (zero-retry policy)
- Test UI extensively (prefer API-first testing)
- Use Cypress patterns (this is Playwright, different APIs)
- Skip cross-browser testing (run all 3: Chromium, Firefox, WebKit)
- Hardcode test data (use constants and Vogen mirrors)

---

## Related Documentation

- **ADR-004:** [Playwright Migration Rationale](docs/architecture/ADR-004-PLAYWRIGHT-MIGRATION.md)
- **Coding Standards:** [Testing Patterns](CODING_STANDARDS.md#testing)
- **Workflows:** [Testing Workflows](WORKFLOWS.md#testing-patterns)
- **Local Setup:** [Running E2E Tests](LOCAL_DEVELOPMENT_SETUP.md#testing-your-setup)

---

**Last Updated:** 2026-01-09
**Version:** 1.0.0
**Zero-Retry Policy:** Enabled (retries: 0)
