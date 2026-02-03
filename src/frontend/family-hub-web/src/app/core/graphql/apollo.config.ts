import { ApplicationConfig } from '@angular/core';
import { provideApollo } from 'apollo-angular';
import { HttpLink } from 'apollo-angular/http';
import { InMemoryCache, ApolloLink } from '@apollo/client/core';
import { setContext } from '@apollo/client/link/context';
import { onError } from '@apollo/client/link/error';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';

/**
 * Apollo Client configuration provider
 * Connects to Family Hub GraphQL API with JWT authentication
 */
export function provideApolloClient(): ApplicationConfig['providers'] {
  return [
    provideApollo(() => {
      const httpLink = inject(HttpLink);

      // Auth link: Add Bearer token to GraphQL requests
      // IMPORTANT: Apollo doesn't use Angular's HttpClient, so we must manually attach the token
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const authLink = setContext((_operation: any, prevContext: any) => {
        const token = localStorage.getItem('access_token');
        return {
          headers: {
            ...prevContext.headers,
            authorization: token ? `Bearer ${token}` : '',
          },
        };
      });

      // Error link: Handle GraphQL errors globally
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const errorLink = onError((errorOptions: any) => {
        const { graphQLErrors, networkError } = errorOptions;

        if (graphQLErrors) {
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          graphQLErrors.forEach((error: any) => {
            console.error(`[GraphQL error]: ${error.message}`);

            // Redirect to login if unauthorized
            if (
              error.extensions?.['code'] === 'AUTH_NOT_AUTHENTICATED' ||
              error.extensions?.['code'] === 'AUTH_NOT_AUTHORIZED'
            ) {
              console.warn('Unauthorized - redirecting to login');
              window.location.href = '/login';
            }
          });
        }

        if (networkError) {
          console.error(`[Network error]: ${networkError}`);
        }
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
