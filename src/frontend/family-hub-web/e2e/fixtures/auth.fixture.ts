import { test as base, Page, APIRequestContext } from '@playwright/test';
import { STORAGE_KEYS, TEST_DATA, TEST_USERS, TestUser } from '../support/constants';
import { GraphQLClient, createAuthenticatedGraphQLClient } from '../support/api-helpers';

/**
 * Auth Fixture for E2E Testing
 *
 * Provides both:
 * 1. Frontend authentication (localStorage tokens for Angular)
 * 2. Backend authentication (X-Test-User-Id/Email headers for API calls)
 *
 * Usage:
 * ```typescript
 * import { test, expect } from '../fixtures/auth.fixture';
 *
 * // UI testing with mocked frontend auth
 * test('should access protected page', async ({ authenticatedPage }) => {
 *   await authenticatedPage.goto('/dashboard');
 *   await expect(authenticatedPage).toHaveURL(/dashboard/);
 * });
 *
 * // API-first testing with real backend
 * test('should create family via API', async ({ graphqlClient }) => {
 *   const result = await graphqlClient.mutate(CREATE_FAMILY, { input: { name: 'Test' } });
 *   expect(result.createFamily.errors).toHaveLength(0);
 * });
 * ```
 *
 * @see Issue #91 - E2E Authentication for API-First Testing
 */

/**
 * Type definitions for auth fixtures
 */
export interface AuthFixture {
  /**
   * The test user to authenticate as.
   * Defaults to TEST_USERS.PRIMARY.
   * Can be overridden in test.use({}).
   */
  testUser: TestUser;

  /**
   * GraphQL client pre-configured with test authentication headers.
   * Use this for API-first testing with the real backend.
   */
  graphqlClient: GraphQLClient;

  /**
   * Function to mock OAuth login by setting localStorage tokens.
   * Uses the same keys as Angular AuthService.
   */
  mockFrontendAuth: () => Promise<void>;

  /**
   * Pre-configured page with OAuth tokens already set.
   * Ready to navigate to protected routes.
   */
  authenticatedPage: Page;

  /**
   * Function to switch the authenticated user.
   * Updates both the graphqlClient and localStorage.
   */
  switchUser: (user: TestUser) => void;
}

/**
 * Extend Playwright's test with auth fixtures
 */
export const test = base.extend<AuthFixture>({
  /**
   * Default test user.
   * Can be overridden per-test or per-file using test.use().
   *
   * @example
   * ```typescript
   * // Override for specific test file
   * test.use({ testUser: TEST_USERS.MEMBER });
   * ```
   */
  testUser: TEST_USERS.PRIMARY,

  /**
   * GraphQL Client Fixture
   *
   * Provides a GraphQL client configured with test authentication headers.
   * When the backend has FAMILYHUB_TEST_MODE=true, these headers are used
   * instead of JWT validation.
   */
  graphqlClient: async ({ request, testUser }, use) => {
    const client = createAuthenticatedGraphQLClient(request, testUser);
    await use(client);
  },

  /**
   * Mock Frontend Auth Fixture
   *
   * Sets localStorage tokens before page loads using addInitScript.
   * This ensures tokens are available when Angular AuthService initializes.
   *
   * Note: This only affects the frontend (Angular) - for API calls, use graphqlClient.
   */
  mockFrontendAuth: async ({ page, testUser }, use) => {
    const mockAuth = async () => {
      await test.step('Mock frontend OAuth state', async () => {
        // Create a mock JWT payload that Angular's AuthService will accept
        const mockPayload = {
          sub: testUser.id,
          email: testUser.email,
          given_name: testUser.firstName,
          family_name: testUser.lastName,
          exp: Math.floor(Date.now() / 1000) + 3600, // 1 hour from now
        };

        // Create a fake JWT (header.payload.signature)
        // Angular only decodes the payload, doesn't verify signature
        const fakeJwt = `eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.${btoa(JSON.stringify(mockPayload))}.fake_signature`;

        const mockExpiresAt = new Date(
          Date.now() + TEST_DATA.TOKEN_EXPIRY_HOURS * 3600000
        ).toISOString();

        // Add init script to set localStorage before page loads
        await page.addInitScript(
          ({ tokenKey, tokenExpiresKey, token, expires }) => {
            window.localStorage.setItem(tokenKey, token);
            window.localStorage.setItem(tokenExpiresKey, expires);
          },
          {
            tokenKey: STORAGE_KEYS.ACCESS_TOKEN,
            tokenExpiresKey: STORAGE_KEYS.TOKEN_EXPIRES,
            token: fakeJwt,
            expires: mockExpiresAt,
          }
        );

        console.log('✅ Frontend auth mocked in localStorage');
        console.log(`   User: ${testUser.email}`);
        console.log(`   Expires: ${mockExpiresAt}`);
      });
    };

    await use(mockAuth);
  },

  /**
   * Authenticated Page Fixture
   *
   * Provides a page with OAuth tokens already configured for the frontend.
   * Use this for UI tests that need the Angular app to think user is logged in.
   */
  authenticatedPage: async ({ page, mockFrontendAuth }, use) => {
    await test.step('Setup authenticated page', async () => {
      await mockFrontendAuth();
    });

    await use(page);
  },

  /**
   * Switch User Fixture
   *
   * Allows switching the authenticated user during a test.
   * Updates the graphqlClient's test user.
   */
  switchUser: async ({ graphqlClient }, use) => {
    const switchFn = (user: TestUser) => {
      graphqlClient.setTestUser(user);
    };
    await use(switchFn);
  },
});

/**
 * Re-export expect from Playwright
 */
export { expect } from '@playwright/test';

/**
 * Legacy alias for backwards compatibility
 * @deprecated Use mockFrontendAuth instead
 */
export const mockOAuthLogin = 'Use mockFrontendAuth fixture instead';
