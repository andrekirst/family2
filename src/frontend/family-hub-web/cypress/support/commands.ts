// ***********************************************
// Custom Cypress commands for Family Hub E2E tests
// ***********************************************

/// <reference types="cypress" />

declare global {
  namespace Cypress {
    interface Chainable {
      /**
       * Custom command to mock OAuth login flow
       * @example cy.mockOAuthLogin()
       */
      mockOAuthLogin(): Chainable<void>;

      /**
       * Custom command to intercept GraphQL queries/mutations
       * @example cy.interceptGraphQL('GetUserFamilies', { data: mockResponse })
       */
      interceptGraphQL(operationName: string, response: any): Chainable<void>;
    }
  }
}

/**
 * Mock OAuth login by setting tokens in localStorage
 */
Cypress.Commands.add('mockOAuthLogin', () => {
  const mockAccessToken = 'mock-jwt-token-for-testing';
  const mockUser = {
    sub: '6dc37d75-f300-4576-aef0-dfdd4f71edbb',
    email: 'test@example.com',
    email_verified: true
  };

  cy.window().then((win) => {
    win.localStorage.setItem('access_token', mockAccessToken);
    win.localStorage.setItem('user', JSON.stringify(mockUser));
  });
});

/**
 * Intercept GraphQL operations by operation name
 */
Cypress.Commands.add('interceptGraphQL', (operationName: string, response: any) => {
  cy.intercept('POST', '**/graphql', (req) => {
    if (req.body.query?.includes(operationName)) {
      req.reply(response);
    }
  }).as(`gql${operationName}`);
});

export {};
