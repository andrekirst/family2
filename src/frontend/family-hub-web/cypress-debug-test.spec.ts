describe('Debug Test', () => {
  it('should show what is actually happening', () => {
    cy.clearLocalStorage();
    cy.clearCookies();
    
    // Mock auth
    const mockAccessToken = 'mock-jwt-token';
    const mockExpiresAt = new Date(Date.now() + 3600000).toISOString();
    cy.window().then((win) => {
      win.localStorage.setItem('family_hub_access_token', mockAccessToken);
      win.localStorage.setItem('family_hub_token_expires', mockExpiresAt);
    });
    
    // Intercept ALL GraphQL calls to see what's happening
    cy.intercept('POST', 'http://localhost:5002/graphql', (req) => {
      console.log('GraphQL Request:', req.body);
      
      if (req.body.query?.includes('GetCurrentFamily')) {
        req.reply({
          data: { family: null }
        });
      } else {
        req.reply({
          statusCode: 200,
          body: { data: {} }
        });
      }
    }).as('graphql');
    
    cy.visit('/family/create');
    cy.wait(2000);
    
    // Take screenshot to see what rendered
    cy.screenshot('debug-wizard-page');
  });
});
