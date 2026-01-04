import { test as base, Page } from '@playwright/test';
import { STORAGE_KEYS, TEST_DATA } from '../support/constants';

/**
 * Auth Fixture for OAuth Mocking
 *
 * Replaces Cypress cy.mockOAuthLogin() command with Playwright fixtures.
 * Provides authenticated page context with OAuth tokens pre-configured.
 *
 * Usage:
 * ```typescript
 * import { test, expect } from '../fixtures/auth.fixture';
 *
 * test('should access protected page', async ({ authenticatedPage }) => {
 *   await authenticatedPage.goto('/dashboard');
 *   await expect(authenticatedPage).toHaveURL(/dashboard/);
 * });
 * ```
 */

/**
 * Type definitions for auth fixtures
 */
export type AuthFixture = {
  /**
   * Function to mock OAuth login by setting localStorage tokens
   * Uses the same keys as Angular AuthService
   */
  mockOAuthLogin: () => Promise<void>;

  /**
   * Pre-configured page with OAuth tokens already set
   * Ready to navigate to protected routes
   */
  authenticatedPage: Page;
};

/**
 * Extend Playwright's test with auth fixtures
 */
export const test = base.extend<AuthFixture>({
  /**
   * Mock OAuth Login Fixture
   *
   * Sets localStorage tokens before page loads using addInitScript.
   * This ensures tokens are available when Angular AuthService initializes.
   */
  mockOAuthLogin: async ({ page }, use) => {
    const mockLogin = async () => {
      await test.step('Mock OAuth login tokens', async () => {
        // Calculate token expiration (1 hour from now)
        const mockAccessToken = TEST_DATA.MOCK_ACCESS_TOKEN;
        const mockExpiresAt = new Date(
          Date.now() + TEST_DATA.TOKEN_EXPIRY_HOURS * 3600000
        ).toISOString();

        // Add init script to set localStorage before page loads
        // This is more reliable than setting after page load
        await page.addInitScript(
          ({ tokenKey, tokenExpiresKey, token, expires }) => {
            window.localStorage.setItem(tokenKey, token);
            window.localStorage.setItem(tokenExpiresKey, expires);
          },
          {
            tokenKey: STORAGE_KEYS.ACCESS_TOKEN,
            tokenExpiresKey: STORAGE_KEYS.TOKEN_EXPIRES,
            token: mockAccessToken,
            expires: mockExpiresAt,
          }
        );

        console.log('✅ OAuth tokens mocked in localStorage');
        console.log(`   Token: ${mockAccessToken.substring(0, 20)}...`);
        console.log(`   Expires: ${mockExpiresAt}`);
      });
    };

    await use(mockLogin);
  },

  /**
   * Authenticated Page Fixture
   *
   * Provides a page with OAuth tokens already configured.
   * Automatically calls mockOAuthLogin before returning the page.
   */
  authenticatedPage: async ({ page, mockOAuthLogin }, use) => {
    await test.step('Setup authenticated page', async () => {
      // Set up OAuth tokens before any navigation
      await mockOAuthLogin();
    });

    // Provide the authenticated page to the test
    await use(page);

    // Cleanup is automatic - Playwright handles page cleanup
  },
});

/**
 * Re-export expect from Playwright
 * This allows importing both test and expect from this file
 */
export { expect } from '@playwright/test';
