/**
 * Simplified wizard test - bypass all guards and mocks
 */

describe('Wizard Simple Test', () => {
  it('should render wizard component directly', () => {
    cy.visit('/family/create', {
      onBeforeLoad(win) {
        // Set auth tokens
        win.localStorage.setItem('family_hub_access_token', 'mock-token');
        win.localStorage.setItem('family_hub_token_expires', new Date(Date.now() + 3600000).toISOString());
      }
    });

    // Check if app root exists
    cy.get('app-root').should('exist');

    // Check if we navigated to the route
    cy.url().should('include', '/family/create');

    // Check for ANY text on the page
    cy.get('body').should('not.be.empty');

    // Try to find the wizard component
    cy.get('app-family-wizard-page').should('exist');

    // Try to find the wizard itself
    cy.get('app-wizard').should('exist');

    // Look for any visible text
    cy.contains('Back').should('be.visible');
  });
});
