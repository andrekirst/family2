import { Injectable } from '@angular/core';
import { ApolloClient, InMemoryCache, HttpLink, ApolloLink } from '@apollo/client/core';

@Injectable({
  providedIn: 'root',
})
export class GraphQLService {
  private client: ApolloClient;

  constructor() {
    // Create HTTP link to backend GraphQL endpoint
    const httpLink = new HttpLink({
      uri: 'http://localhost:7001/graphql',
    });

    // Auth link to add Bearer token to requests
    const authLink = new ApolloLink((operation, forward) => {
      const token = localStorage.getItem('family_hub_access_token');

      if (token) {
        operation.setContext({
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });
      }

      return forward(operation);
    });

    // Create Apollo Client (HTTP only, no WebSocket for MVP)
    this.client = new ApolloClient({
      link: authLink.concat(httpLink),
      cache: new InMemoryCache(),
      defaultOptions: {
        watchQuery: {
          fetchPolicy: 'network-only', // Always fetch from network
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
    });
  }

  getClient() {
    return this.client;
  }

  async query<T = any>(query: any, variables?: any): Promise<T> {
    const result = await this.client.query({
      query,
      variables,
    });
    return result.data as T;
  }

  async mutate<T = any>(mutation: any, variables?: any): Promise<T> {
    const result = await this.client.mutate({
      mutation,
      variables,
    });
    return result.data as T;
  }
}
