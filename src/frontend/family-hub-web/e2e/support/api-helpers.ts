import { APIRequestContext } from '@playwright/test';
import { URLS } from './constants';
import { FamilyName } from './vogen-mirrors';

/**
 * API Helper Utilities for GraphQL Testing
 *
 * Provides API-first testing capabilities using Playwright's APIRequestContext.
 * Enables testing event chains without UI interaction for faster, more reliable tests.
 */

/**
 * GraphQL Client for API-first testing
 *
 * Wraps Playwright's APIRequestContext with GraphQL-specific helpers.
 */
export class GraphQLClient {
  constructor(private apiContext: APIRequestContext) {}

  /**
   * Execute a GraphQL query
   *
   * @param query - GraphQL query string
   * @param variables - Query variables
   * @returns Parsed response data
   * @throws Error if request fails or GraphQL returns errors
   */
  async query<T = any>(query: string, variables?: any): Promise<T> {
    const response = await this.apiContext.post(URLS.GRAPHQL, {
      data: { query, variables },
    });

    if (!response.ok()) {
      throw new Error(`GraphQL query failed: ${response.statusText()}`);
    }

    const data = await response.json();

    if (data.errors) {
      throw new Error(`GraphQL errors: ${JSON.stringify(data.errors, null, 2)}`);
    }

    return data.data;
  }

  /**
   * Execute a GraphQL mutation
   *
   * @param mutation - GraphQL mutation string
   * @param variables - Mutation variables
   * @returns Parsed response data
   */
  async mutate<T = any>(mutation: string, variables?: any): Promise<T> {
    return this.query<T>(mutation, variables);
  }
}

/**
 * Create a family via API
 *
 * Example usage for test setup:
 * ```typescript
 * const client = new GraphQLClient(apiContext);
 * const family = await createFamilyViaAPI(client, 'Smith Family');
 * console.log('Created family:', family.id);
 * ```
 */
export async function createFamilyViaAPI(
  client: GraphQLClient,
  name: string
): Promise<{ id: string; name: string; createdAt: string }> {
  // Validate name using Vogen mirror
  const familyName = FamilyName.from(name);

  const mutation = `
    mutation CreateFamily($input: CreateFamilyInput!) {
      createFamily(input: $input) {
        createdFamilyDto {
          id
          name
          createdAt
        }
        errors {
          __typename
          ... on ValidationError {
            message
            field
          }
          ... on BusinessError {
            message
            code
          }
          ... on ValueObjectError {
            message
          }
        }
      }
    }
  `;

  const result = await client.mutate(mutation, {
    input: { name: familyName.toString() },
  });

  if (result.createFamily.errors && result.createFamily.errors.length > 0) {
    throw new Error(result.createFamily.errors[0].message);
  }

  return result.createFamily.createdFamilyDto;
}

/**
 * Get current user's family via API
 */
export async function getCurrentFamilyViaAPI(
  client: GraphQLClient
): Promise<{ id: string; name: string } | null> {
  const query = `
    query GetCurrentFamily {
      family {
        id
        name
      }
    }
  `;

  const result = await client.query(query);
  return result.family;
}

/**
 * Create a GraphQL client with authentication
 *
 * @param apiContext - Playwright APIRequestContext
 * @param accessToken - OAuth access token (optional, uses mock token by default)
 */
export function createAuthenticatedGraphQLClient(
  apiContext: APIRequestContext,
  accessToken?: string
): GraphQLClient {
  // Note: In real implementation, we'll need to configure
  // APIRequestContext with proper headers. For now, this is a placeholder.
  return new GraphQLClient(apiContext);
}

/**
 * GraphQL Query Templates
 *
 * Centralized query/mutation strings to avoid duplication
 */
export const GraphQLQueries = {
  /**
   * Get current user's family
   */
  GET_CURRENT_FAMILY: `
    query GetCurrentFamily {
      family {
        id
        name
        createdAt
      }
    }
  `,

  /**
   * Create a new family
   */
  CREATE_FAMILY: `
    mutation CreateFamily($input: CreateFamilyInput!) {
      createFamily(input: $input) {
        createdFamilyDto {
          id
          name
          createdAt
        }
        errors {
          __typename
          ... on ValidationError {
            message
            field
          }
          ... on BusinessError {
            message
            code
          }
          ... on ValueObjectError {
            message
          }
        }
      }
    }
  `,

  /**
   * Get user families
   */
  GET_USER_FAMILIES: `
    query GetUserFamilies {
      families {
        id
        name
        createdAt
      }
    }
  `,
} as const;
