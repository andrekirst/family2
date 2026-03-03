import { ApplicationConfig } from '@angular/core';
import { provideApollo } from 'apollo-angular';
import { HttpLink } from 'apollo-angular/http';
import { InMemoryCache, ApolloLink, Observable, split } from '@apollo/client/core';
import { getMainDefinition } from '@apollo/client/utilities';
import { setContext } from '@apollo/client/link/context';
import { onError } from '@apollo/client/link/error';
import { GraphQLWsLink } from '@apollo/client/link/subscriptions';
import { createClient } from 'graphql-ws';
import { Kind, OperationTypeNode } from 'graphql';
import { inject } from '@angular/core';
import { EnvironmentConfigService } from '../config/environment-config.service';
import { AuthService } from '../auth/auth.service';
import { I18nService } from '../i18n/i18n.service';

/**
 * Apollo Client configuration provider
 * Connects to Family Hub GraphQL API with JWT authentication
 */
export function provideApolloClient(): ApplicationConfig['providers'] {
  return [
    provideApollo(() => {
      const httpLink = inject(HttpLink);
      const envConfig = inject(EnvironmentConfigService);
      const authService = inject(AuthService);
      const i18nService = inject(I18nService);

      // Auth link: Add Bearer token to GraphQL requests
      // IMPORTANT: Apollo doesn't use Angular's HttpClient interceptors, so we must manually attach the token
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const authLink = setContext((_operation: any, prevContext: any) => {
        const token = localStorage.getItem('access_token');

        // Always send Accept-Language; only add Authorization if token exists
        if (!token) {
          return {
            headers: {
              ...prevContext.headers,
              'Accept-Language': i18nService.getLocaleForHeader(),
            },
          };
        }

        return {
          headers: {
            ...prevContext.headers,
            authorization: `Bearer ${token}`,
            'Accept-Language': i18nService.getLocaleForHeader(),
          },
        };
      });

      // Error link: Handle GraphQL errors globally with token refresh retry
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const errorLink = onError((errorOptions: any) => {
        const { graphQLErrors, networkError, forward, operation } = errorOptions;

        if (graphQLErrors) {
          // Check if any error is an authentication failure (expired token)
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          const authError = graphQLErrors.find(
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            (error: any) => error.extensions?.['code'] === 'AUTH_NOT_AUTHENTICATED',
          );

          if (authError) {
            // Attempt to refresh the token and retry the failed operation
            return new Observable((observer) => {
              authService
                .refreshAccessToken()
                .then((success) => {
                  if (!success) {
                    // Refresh failed — redirect to login
                    window.location.href = '/login';
                    observer.error(authError);
                    return;
                  }

                  // Update the operation with the new token
                  const newToken = authService.getAccessToken();
                  operation.setContext({
                    headers: {
                      ...operation.getContext().headers,
                      authorization: `Bearer ${newToken}`,
                    },
                  });

                  // Retry the operation
                  forward(operation).subscribe(observer);
                })
                .catch((err) => {
                  window.location.href = '/login';
                  observer.error(err);
                });
            });
          }

          // Log non-auth errors and handle authorization (permission) errors
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          graphQLErrors.forEach((error: any) => {
            console.error(`[GraphQL error]: ${error.message}`);

            if (error.extensions?.['code'] === 'AUTH_NOT_AUTHORIZED') {
              console.warn('Forbidden - redirecting to login');
              window.location.href = '/login';
            }
          });
        }

        if (networkError) {
          console.error(`[Network error]: ${networkError}`);
        }

        return;
      });

      // WebSocket link for subscriptions (graphql-ws protocol)
      const wsUrl = envConfig.apiUrl.replace(/^http/, 'ws');
      const wsLink = new GraphQLWsLink(
        createClient({
          url: wsUrl,
          connectionParams: () => {
            const token = localStorage.getItem('access_token');
            return token ? { Authorization: `Bearer ${token}` } : {};
          },
        }),
      );

      // HTTP link for queries and mutations
      const httpLinkInstance = ApolloLink.from([
        authLink as unknown as ApolloLink,
        errorLink as unknown as ApolloLink,
        httpLink.create({ uri: envConfig.apiUrl }),
      ]);

      // Split: subscriptions → WebSocket, everything else → HTTP
      const link = split(
        ({ query }) => {
          const definition = getMainDefinition(query);
          return (
            definition.kind === Kind.OPERATION_DEFINITION &&
            definition.operation === OperationTypeNode.SUBSCRIPTION
          );
        },
        wsLink,
        httpLinkInstance,
      );

      return {
        link,
        cache: new InMemoryCache(),
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
    }),
  ];
}
