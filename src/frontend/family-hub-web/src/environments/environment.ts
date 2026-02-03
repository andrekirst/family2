export const environment = {
  production: true,
  apiUrl: 'https://api.familyhub.com/graphql',
  keycloak: {
    issuer: 'https://auth.familyhub.com/realms/FamilyHub',
    clientId: 'familyhub-web',
    redirectUri: 'https://app.familyhub.com/callback',
    postLogoutRedirectUri: 'https://app.familyhub.com',
    scope: 'openid profile email',
  },
};
