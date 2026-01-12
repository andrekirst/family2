/**
 * Schema Fixture for GraphQL Schema Validation Tests
 *
 * Provides Playwright fixture for schema introspection and validation.
 * Follows Single Responsibility Principle - dedicated to schema operations.
 *
 * Usage:
 * ```typescript
 * import { test, expect } from '../fixtures/schema.fixture';
 *
 * test('should validate schema', async ({ schemaClient }) => {
 *   const types = await schemaClient.getTypeNames();
 *   expect(types).toContain('User');
 * });
 * ```
 *
 * @see Issue #74 - E2E Tests - GraphQL Schema Validation
 */

import { test as base } from '@playwright/test';
import { SchemaClient } from '../support/schema-helpers';
import { URLS } from '../support/constants';

/**
 * Schema fixture type definitions
 */
export interface SchemaFixture {
  /**
   * Schema client for introspection and validation operations
   *
   * Provides methods to:
   * - Fetch and cache schema via introspection
   * - Get normalized schema for snapshots
   * - Extract type/field names
   * - Detect breaking changes
   */
  schemaClient: SchemaClient;
}

/**
 * Extended Playwright test with schema fixture
 */
export const test = base.extend<SchemaFixture>({
  /**
   * Schema client fixture
   *
   * Creates a SchemaClient instance configured with the GraphQL endpoint.
   * The client caches schema results for efficiency during test runs.
   *
   * Features:
   * - Automatic cache cleanup after each test
   * - Clear error messages if GraphQL endpoint is unreachable
   */
  schemaClient: async ({ request }, use) => {
    const client = new SchemaClient(URLS.GRAPHQL, request);

    await use(client);

    // Cleanup: Clear cache after test to ensure isolation
    client.clearCache();
  },
});

/**
 * Re-export expect from Playwright for convenient imports
 */
export { expect } from '@playwright/test';
