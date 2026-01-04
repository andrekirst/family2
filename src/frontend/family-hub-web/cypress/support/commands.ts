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
 * IMPORTANT: Uses keys matching AuthService TOKEN_KEY and TOKEN_EXPIRES_KEY
 */
Cypress.Commands.add('mockOAuthLogin', () => {
  const mockAccessToken = 'mock-jwt-token-for-testing';
  const mockExpiresAt = new Date(Date.now() + 3600000).toISOString(); // 1 hour from now

  cy.window().then((win) => {
    win.localStorage.setItem('family_hub_access_token', mockAccessToken);
    win.localStorage.setItem('family_hub_token_expires', mockExpiresAt);
  });
});

/**
 * Intercept GraphQL operations by operation name
 * Matches both query and operationName fields in GraphQL requests
 */
Cypress.Commands.add('interceptGraphQL', (operationName: string, response: any) => {
  cy.intercept('POST', 'http://localhost:5002/graphql', (req) => {
    // Debug logging
    console.log('=== GraphQL Request ===');
    console.log('Looking for operation:', operationName);
    console.log('Request operationName:', req.body.operationName);
    console.log('Request query:', req.body.query);
    console.log('Request variables:', req.body.variables);

    // Match by operation name in request body or query string
    const matchesQuery = req.body.query?.includes(operationName);
    const matchesOperationName = req.body.operationName === operationName;

    if (matchesQuery || matchesOperationName) {
      console.log(`✅ MATCHED ${operationName} - Replying with mock data`);
      console.log('Mock response:', response);
      req.reply(response);
    } else {
      console.log(`❌ NO MATCH - Continuing to real API`);
      // Continue without intercepting if operation doesn't match
      req.continue();
    }
    console.log('======================');
  }).as(`gql${operationName}`);
});

export {};
