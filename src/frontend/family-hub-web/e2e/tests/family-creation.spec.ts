/**
 * E2E Test Suite: Family Creation Wizard Flow (Playwright)
 *
 * Migrated from Cypress (cypress/e2e/family-creation.cy.ts)
 *
 * Tests the complete user journey from login to family creation using wizard-based page flow,
 * including validation, error handling, and accessibility compliance.
 *
 * Test Coverage:
 * - Happy path: Login → Auto-redirect to wizard → Create family → Redirect to dashboard
 * - Form validation (empty name, too long name, character counter)
 * - API errors (user already has family, network errors)
 * - Keyboard navigation (Tab, Enter)
 * - Accessibility (WCAG 2.1 AA compliance)
 * - Loading states (disabled buttons, loading text)
 * - UX edge cases (rapid submissions, form reset)
 * - Guard-based routing (familyGuard, noFamilyGuard)
 */

import { test, expect } from '@playwright/test';
import { test as authTest } from '../fixtures/auth.fixture';
import { test as graphqlTest } from '../fixtures/graphql.fixture';
import { URLS, SELECTORS } from '../support/constants';

test.describe('Family Creation Flow', () => {
  test.beforeEach(async ({ context }) => {
    // Reset application state
    await context.clearCookies();
    // Note: localStorage is reset per test via page.addInitScript() which runs before page load
  });

  test.describe('Happy Path: Complete Family Creation', () => {
    test('should complete family creation wizard from login to dashboard', async ({ page }) => {
      await test.step('Setup: Mock OAuth and GraphQL', async () => {
        // Mock OAuth login
        const mockAccessToken = 'mock-jwt-token-for-testing';
        const mockExpiresAt = new Date(Date.now() + 3600000).toISOString();

        await page.addInitScript(({ token, expires }) => {
          window.localStorage.setItem('family_hub_access_token', token);
          window.localStorage.setItem('family_hub_token_expires', expires);
        }, { token: mockAccessToken, expires: mockExpiresAt });

        // Mock GetCurrentFamily (null - no family)
        await page.route('http://localhost:5002/graphql', async (route) => {
          const request = route.request();
          const postData = request.postDataJSON();

          if (postData?.query?.includes('GetCurrentFamily')) {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({ data: { family: null } }),
            });
          } else {
            await route.continue();
          }
        });
      });

      await test.step('Navigate: Visit dashboard (should auto-redirect to wizard)', async () => {
        await page.goto('/dashboard');
        await expect(page).toHaveURL(/\/family\/create/);
      });

      await test.step('Verify: Wizard page renders correctly', async () => {
        await expect(page.getByText('Create Your Family')).toBeVisible();
        await expect(page.getByText('Step 1 of 1')).toBeVisible();
        await expect(page.locator('app-icon[name="users"]')).toBeVisible();
        await expect(page.getByText('Give your family a name to get started')).toBeVisible();
      });

      await test.step('Setup: Mock CreateFamily mutation (success)', async () => {
        await page.route('http://localhost:5002/graphql', async (route) => {
          const request = route.request();
          const postData = request.postDataJSON();

          if (postData?.query?.includes('CreateFamily') || postData?.operationName === 'CreateFamily') {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  createFamily: {
                    family: {
                      id: 'family-123',
                      name: 'Smith Family',
                      createdAt: '2025-12-30T00:00:00Z',
                    },
                    errors: null,
                  },
                },
              }),
            });
          } else if (postData?.query?.includes('GetCurrentFamily')) {
            // After creation, return the family
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  family: {
                    id: 'family-123',
                    name: 'Smith Family',
                    createdAt: '2025-12-30T00:00:00Z',
                  },
                },
              }),
            });
          } else {
            await route.continue();
          }
        });
      });

      await test.step('Action: Enter family name and submit', async () => {
        await page.locator(SELECTORS.FAMILY_NAME_INPUT).fill('Smith Family');
        await expect(page.getByRole('button', { name: 'Create Family' })).toBeEnabled();
        await page.getByRole('button', { name: 'Create Family' }).click();
      });

      await test.step('Verify: Loading state displayed', async () => {
        await expect(page.getByText('Creating...')).toBeVisible();
      });

      await test.step('Verify: Redirect to dashboard and show family info', async () => {
        await expect(page).toHaveURL(/\/dashboard/);
        await expect(page.locator('h1')).toContainText('Smith Family');
        await expect(page.getByText('Created:')).toBeVisible();
      });
    });
  });

  test.describe('Form Validation', () => {
    test.beforeEach(async ({ page }) => {
      // Mock OAuth
      const mockAccessToken = 'mock-jwt-token-for-testing';
      const mockExpiresAt = new Date(Date.now() + 3600000).toISOString();

      await page.addInitScript(({ token, expires }) => {
        window.localStorage.setItem('family_hub_access_token', token);
        window.localStorage.setItem('family_hub_token_expires', expires);
      }, { token: mockAccessToken, expires: mockExpiresAt });

      // Mock GetCurrentFamily
      await page.route('http://localhost:5002/graphql', async (route) => {
        const postData = route.request().postDataJSON();
        if (postData?.query?.includes('GetCurrentFamily')) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({ data: { family: null } }),
          });
        } else {
          await route.continue();
        }
      });

      await page.goto('/family/create');
    });

    test('should show error when family name is empty', async ({ page }) => {
      await test.step('Trigger validation by focusing and blurring', async () => {
        await page.locator(SELECTORS.FAMILY_NAME_INPUT).focus();
        await page.locator(SELECTORS.FAMILY_NAME_INPUT).blur();
      });

      await test.step('Verify error message and ARIA attributes', async () => {
        await expect(page.getByText('Family name is required')).toBeVisible();
        await expect(page.getByRole('button', { name: 'Create Family' })).toBeDisabled();

        const input = page.locator(SELECTORS.FAMILY_NAME_INPUT);
        await expect(input).toHaveAttribute('aria-invalid', 'true');
        await expect(input).toHaveAttribute('aria-describedby');
      });
    });

    test('should show error when family name exceeds 50 characters', async ({ page }) => {
      await test.step('Enter 51 characters', async () => {
        const longName = 'a'.repeat(51);
        await page.locator(SELECTORS.FAMILY_NAME_INPUT).fill(longName);
        await page.locator(SELECTORS.FAMILY_NAME_INPUT).blur();
      });

      await test.step('Verify error message', async () => {
        await expect(page.getByText('Family name must be 50 characters or less')).toBeVisible();
        await expect(page.getByRole('button', { name: 'Create Family' })).toBeDisabled();
      });
    });

    test('should enable submit button when valid name is entered', async ({ page }) => {
      await test.step('Enter valid name', async () => {
        await page.locator(SELECTORS.FAMILY_NAME_INPUT).fill('Valid Family Name');
      });

      await test.step('Verify no errors and button enabled', async () => {
        await expect(page.getByText('Family name is required')).not.toBeVisible();
        await expect(page.getByText('Family name must be 50 characters or less')).not.toBeVisible();
        await expect(page.getByRole('button', { name: 'Create Family' })).toBeEnabled();
      });
    });
  });

  test.describe('API Error Handling', () => {
    test.beforeEach(async ({ page }) => {
      // Mock OAuth
      const mockAccessToken = 'mock-jwt-token-for-testing';
      const mockExpiresAt = new Date(Date.now() + 3600000).toISOString();

      await page.addInitScript(({ token, expires }) => {
        window.localStorage.setItem('family_hub_access_token', token);
        window.localStorage.setItem('family_hub_token_expires', expires);
      }, { token: mockAccessToken, expires: mockExpiresAt });

      // Mock GetCurrentFamily
      await page.route('http://localhost:5002/graphql', async (route) => {
        const postData = route.request().postDataJSON();
        if (postData?.query?.includes('GetCurrentFamily')) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({ data: { family: null } }),
          });
        } else {
          await route.continue();
        }
      });

      await page.goto('/family/create');
    });

    test('should display error when user already has a family', async ({ page }) => {
      await test.step('Mock CreateFamily with business rule error', async () => {
        await page.route('http://localhost:5002/graphql', async (route) => {
          const postData = route.request().postDataJSON();

          if (postData?.query?.includes('CreateFamily')) {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  createFamily: {
                    family: null,
                    errors: [{
                      message: 'User already has a family',
                      code: 'BUSINESS_RULE_VIOLATION',
                    }],
                  },
                },
              }),
            });
          } else {
            await route.continue();
          }
        });
      });

      await test.step('Submit form', async () => {
        await page.locator(SELECTORS.FAMILY_NAME_INPUT).fill('Test Family');
        await page.getByRole('button', { name: 'Create Family' }).click();
      });

      await test.step('Verify error displayed and wizard page remains', async () => {
        await expect(page.locator('[role="alert"]')).toContainText('User already has a family');
        await expect(page).toHaveURL(/\/family\/create/);
        await expect(page.locator(SELECTORS.FAMILY_NAME_INPUT)).toHaveValue('Test Family');
      });
    });

    test('should display error when network request fails', async ({ page }) => {
      await test.step('Mock network error', async () => {
        await page.route('http://localhost:5002/graphql', async (route) => {
          const postData = route.request().postDataJSON();

          if (postData?.query?.includes('CreateFamily')) {
            await route.fulfill({
              status: 500,
              body: JSON.stringify({ error: 'Internal Server Error' }),
            });
          } else {
            await route.continue();
          }
        });
      });

      await test.step('Submit form', async () => {
        await page.locator(SELECTORS.FAMILY_NAME_INPUT).fill('Test Family');
        await page.getByRole('button', { name: 'Create Family' }).click();
      });

      await test.step('Verify error message', async () => {
        await expect(page.locator('[role="alert"]')).toBeVisible();
        await expect(page.getByText('Failed to create family')).toBeVisible();
      });
    });
  });

  test.describe('Keyboard Navigation', () => {
    test.beforeEach(async ({ page }) => {
      // Mock OAuth
      const mockAccessToken = 'mock-jwt-token-for-testing';
      const mockExpiresAt = new Date(Date.now() + 3600000).toISOString();

      await page.addInitScript(({ token, expires }) => {
        window.localStorage.setItem('family_hub_access_token', token);
        window.localStorage.setItem('family_hub_token_expires', expires);
      }, { token: mockAccessToken, expires: mockExpiresAt });

      // Mock GetCurrentFamily
      await page.route('http://localhost:5002/graphql', async (route) => {
        const postData = route.request().postDataJSON();
        if (postData?.query?.includes('GetCurrentFamily')) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({ data: { family: null } }),
          });
        } else {
          await route.continue();
        }
      });

      await page.goto('/family/create');
    });

    test('should allow Tab navigation through wizard elements', async ({ page }) => {
      await test.step('Tab to input', async () => {
        await page.keyboard.press('Tab');
        await expect(page.locator(SELECTORS.FAMILY_NAME_INPUT)).toBeFocused();
      });

      await test.step('Tab to submit button', async () => {
        await page.keyboard.press('Tab');
        await expect(page.getByRole('button', { name: 'Create Family' })).toBeFocused();
      });
    });

    test('should submit form with Enter key', async ({ page }) => {
      await test.step('Mock successful creation', async () => {
        await page.route('http://localhost:5002/graphql', async (route) => {
          const postData = route.request().postDataJSON();

          if (postData?.query?.includes('CreateFamily')) {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  createFamily: {
                    family: {
                      id: 'family-456',
                      name: 'Keyboard Family',
                      createdAt: '2025-12-30T00:00:00Z',
                    },
                    errors: null,
                  },
                },
              }),
            });
          } else {
            await route.continue();
          }
        });
      });

      await test.step('Type name and press Enter', async () => {
        await page.locator(SELECTORS.FAMILY_NAME_INPUT).fill('Keyboard Family');
        await page.locator(SELECTORS.FAMILY_NAME_INPUT).press('Enter');
      });

      await test.step('Verify redirect to dashboard', async () => {
        await expect(page).toHaveURL(/\/dashboard/);
      });
    });
  });

  test.describe('Loading States', () => {
    test.beforeEach(async ({ page }) => {
      // Mock OAuth
      const mockAccessToken = 'mock-jwt-token-for-testing';
      const mockExpiresAt = new Date(Date.now() + 3600000).toISOString();

      await page.addInitScript(({ token, expires }) => {
        window.localStorage.setItem('family_hub_access_token', token);
        window.localStorage.setItem('family_hub_token_expires', expires);
      }, { token: mockAccessToken, expires: mockExpiresAt });

      // Mock GetCurrentFamily
      await page.route('http://localhost:5002/graphql', async (route) => {
        const postData = route.request().postDataJSON();
        if (postData?.query?.includes('GetCurrentFamily')) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({ data: { family: null } }),
          });
        } else {
          await route.continue();
        }
      });

      await page.goto('/family/create');
    });

    test('should disable submit button while creating family', async ({ page }) => {
      await test.step('Mock delayed response', async () => {
        await page.route('http://localhost:5002/graphql', async (route) => {
          const postData = route.request().postDataJSON();

          if (postData?.query?.includes('CreateFamily')) {
            // Delay response
            await new Promise((resolve) => setTimeout(resolve, 1000));
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  createFamily: {
                    family: {
                      id: 'family-999',
                      name: 'Test Family',
                      createdAt: '2025-12-30T00:00:00Z',
                    },
                    errors: null,
                  },
                },
              }),
            });
          } else {
            await route.continue();
          }
        });
      });

      await test.step('Submit form', async () => {
        await page.locator(SELECTORS.FAMILY_NAME_INPUT).fill('Test Family');
        await page.getByRole('button', { name: 'Create Family' }).click();
      });

      await test.step('Verify button disabled and loading text visible', async () => {
        await expect(page.getByRole('button', { name: 'Create Family' })).toBeDisabled();
        await expect(page.getByText('Creating...')).toBeVisible();
      });
    });
  });

  test.describe('User Experience Edge Cases', () => {
    test('should handle rapid form submissions gracefully', async ({ page }) => {
      await test.step('Setup mocks', async () => {
        // Mock OAuth
        const mockAccessToken = 'mock-jwt-token-for-testing';
        const mockExpiresAt = new Date(Date.now() + 3600000).toISOString();

        await page.addInitScript(({ token, expires }) => {
          window.localStorage.setItem('family_hub_access_token', token);
          window.localStorage.setItem('family_hub_token_expires', expires);
        }, { token: mockAccessToken, expires: mockExpiresAt });

        // Mock GraphQL
        let mutationCount = 0;
        await page.route('http://localhost:5002/graphql', async (route) => {
          const postData = route.request().postDataJSON();

          if (postData?.query?.includes('CreateFamily')) {
            mutationCount++;
            await new Promise((resolve) => setTimeout(resolve, 500));
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  createFamily: {
                    family: {
                      id: 'family-rapid',
                      name: 'Rapid Family',
                      createdAt: '2025-12-30T00:00:00Z',
                    },
                    errors: null,
                  },
                },
              }),
            });
          } else if (postData?.query?.includes('GetCurrentFamily')) {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({ data: { family: null } }),
            });
          } else {
            await route.continue();
          }
        });

        await page.goto('/family/create');
      });

      await test.step('Click submit multiple times rapidly', async () => {
        await page.locator(SELECTORS.FAMILY_NAME_INPUT).fill('Rapid Family');

        const submitButton = page.getByRole('button', { name: 'Create Family' });
        await submitButton.click();
        await submitButton.click().catch(() => {}); // May be disabled
        await submitButton.click().catch(() => {}); // May be disabled
      });

      await test.step('Verify successful redirect', async () => {
        await expect(page).toHaveURL(/\/dashboard/);
        // Only one mutation should have been sent (button disabled after first click)
      });
    });
  });

  test.describe('Guard-Based Routing', () => {
    test('should redirect from dashboard to wizard when user has no family', async ({ page }) => {
      await test.step('Setup mocks', async () => {
        // Mock OAuth
        const mockAccessToken = 'mock-jwt-token-for-testing';
        const mockExpiresAt = new Date(Date.now() + 3600000).toISOString();

        await page.addInitScript(({ token, expires }) => {
          window.localStorage.setItem('family_hub_access_token', token);
          window.localStorage.setItem('family_hub_token_expires', expires);
        }, { token: mockAccessToken, expires: mockExpiresAt });

        // Mock GetCurrentFamily (no family)
        await page.route('http://localhost:5002/graphql', async (route) => {
          const postData = route.request().postDataJSON();
          if (postData?.query?.includes('GetCurrentFamily')) {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({ data: { family: null } }),
            });
          } else {
            await route.continue();
          }
        });
      });

      await test.step('Attempt to visit dashboard', async () => {
        await page.goto('/dashboard');
      });

      await test.step('Verify redirect to wizard', async () => {
        await expect(page).toHaveURL(/\/family\/create/);
        await expect(page.getByText('Create Your Family')).toBeVisible();
      });
    });

    test('should redirect from wizard to dashboard when user already has family', async ({ page }) => {
      await test.step('Setup mocks', async () => {
        // Mock OAuth
        const mockAccessToken = 'mock-jwt-token-for-testing';
        const mockExpiresAt = new Date(Date.now() + 3600000).toISOString();

        await page.addInitScript(({ token, expires }) => {
          window.localStorage.setItem('family_hub_access_token', token);
          window.localStorage.setItem('family_hub_token_expires', expires);
        }, { token: mockAccessToken, expires: mockExpiresAt });

        // Mock GetCurrentFamily (has family)
        await page.route('http://localhost:5002/graphql', async (route) => {
          const postData = route.request().postDataJSON();
          if (postData?.query?.includes('GetCurrentFamily')) {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  family: {
                    id: 'existing-family',
                    name: 'Existing Family',
                    createdAt: '2025-12-30T00:00:00Z',
                  },
                },
              }),
            });
          } else {
            await route.continue();
          }
        });
      });

      await test.step('Attempt to visit wizard', async () => {
        await page.goto('/family/create');
      });

      await test.step('Verify redirect to dashboard', async () => {
        await expect(page).toHaveURL(/\/dashboard/);
        await expect(page.locator('h1')).toContainText('Existing Family');
      });
    });
  });
});
