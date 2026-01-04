# E2E Tests - Playwright

This directory contains end-to-end tests for the Family Hub Angular application using Playwright.

## Structure

- **fixtures/** - Reusable test fixtures (auth, GraphQL, RabbitMQ)
- **support/** - Helper utilities and constants
- **tests/** - Actual test files (.spec.ts)
- **global-setup.ts** - Testcontainers lifecycle (start services)
- **global-teardown.ts** - Cleanup (stop services)

## Running Tests

```bash
# UI mode (interactive)
npm run e2e

# Headless mode
npm run e2e:headless

# Debug mode
npm run e2e:debug

# Single browser
npm run e2e:chromium
npm run e2e:firefox
npm run e2e:webkit

# View HTML report
npm run e2e:report
```

## Key Principles

1. **Zero tolerance for flaky tests** - `retries: 0` in config
2. **Cross-browser testing** - Chromium, Firefox, WebKit from day one
3. **Real backend** - Testcontainers (PostgreSQL + RabbitMQ + .NET API)
4. **API-first testing** - Use Playwright APIRequestContext for event chains
5. **Desktop-first** - 1280x720 viewport (mobile in Phase 2)

## Test Patterns

### Using Fixtures

```typescript
import { test, expect } from '../fixtures/auth.fixture';

test('should access protected page', async ({ authenticatedPage }) => {
  await authenticatedPage.goto('/dashboard');
  await expect(authenticatedPage).toHaveURL(/dashboard/);
});
```

### Using test.step()

```typescript
test('should create family', async ({ page }) => {
  await test.step('Navigate to wizard', async () => {
    await page.goto('/family/create');
  });

  await test.step('Fill family name', async () => {
    await page.locator('input[aria-label="Family name"]').fill('Smith Family');
  });

  await test.step('Submit form', async () => {
    await page.getByRole('button', { name: 'Create Family' }).click();
  });
});
```

## Documentation

- Migration plan: `/docs/architecture/ADR-004-PLAYWRIGHT-MIGRATION.md`
- Playwright docs: https://playwright.dev/
- Family Hub testing guide: `/docs/testing-guide.md`
