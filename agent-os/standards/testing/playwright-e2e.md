# Playwright E2E Testing

API-first testing approach. Zero retry policy. Multi-browser support.

## Test Structure

```typescript
import { test, expect } from '@playwright/test';

test.describe('Family Management', () => {
  test('creates a new family', async ({ page }) => {
    await page.goto('/families/create');
    await page.fill('[data-testid="family-name"]', 'Smith Family');
    await page.click('[data-testid="submit-button"]');

    await expect(page).toHaveURL(/\/families\/[a-z0-9-]+/);
    await expect(page.locator('h1')).toContainText('Smith Family');
  });
});
```

## API-First Setup

```typescript
test.beforeEach(async ({ request }) => {
  // Create test data via API
  await request.post('/api/graphql', {
    data: {
      query: `mutation { createTestFamily(name: "Test") { id } }`
    }
  });
});
```

## Page Object Model

```typescript
// pages/family.page.ts
export class FamilyPage {
  constructor(private page: Page) {}

  async createFamily(name: string) {
    await this.page.fill('[data-testid="family-name"]', name);
    await this.page.click('[data-testid="submit"]');
  }

  async expectFamilyCreated(name: string) {
    await expect(this.page.locator('h1')).toContainText(name);
  }
}
```

## Configuration

```typescript
// playwright.config.ts
export default defineConfig({
  retries: 0,  // Zero retry policy!
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    { name: 'webkit', use: { ...devices['Desktop Safari'] } },
  ],
});
```

## Rules

- Zero retries - fix flaky tests, don't mask them
- Use data-testid for selectors
- API-first: setup data via GraphQL, not UI
- Multi-browser: test chromium, firefox, webkit
- Location: `e2e/{feature}.spec.ts`
