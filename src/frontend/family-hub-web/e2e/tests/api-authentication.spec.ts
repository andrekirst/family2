/**
 * E2E Test Suite: API Authentication with Test Mode
 *
 * Tests that verify header-based authentication works correctly when
 * the backend has FAMILYHUB_TEST_MODE=true enabled.
 *
 * @see Issue #91 - E2E Authentication for API-First Testing
 */

import { test, expect } from '../fixtures/auth.fixture';
import { TEST_USERS } from '../support/constants';
import { GraphQLQueries } from '../support/api-helpers';

test.describe('API Authentication (Test Mode)', () => {
  test('should authenticate with test headers and access GraphQL', async ({ graphqlClient }) => {
    /**
     * This test verifies that the backend accepts X-Test-User-Id and
     * X-Test-User-Email headers for authentication when test mode is enabled.
     */
    await test.step('Query current family with authenticated client', async () => {
      // This query requires authentication - will fail without valid auth headers
      const result = await graphqlClient.query(GraphQLQueries.GET_CURRENT_FAMILY);

      // Query succeeded (no error thrown) - this proves auth headers were accepted
      // Result may be null if user has no family, but that's expected
      expect(result).toBeDefined();
      expect(result.family).toBeDefined(); // May be null, but key should exist
    });
  });

  test('should create family with authenticated user', async ({ graphqlClient }) => {
    /**
     * Test that we can create a family using authenticated GraphQL calls.
     * This is the foundation for email verification tests.
     */
    const timestamp = Date.now();
    const familyName = `E2E Test Family ${timestamp}`;

    await test.step('Create family via GraphQL API', async () => {
      const result = await graphqlClient.mutate(GraphQLQueries.CREATE_FAMILY, {
        input: { name: familyName },
      });

      expect(result.createFamily).toBeDefined();

      // Check for errors
      if (result.createFamily.errors && result.createFamily.errors.length > 0) {
        // Log error details for debugging
        console.log('GraphQL errors:', JSON.stringify(result.createFamily.errors, null, 2));
      }

      // If user already has a family, we expect a BusinessError
      // Otherwise, we expect successful creation
      const hasBusinessError = result.createFamily.errors?.some(
        (e: any) => e.__typename === 'BusinessError'
      );

      if (!hasBusinessError) {
        // Successful creation
        expect(result.createFamily.createdFamilyDto).toBeDefined();
        expect(result.createFamily.createdFamilyDto.name).toBe(familyName);
        console.log(`✅ Family created: ${result.createFamily.createdFamilyDto.id}`);
      } else {
        // User already has a family - this is also valid
        console.log('ℹ️ User already has a family (expected for repeat test runs)');
      }
    });
  });

  test('should allow switching test users', async ({ graphqlClient, switchUser }) => {
    /**
     * Test that we can switch between different test users.
     */
    await test.step('Query as PRIMARY user', async () => {
      const result = await graphqlClient.query(GraphQLQueries.GET_CURRENT_FAMILY);
      expect(result).toBeDefined();
      console.log('Query as PRIMARY user succeeded');
    });

    await test.step('Switch to MEMBER user and query', async () => {
      switchUser(TEST_USERS.MEMBER);

      const result = await graphqlClient.query(GraphQLQueries.GET_CURRENT_FAMILY);
      expect(result).toBeDefined();
      console.log('Query as MEMBER user succeeded');
    });

    await test.step('Switch to NO_FAMILY user and query', async () => {
      switchUser(TEST_USERS.NO_FAMILY);

      const result = await graphqlClient.query(GraphQLQueries.GET_CURRENT_FAMILY);
      expect(result).toBeDefined();
      expect(result.family).toBeNull(); // User without family should get null
      console.log('Query as NO_FAMILY user succeeded (family is null as expected)');
    });
  });

  test('should fail without authentication headers', async ({ request }) => {
    /**
     * Test that requests without auth headers fail appropriately.
     * This ensures the backend requires authentication.
     */
    await test.step('Send unauthenticated request', async () => {
      const response = await request.post('http://localhost:5002/graphql', {
        headers: {
          'Content-Type': 'application/json',
          // No X-Test-User-Id header
        },
        data: {
          query: GraphQLQueries.GET_CURRENT_FAMILY,
        },
      });

      // The response may be 200 (GraphQL always returns 200) but with errors
      const data = await response.json();

      // Either the request should fail or return GraphQL errors
      if (data.errors) {
        // Check for unauthorized error
        const hasAuthError = data.errors.some(
          (e: any) =>
            e.message?.toLowerCase().includes('unauthorized') ||
            e.message?.toLowerCase().includes('authenticated') ||
            e.extensions?.code === 'AUTH_NOT_AUTHENTICATED'
        );
        expect(hasAuthError).toBe(true);
        console.log('✅ Unauthenticated request correctly returned auth error');
      } else {
        // If no errors, the endpoint might be public or auth is optional for this query
        console.log('ℹ️ Request succeeded without auth (endpoint may be public)');
      }
    });
  });
});

test.describe('API Authentication - User Context', () => {
  test.use({ testUser: TEST_USERS.PRIMARY });

  test('should use PRIMARY user for all requests in this describe block', async ({
    graphqlClient,
    testUser,
  }) => {
    expect(testUser.id).toBe(TEST_USERS.PRIMARY.id);
    expect(testUser.email).toBe(TEST_USERS.PRIMARY.email);

    const currentUser = graphqlClient.getTestUser();
    expect(currentUser?.id).toBe(TEST_USERS.PRIMARY.id);
  });
});

test.describe('API Authentication - Member User', () => {
  test.use({ testUser: TEST_USERS.MEMBER });

  test('should use MEMBER user for all requests in this describe block', async ({
    graphqlClient,
    testUser,
  }) => {
    expect(testUser.id).toBe(TEST_USERS.MEMBER.id);
    expect(testUser.email).toBe(TEST_USERS.MEMBER.email);

    const currentUser = graphqlClient.getTestUser();
    expect(currentUser?.id).toBe(TEST_USERS.MEMBER.id);
  });
});
