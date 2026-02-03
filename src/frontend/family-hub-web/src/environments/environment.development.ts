export const environment = {
  production: false,
  apiUrl: 'http://localhost:5152/graphql',
  keycloak: {
    issuer: 'http://localhost:8080/realms/FamilyHub',
    clientId: 'familyhub-web',
    redirectUri: 'http://localhost:4200/callback',
    postLogoutRedirectUri: 'http://localhost:4200',
    scope: 'openid profile email',
  },
};
