/**
 * GraphQL Subscription Helper for E2E Testing
 *
 * Provides Apollo Client with WebSocket support for testing real-time GraphQL subscriptions.
 * Uses graphql-ws protocol for WebSocket transport and HTTP for regular queries/mutations.
 *
 * Architecture:
 * - Apollo Client with split link (WebSocket for subscriptions, HTTP for queries/mutations)
 * - graphql-ws client for WebSocket transport (supports auto-reconnect, heartbeat)
 * - In-memory cache for client-side state management
 * - Authentication via Bearer token in connection params
 *
 * Use Cases:
 * - Multi-client real-time collaboration testing
 * - Verify subscription messages received when mutations occur
 * - Test authorization on subscription resolvers
 * - Validate event payload structure and timing
 *
 * Example:
 * ```typescript
 * const client = createSubscriptionClient(
 *   'ws://localhost:7000/graphql',
 *   'http://localhost:7000/graphql',
 *   'mock-access-token'
 * );
 *
 * const updates: any[] = [];
 * const subscription = client.subscribe({
 *   query: FAMILY_MEMBERS_CHANGED_SUBSCRIPTION,
 *   variables: { familyId: 'family-123' }
 * }).subscribe(result => updates.push(result));
 *
 * // Trigger mutation via UI or API
 * await page.click('button[name="invite-member"]');
 *
 * // Wait for subscription update
 * await page.waitForTimeout(2000);
 * expect(updates.length).toBeGreaterThan(0);
 *
 * subscription.unsubscribe();
 * ```
 *
 * Protocol Compatibility:
 * - Backend: HotChocolate 15.1.11 with Redis subscriptions
 * - Frontend: graphql-ws (subscriptions-transport-ws deprecated)
 * - Transport: WebSocket (ws:// for dev, wss:// for prod)
 *
 * Prerequisites:
 * - npm install @apollo/client graphql-ws cross-fetch
 * - Backend WebSocket endpoint: /graphql (same as HTTP)
 * - Redis PubSub configured for multi-instance support
 */

import { ApolloClient, InMemoryCache, split, HttpLink, gql } from '@apollo/client/core';
import { GraphQLWsLink } from '@apollo/client/link/subscriptions';
import { getMainDefinition } from '@apollo/client/utilities';
import { createClient, Client as GraphQLWsClient } from 'graphql-ws';
import fetch from 'cross-fetch';

/**
 * Create Apollo Client configured for GraphQL subscriptions
 *
 * @param wsUrl - WebSocket URL (e.g., 'ws://localhost:7000/graphql')
 * @param httpUrl - HTTP URL (e.g., 'http://localhost:7000/graphql')
 * @param authToken - Optional Bearer token for authentication
 * @returns Configured Apollo Client with subscription support
 */
export function createSubscriptionClient(
  wsUrl: string,
  httpUrl: string,
  authToken?: string
): ApolloClient<any> {
  // WebSocket link for subscriptions
  const wsLink = new GraphQLWsLink(
    createClient({
      url: wsUrl,
      connectionParams: authToken
        ? {
            authorization: `Bearer ${authToken}`,
          }
        : {},
      // Optional: Add retry logic for flaky connections
      retryAttempts: 3,
      shouldRetry: () => true,
      // Heartbeat to keep connection alive
      keepAlive: 10000, // 10 seconds
    })
  );

  // HTTP link for queries and mutations
  const httpLink = new HttpLink({
    uri: httpUrl,
    fetch,
    headers: authToken ? { authorization: `Bearer ${authToken}` } : {},
  });

  // Split link: route operations based on type
  // - subscription -> WebSocket
  // - query/mutation -> HTTP
  const splitLink = split(
    ({ query }) => {
      const definition = getMainDefinition(query);
      return definition.kind === 'OperationDefinition' && definition.operation === 'subscription';
    },
    wsLink,
    httpLink
  );

  return new ApolloClient({
    link: splitLink,
    cache: new InMemoryCache(),
    // Disable default error handling (let tests handle errors)
    defaultOptions: {
      watchQuery: {
        errorPolicy: 'all',
      },
      query: {
        errorPolicy: 'all',
      },
      mutate: {
        errorPolicy: 'all',
      },
    },
  });
}

/**
 * Create raw graphql-ws client for advanced use cases
 *
 * @param wsUrl - WebSocket URL
 * @param authToken - Optional Bearer token
 * @returns GraphQL WebSocket client
 */
export function createRawWsClient(wsUrl: string, authToken?: string): GraphQLWsClient {
  return createClient({
    url: wsUrl,
    connectionParams: authToken
      ? {
          authorization: `Bearer ${authToken}`,
        }
      : {},
    retryAttempts: 3,
    shouldRetry: () => true,
    keepAlive: 10000,
  });
}

