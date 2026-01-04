/**
 * Test to understand timing issues with wizard rendering
 */

describe('Wizard Timing Test', () => {
  it('should check wizard visibility before and after GraphQL', () => {
    // Setup auth
    cy.window().then((win) => {
      win.localStorage.setItem('family_hub_access_token', 'mock-token');
      win.localStorage.setItem('family_hub_token_expires', new Date(Date.now() + 3600000).toISOString());
    });

    // Intercept GraphQL
    cy.intercept('POST', 'http://localhost:5002/graphql', (req) => {
      if (req.body.query?.includes('GetCurrentFamily') || req.body.operationName === 'GetCurrentFamily') {
        req.reply({
          data: {
            family: null
          }
        });
      } else {
        req.continue();
      }
    }).as('gqlGetCurrentFamily');

    // Visit page
    cy.visit('/family/create');

    // Check IMMEDIATELY - before waiting for GraphQL
    cy.log('CHECKING IMMEDIATELY AFTER VISIT');
    cy.get('app-wizard', { timeout: 1000 }).should('exist');

    // Check what's actually in the wizard
    cy.get('app-wizard').then(($wizard) => {
      const html = $wizard[0].innerHTML;
      cy.log(`Wizard HTML length: ${html.length}`);
      cy.log(`Has header tag: ${html.includes('<header')}`);
      cy.log(`Has main tag: ${html.includes('<main')}`);
      cy.log(`Has footer tag: ${html.includes('<footer')}`);
      cy.log(`Title text: ${html.includes('Create Your Family')}`);
    });

    cy.get('body').then(($body) => {
      const hasTitle = $body.text().includes('Create Your Family');
      const hasBack = $body.text().includes('Back');
      cy.log(`Has title: ${hasTitle}, Has Back button: ${hasBack}`);
    });

    // Now wait for GraphQL
    cy.wait('@gqlGetCurrentFamily');

    // Check AFTER GraphQL completes
    cy.log('CHECKING AFTER GRAPHQL WAIT');
    cy.get('app-wizard').should('exist');
    cy.get('body').then(($body) => {
      const hasTitle = $body.text().includes('Create Your Family');
      const hasBack = $body.text().includes('Back');
      cy.log(`Has title: ${hasTitle}, Has Back button: ${hasBack}`);
    });

    // Try to find the title
    cy.contains('Create Your Family', { timeout: 5000 }).should('be.visible');
  });
});
