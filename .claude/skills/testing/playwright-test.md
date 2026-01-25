---
name: playwright-test
description: Create Playwright E2E test with API-first approach
category: testing
inputs:
  - feature: Feature name (e.g., family-creation)
  - module: DDD module name (e.g., auth, calendar)
---

# Playwright E2E Test Skill

Creates a Playwright E2E test following API-first approach with zero retry policy.

## Files Created

1. `e2e/{module}/{feature}.spec.ts` - Test file
2. `e2e/{module}/fixtures/{feature}.fixture.ts` - Test fixtures (if needed)
3. `e2e/{module}/pages/{feature}.page.ts` - Page object (if needed)

## Step 1: Create Test File

Location: `src/frontend/family-hub-web/e2e/{module}/{feature}.spec.ts`

```typescript
import { test, expect } from '@playwright/test';

test.describe('{Feature} E2E Tests', () => {

  test.beforeEach(async ({ request }) => {
    // API-first: Setup test data via GraphQL
    await request.post('/api/graphql', {
      data: {
        query: `mutation { createTestData { id } }`
      }
    });
  });

  test('should successfully complete {feature}', async ({ page }) => {
    // Navigate
    await page.goto('/{route}');

    // Interact
    await page.fill('[data-testid="{field}-input"]', 'Test Value');
    await page.click('[data-testid="submit-button"]');

    // Assert
    await expect(page).toHaveURL(/\/{expected-route}/);
    await expect(page.locator('[data-testid="success-message"]'))
      .toContainText('Success');
  });

  test('should show validation error for invalid input', async ({ page }) => {
    await page.goto('/{route}');

    // Submit empty form
    await page.click('[data-testid="submit-button"]');

    // Assert validation error
    await expect(page.locator('[data-testid="error-message"]'))
      .toBeVisible();
  });

  test.afterEach(async ({ request }) => {
    // Cleanup test data
    await request.post('/api/graphql', {
      data: {
        query: `mutation { cleanupTestData }`
      }
    });
  });
});
```

## Step 2: Create Page Object (for complex pages)

Location: `src/frontend/family-hub-web/e2e/{module}/pages/{feature}.page.ts`

```typescript
import { Page, Locator, expect } from '@playwright/test';

export class {Feature}Page {
  readonly page: Page;
  readonly nameInput: Locator;
  readonly submitButton: Locator;
  readonly successMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    this.nameInput = page.locator('[data-testid="name-input"]');
    this.submitButton = page.locator('[data-testid="submit-button"]');
    this.successMessage = page.locator('[data-testid="success-message"]');
  }

  async goto() {
    await this.page.goto('/{route}');
  }

  async fillForm(name: string) {
    await this.nameInput.fill(name);
  }

  async submit() {
    await this.submitButton.click();
  }

  async expectSuccess(message: string) {
    await expect(this.successMessage).toContainText(message);
  }
}
```

Usage in test:

```typescript
import { {Feature}Page } from './pages/{feature}.page';

test('should complete {feature}', async ({ page }) => {
  const featurePage = new {Feature}Page(page);

  await featurePage.goto();
  await featurePage.fillForm('Test Value');
  await featurePage.submit();
  await featurePage.expectSuccess('Created successfully');
});
```

## Step 3: API-First Test Data Setup

```typescript
import { test, expect } from '@playwright/test';

// Helper for GraphQL requests
async function graphql(request: APIRequestContext, query: string, variables = {}) {
  const response = await request.post('/api/graphql', {
    data: { query, variables }
  });
  return response.json();
}

test.describe('{Feature}', () => {
  let testFamilyId: string;

  test.beforeAll(async ({ request }) => {
    // Create test data via API (not UI)
    const result = await graphql(request, `
      mutation CreateTestFamily($input: CreateFamilyInput!) {
        createFamily(input: $input) { familyId }
      }
    `, { input: { name: 'Test Family' } });

    testFamilyId = result.data.createFamily.familyId;
  });

  test.afterAll(async ({ request }) => {
    // Cleanup
    await graphql(request, `
      mutation DeleteTestFamily($id: ID!) {
        deleteFamily(id: $id)
      }
    `, { id: testFamilyId });
  });
});
```

## Configuration Reference

From `playwright.config.ts`:

```typescript
export default defineConfig({
  retries: 0,  // Zero retry policy!
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    { name: 'webkit', use: { ...devices['Desktop Safari'] } },
  ],
});
```

## Data-TestId Convention

Use consistent `data-testid` attributes:

- Inputs: `{field}-input`
- Buttons: `{action}-button`
- Messages: `{type}-message` (error, success, info)
- Lists: `{item}-list`, `{item}-list-item`

## Verification

- [ ] Test uses API-first data setup
- [ ] Uses data-testid selectors
- [ ] Covers happy path
- [ ] Covers validation errors
- [ ] Cleans up test data
- [ ] Works in all 3 browsers