/**
 * GraphQL Subscription: Family Members Changed
 *
 * Subscribes to real-time updates when family members are added/removed/updated.
 * Triggered by:
 * - InvitationAcceptedEvent -> FamilyMemberAddedEvent
 * - RemoveFamilyMember mutation
 * - UpdateFamilyMemberRole mutation
 *
 * Authorization: Requires family membership (any role)
 */
export const FAMILY_MEMBERS_CHANGED_SUBSCRIPTION = gql`
  subscription FamilyMembersChanged($familyId: UUID!) {
    familyMembersChanged(familyId: $familyId) {
      changeType
      member {
        id
        email
        role
        emailVerified
        joinedAt
        isOwner
        auditInfo {
          createdAt
          updatedAt
        }
      }
    }
  }
`;

/**
 * GraphQL Subscription: Pending Invitations Changed
 *
 * Subscribes to real-time updates when pending invitations change.
 * Triggered by:
 * - InviteFamilyMembers mutation -> InvitationAddedEvent
 * - AcceptInvitation mutation -> InvitationRemovedEvent
 * - CancelInvitation mutation -> InvitationRemovedEvent
 *
 * Authorization: Requires OWNER or ADMIN role
 */
export const PENDING_INVITATIONS_CHANGED_SUBSCRIPTION = gql`
  subscription PendingInvitationsChanged($familyId: UUID!) {
    pendingInvitationsChanged(familyId: $familyId) {
      changeType
      invitation {
        id
        email
        role
        status
        invitedById
        invitedAt
        expiresAt
        message
        displayCode
      }
    }
  }
`;

/**
 * Type definitions for subscription payloads
 */

export enum ChangeType {
  ADDED = 'ADDED',
  UPDATED = 'UPDATED',
  REMOVED = 'REMOVED',
}

export enum UserRole {
  OWNER = 'OWNER',
  ADMIN = 'ADMIN',
  MEMBER = 'MEMBER',
  CHILD = 'CHILD',
}

export enum InvitationStatus {
  PENDING = 'PENDING',
  ACCEPTED = 'ACCEPTED',
  EXPIRED = 'EXPIRED',
  REVOKED = 'REVOKED',
}

export interface FamilyMember {
  id: string;
  email: string;
  role: UserRole;
  emailVerified: boolean;
  joinedAt: string;
  isOwner: boolean;
  auditInfo: {
    createdAt: string;
    updatedAt: string;
  };
}

export interface FamilyMembersChangedPayload {
  changeType: ChangeType;
  member: FamilyMember;
}

export interface PendingInvitation {
  id: string;
  email: string;
  role: UserRole;
  status: InvitationStatus;
  invitedById: string;
  invitedAt: string;
  expiresAt: string;
  message?: string;
  displayCode: string;
}

export interface PendingInvitationsChangedPayload {
  changeType: ChangeType;
  invitation: PendingInvitation;
}

/**
 * Helper: Wait for subscription update with timeout
 *
 * Polls updates array until predicate matches or timeout.
 * Useful for asserting subscription updates in tests.
 *
 * @param updates - Array to monitor for new updates
 * @param predicate - Function to match desired update
 * @param timeout - Max wait time in milliseconds (default: 5000)
 * @returns Matched update or null if timeout
 */
export async function waitForSubscriptionUpdate<T>(
  updates: T[],
  predicate: (update: T) => boolean,
  timeout = 5000
): Promise<T | null> {
  const startTime = Date.now();

  while (Date.now() - startTime < timeout) {
    const found = updates.find(predicate);
    if (found) {
      return found;
    }
    // Poll every 100ms
    await new Promise((resolve) => setTimeout(resolve, 100));
  }

  return null;
}

/**
 * Helper: Subscribe and collect updates
 *
 * Convenience wrapper for subscribing and collecting all updates.
 * Automatically unsubscribes when done.
 *
 * @param client - Apollo Client
 * @param subscription - GraphQL subscription document
 * @param variables - Subscription variables
 * @returns Object with updates array and unsubscribe function
 */
export function subscribeAndCollect<T>(
  client: ApolloClient<any>,
  subscription: any,
  variables: any
): {
  updates: T[];
  unsubscribe: () => void;
} {
  const updates: T[] = [];

  const observable = client.subscribe({
    query: subscription,
    variables,
  });

  const subscription_handle = observable.subscribe({
    next: (result) => {
      if (result.data) {
        updates.push(result.data as T);
      }
    },
    error: (error) => {
      console.error('Subscription error:', error);
    },
  });

  return {
    updates,
    unsubscribe: () => subscription_handle.unsubscribe(),
  };
}
