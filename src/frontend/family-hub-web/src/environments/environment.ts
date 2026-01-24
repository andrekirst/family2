export const environment = {
  production: false,
  graphqlEndpoint: 'http://localhost:5002/graphql',
  graphqlWsEndpoint: 'ws://localhost:5002/graphql', // WebSocket endpoint for GraphQL subscriptions
  zitadelAuthority: 'http://localhost:8080',
  redirectUri: 'http://localhost:4200/auth/callback',
};
