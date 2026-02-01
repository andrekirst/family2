import { ApplicationConfig } from '@angular/core';
import { provideApollo } from 'apollo-angular';
import { HttpLink } from 'apollo-angular/http';
import { InMemoryCache } from '@apollo/client/core';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';

/**
 * Apollo Client configuration provider
 * Connects to Family Hub GraphQL API
 */
export function provideApolloClient(): ApplicationConfig['providers'] {
  return [
    provideApollo(() => {
      const httpLink = inject(HttpLink);

      return {
        link: httpLink.create({
          uri: environment.apiUrl,
        }),
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
