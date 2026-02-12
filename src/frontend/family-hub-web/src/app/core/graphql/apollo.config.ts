import { ApplicationConfig } from '@angular/core';
import { provideApollo } from 'apollo-angular';
import { HttpLink } from 'apollo-angular/http';
import { InMemoryCache, ApolloLink, Observable } from '@apollo/client/core';
import { setContext } from '@apollo/client/link/context';
import { onError } from '@apollo/client/link/error';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';

/**
 * Apollo Client configuration provider
 * Connects to Family Hub GraphQL API with JWT authentication
 */
export function provideApolloClient(): ApplicationConfig['providers'] {
  return [
    provideApollo(() => {
      const httpLink = inject(HttpLink);
      const authService = inject(AuthService);

      // Auth link: Add Bearer token to GraphQL requests
      // IMPORTANT: Apollo doesn't use Angular's HttpClient interceptors, so we must manually attach the token
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const authLink = setContext((_operation: any, prevContext: any) => {
        const token = localStorage.getItem('access_token');

        // Only add Authorization header if token exists
        // Sending an empty Authorization header causes AUTH_NOT_AUTHENTICATED errors
        if (!token) {
          return prevContext;
        }

        return {
          headers: {
            ...prevContext.headers,
            authorization: `Bearer ${token}`,
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

      // Combine links: auth → error → http
      const link = ApolloLink.from([
        authLink as unknown as ApolloLink,
        errorLink as unknown as ApolloLink,
        httpLink.create({ uri: environment.apiUrl }),
      ]);

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
