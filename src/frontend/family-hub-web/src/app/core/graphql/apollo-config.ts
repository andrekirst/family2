import { inject } from '@angular/core';
import { HttpLink } from 'apollo-angular/http';
import { ApolloClientOptions, InMemoryCache, split } from '@apollo/client/core';
import { getMainDefinition } from '@apollo/client/utilities';
import { GraphQLWsLink } from '@apollo/client/link/subscriptions';
import { createClient } from 'graphql-ws';
import { environment } from '../../../environments/environment';

/**
 * Creates Apollo Client configuration with split link pattern
 *
 * **Architecture:**
 * - HTTP Link: Used for queries and mutations
 * - WebSocket Link: Used for subscriptions (real-time updates)
 * - Split Link: Routes operations based on type
 *
 * **Features:**
 * - Auto-reconnect on connection loss (5 retries)
 * - Auth token injection from localStorage
 * - InMemoryCache with merge policies
 * - Separate endpoints for HTTP and WebSocket
 *
 * **Usage:**
 * Register in app.config.ts:
 * ```typescript
 * import { provideApollo } from 'apollo-angular';
 * import { createApollo } from './core/graphql/apollo-config';
 *
 * export const appConfig: ApplicationConfig = {
 *   providers: [
 *     provideApollo(createApollo),
 *     // ...
 *   ]
 * };
 * ```
 *
 * @returns Apollo Client configuration options
 */
export function createApollo(): ApolloClientOptions<unknown> {
  const httpLink = inject(HttpLink);

  // HTTP link for queries and mutations
  const http = httpLink.create({
    uri: environment.graphqlEndpoint,
  });

  // WebSocket link for subscriptions
  const ws = new GraphQLWsLink(
    createClient({
      url: environment.graphqlWsEndpoint,
      connectionParams: () => {
        // Get auth token from localStorage
        const token = localStorage.getItem('access_token');

        // Return connection params with Authorization header
        return token
          ? {
              Authorization: `Bearer ${token}`,
            }
          : {};
      },
      // Auto-reconnect configuration
      retryAttempts: 5,
      shouldRetry: () => true,
      // Retry with exponential backoff (1s, 2s, 4s, 8s, 16s)
      retryWait: async (retries) => {
        await new Promise((resolve) => setTimeout(resolve, Math.min(1000 * 2 ** retries, 30000)));
      },
      // Lazy connection (only connect when subscription starts)
      lazy: true,
      // Heartbeat to keep connection alive
      keepAlive: 10_000, // 10 seconds
      // Connection lifecycle hooks
      on: {
        connected: () => {
          console.log('[Apollo] WebSocket connected');
        },
        closed: (event) => {
          console.log('[Apollo] WebSocket closed', event);
        },
        error: (error) => {
          console.error('[Apollo] WebSocket error', error);
        },
      },
    })
  );

  // Split link: HTTP for queries/mutations, WebSocket for subscriptions
  const link = split(
    ({ query }) => {
      const definition = getMainDefinition(query);
      return definition.kind === 'OperationDefinition' && definition.operation === 'subscription';
    },
    ws, // Subscriptions go through WebSocket
    http // Queries and mutations go through HTTP
  );

  // InMemoryCache configuration
  const cache = new InMemoryCache({
    typePolicies: {
      Query: {
        fields: {
          // Merge strategy for invitation lists
          pendingInvitations: {
            merge(existing = [], incoming) {
              return incoming;
            },
          },
          // Merge strategy for family member lists
          familyMembers: {
            merge(existing = [], incoming) {
              return incoming;
            },
          },
        },
      },
    },
  });

  return {
    link,
    cache,
    // Enable in development for debugging
    connectToDevTools: !environment.production,
    // Default fetch policy
    defaultOptions: {
      watchQuery: {
        fetchPolicy: 'cache-and-network',
        errorPolicy: 'all',
      },
      query: {
        fetchPolicy: 'network-only',
        errorPolicy: 'all',
      },
      mutate: {
        errorPolicy: 'all',
      },
    },
  };
}
