/**
 * Debug test for wizard rendering
 */

describe('Wizard Debug', () => {
  it('should render wizard page with proper GraphQL mock', () => {
    // Setup auth
    cy.window().then((win) => {
      win.localStorage.setItem('family_hub_access_token', 'mock-token');
      win.localStorage.setItem('family_hub_token_expires', new Date(Date.now() + 3600000).toISOString());
    });

    // Intercept GetCurrentFamily with proper response
    cy.intercept('POST', 'http://localhost:5002/graphql', (req) => {
      console.log('GraphQL Request:', req.body);

      if (req.body.query?.includes('GetCurrentFamily') || req.body.operationName === 'GetCurrentFamily') {
        console.log('Matched GetCurrentFamily query');
        req.reply({
          data: {
            family: null  // User has no family
          }
        });
      } else {
        req.continue();
      }
    }).as('gqlGetCurrentFamily');

    // Visit wizard page directly
    cy.visit('/family/create');

    // Wait for GraphQL call
    cy.wait('@gqlGetCurrentFamily', { timeout: 10000 });

    // Give Angular time to process the response and update the view
    cy.wait(1000);

    // Debug: Log page HTML and check what's actually rendered
    cy.document().then((doc) => {
      console.log('=== PAGE STATE ===');
      console.log('Body HTML length:', doc.body.innerHTML.length);
      console.log('app-wizard elements:', doc.querySelectorAll('app-wizard').length);
      console.log('Contains "Create Your Family":', doc.body.innerHTML.includes('Create Your Family'));
      console.log('Contains "Loading wizard":', doc.body.innerHTML.includes('Loading wizard'));
      console.log('Contains "Error Loading Wizard":', doc.body.innerHTML.includes('Error Loading Wizard'));
    });

    // Check if error state is showing
    cy.get('body').then(($body) => {
      if ($body.text().includes('Error Loading Wizard')) {
        cy.log('ERROR STATE DETECTED');
        cy.contains('Error Loading Wizard').should('be.visible');
      } else if ($body.text().includes('Loading wizard')) {
        cy.log('LOADING STATE DETECTED');
        cy.contains('Loading wizard').should('be.visible');
      } else {
        cy.log('WIZARD STATE - Looking for title');
      }
    });

    // Check if wizard title appears
    cy.contains('Create Your Family', { timeout: 10000 }).should('be.visible');

    // Check for wizard component
    cy.get('app-wizard', { timeout: 10000 }).should('exist');

    // Check for input
    cy.get('input[aria-label="Family name"]', { timeout: 10000 }).should('exist');
  });
});
