import { APIRequestContext } from '@playwright/test';
import { URLS, TEST_AUTH_HEADERS, TEST_USERS, TestUser } from './constants';
import { FamilyName } from './vogen-mirrors';

/**
 * API Helper Utilities for GraphQL Testing
 *
 * Provides API-first testing capabilities using Playwright's APIRequestContext.
 * Enables testing event chains without UI interaction for faster, more reliable tests.
 *
 * @see Issue #91 - E2E Authentication for API-First Testing
 */

/**
 * GraphQL Client for API-first testing
 *
 * Wraps Playwright's APIRequestContext with GraphQL-specific helpers.
 * Supports header-based authentication for test mode.
 */
export class GraphQLClient {
  private testUser: TestUser | null = null;

  constructor(private apiContext: APIRequestContext) {}

  /**
   * Set the test user for authenticated requests.
   * When set, all requests will include X-Test-User-Id and X-Test-User-Email headers.
   *
   * @param user - The test user to authenticate as, or null to clear authentication
   * @returns this - for method chaining
   *
   * @example
   * ```typescript
   * const client = new GraphQLClient(apiContext);
   * client.setTestUser(TEST_USERS.PRIMARY);
   * await client.mutate(CREATE_FAMILY_MUTATION, { input: { name: 'Test' } });
   * ```
   */
  setTestUser(user: TestUser | null): this {
    this.testUser = user;
    return this;
  }

  /**
   * Get the currently set test user.
   */
  getTestUser(): TestUser | null {
    return this.testUser;
  }

  /**
   * Build headers for the GraphQL request.
   * Includes test auth headers if a test user is set.
   */
  private getHeaders(): Record<string, string> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
    };

    if (this.testUser) {
      headers[TEST_AUTH_HEADERS.USER_ID] = this.testUser.id;
      headers[TEST_AUTH_HEADERS.USER_EMAIL] = this.testUser.email;
    }

    return headers;
  }

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
      headers: this.getHeaders(),
      data: { query, variables },
    });

    if (!response.ok()) {
      const body = await response.text();
      throw new Error(
        `GraphQL query failed: ${response.status()} ${response.statusText()}\n${body}`
      );
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
 * Create a GraphQL client with test authentication
 *
 * Creates a GraphQL client that authenticates using test mode headers.
 * When the backend has FAMILYHUB_TEST_MODE=true, it will accept these
 * headers instead of requiring valid JWT tokens.
 *
 * @param apiContext - Playwright APIRequestContext
 * @param testUser - The test user to authenticate as (defaults to PRIMARY)
 * @returns GraphQL client configured with test authentication headers
 *
 * @see Issue #91 - E2E Authentication for API-First Testing
 *
 * @example
 * ```typescript
 * // Use default PRIMARY user
 * const client = createAuthenticatedGraphQLClient(apiContext);
 *
 * // Use specific test user
 * const memberClient = createAuthenticatedGraphQLClient(apiContext, TEST_USERS.MEMBER);
 * ```
 */
export function createAuthenticatedGraphQLClient(
  apiContext: APIRequestContext,
  testUser: TestUser = TEST_USERS.PRIMARY
): GraphQLClient {
  return new GraphQLClient(apiContext).setTestUser(testUser);
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

  /**
   * Invite family members (batch operation)
   * Used for E2E email verification tests
   */
  INVITE_FAMILY_MEMBERS: `
    mutation InviteFamilyMembers($input: InviteFamilyMembersInput!) {
      inviteFamilyMembers(input: $input) {
        successfulInvitations {
          invitationId
          email
          role
          token
          displayCode
          expiresAt
          status
        }
        failedInvitations {
          email
          role
          errorCode
          errorMessage
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
        }
      }
    }
  `,
} as const;

/**
 * Invitation request input for batch invitations
 */
export interface InvitationInput {
  email: string;
  role: 'ADMIN' | 'MEMBER' | 'CHILD';
}

/**
 * Result of a successful invitation
 */
export interface InvitationSuccess {
  invitationId: string;
  email: string;
  role: string;
  token: string;
  displayCode: string;
  expiresAt: string;
  status: string;
}

/**
 * Result of a failed invitation
 */
export interface InvitationFailure {
  email: string;
  role: string;
  errorCode: string;
  errorMessage: string;
}

/**
 * Result of invite family members mutation
 */
export interface InviteFamilyMembersResult {
  successfulInvitations: InvitationSuccess[];
  failedInvitations: InvitationFailure[];
}

/**
 * Invite family members via API
 *
 * Sends batch invitations to the specified email addresses.
 * This triggers the email sending pipeline through the EmailOutbox.
 *
 * @param client - Authenticated GraphQL client
 * @param familyId - ID of the family to invite members to
 * @param invitations - List of email/role pairs to invite
 * @param message - Optional personal message to include
 * @returns Result with successful and failed invitations
 *
 * @example
 * ```typescript
 * const result = await inviteFamilyMembersViaAPI(client, familyId, [
 *   { email: 'alice@example.com', role: 'ADMIN' },
 *   { email: 'bob@example.com', role: 'MEMBER' },
 * ], 'Welcome to our family!');
 * console.log('Sent:', result.successfulInvitations.length);
 * ```
 */
export async function inviteFamilyMembersViaAPI(
  client: GraphQLClient,
  familyId: string,
  invitations: InvitationInput[],
  message?: string
): Promise<InviteFamilyMembersResult> {
  const result = await client.mutate(GraphQLQueries.INVITE_FAMILY_MEMBERS, {
    input: {
      familyId,
      invitations: invitations.map((i) => ({
        email: i.email,
        role: i.role,
      })),
      message,
    },
  });

  if (result.inviteFamilyMembers.errors?.length > 0) {
    throw new Error(`Invite failed: ${JSON.stringify(result.inviteFamilyMembers.errors, null, 2)}`);
  }

  return {
    successfulInvitations: result.inviteFamilyMembers.successfulInvitations || [],
    failedInvitations: result.inviteFamilyMembers.failedInvitations || [],
  };
}
