import { test as base, Page, Route } from '@playwright/test';
import { URLS } from '../support/constants';

/**
 * GraphQL Fixture for API Interception
 *
 * Replaces Cypress cy.interceptGraphQL() command with Playwright route handlers.
 * Provides more control and reliability than Cypress intercepts.
 *
 * Usage:
 * ```typescript
 * import { test, expect } from '../fixtures/graphql.fixture';
 *
 * test('should mock GraphQL', async ({ page, interceptGraphQL }) => {
 *   await interceptGraphQL('GetCurrentFamily', {
 *     data: { family: { id: '123', name: 'Smith Family' } }
 *   });
 *
 *   await page.goto('/dashboard');
 *   await expect(page.getByText('Smith Family')).toBeVisible();
 * });
 * ```
 */

/**
 * Type definitions for GraphQL fixtures
 */
export type GraphQLFixture = {
  /**
   * Intercept and mock GraphQL operations by operation name
   *
   * @param operationName - The GraphQL operation name to intercept
   * @param response - The mock response to return
   */
  interceptGraphQL: (operationName: string, response: any) => Promise<void>;
};

/**
 * Extend Playwright's test with GraphQL fixtures
 */
export const test = base.extend<GraphQLFixture>({
  /**
   * GraphQL Interception Fixture
   *
   * Sets up route handler to intercept GraphQL requests and return mock data.
   * Matches by operationName or query string includes operation name.
   */
  interceptGraphQL: async ({ page }, use) => {
    const intercept = async (operationName: string, response: any) => {
      await test.step(`Intercept GraphQL: ${operationName}`, async () => {
        await page.route(URLS.GRAPHQL, async (route: Route) => {
          const request = route.request();
          const postData = request.postDataJSON();

          // Debug logging (helpful for troubleshooting)
          console.log('=== GraphQL Request ===');
          console.log('Looking for operation:', operationName);
          console.log('Request operationName:', postData?.operationName);
          console.log('Request query:', postData?.query?.substring(0, 100) + '...');

          // Match by operation name in request body or query string
          const matchesQuery = postData?.query?.includes(operationName);
          const matchesOperationName = postData?.operationName === operationName;

          if (matchesQuery || matchesOperationName) {
            console.log(`✅ MATCHED ${operationName} - Replying with mock data`);
            console.log('Mock response:', JSON.stringify(response, null, 2));

            // Fulfill request with mock response
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify(response),
            });
          } else {
            console.log(`❌ NO MATCH - Continuing to real API`);
            // Continue to real API if no match
            await route.continue();
          }

          console.log('======================');
        });

        console.log(`📡 GraphQL interception configured for: ${operationName}`);
      });
    };

    await use(intercept);

    // Cleanup: Unroute all routes after test
    // Playwright handles this automatically, but we can be explicit
    await page.unrouteAll({ behavior: 'ignoreErrors' });
  },
});

/**
 * Re-export expect from Playwright
 */
export { expect } from '@playwright/test';
