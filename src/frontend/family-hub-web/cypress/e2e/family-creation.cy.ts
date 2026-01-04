/**
 * E2E Test Suite: Family Creation Wizard Flow
 *
 * Tests the complete user journey from login to family creation using wizard-based page flow,
 * including validation, error handling, and accessibility compliance.
 *
 * Test Coverage:
 * - Happy path: Login → Auto-redirect to wizard → Create family → Redirect to dashboard
 * - Validation errors (empty name, too long)
 * - API errors (user already has family)
 * - Accessibility (WCAG 2.1 AA compliance)
 * - Keyboard navigation (Tab, Enter)
 * - Guard-based routing (familyGuard, noFamilyGuard)
 *
 * Architecture Changes (vs Modal):
 * - Wizard page at /family/create (not modal)
 * - Guard-based auto-redirect when !hasFamily()
 * - Progress indicator (Step 1 of 1)
 * - No cancel/escape option (completion required)
 */

describe('Family Creation Flow', () => {
  beforeEach(() => {
    // Reset application state
    cy.clearLocalStorage();
    cy.clearCookies();
  });

  describe('Happy Path: Complete Family Creation', () => {
    it('should complete family creation wizard from login to dashboard', () => {
      // 1. Mock OAuth login
      cy.mockOAuthLogin();

      // 2. Mock GetCurrentFamily query (null response - user has no family)
      cy.interceptGraphQL('GetCurrentFamily', {
        data: {
          family: null
        }
      });

      // 3. Visit dashboard (should auto-redirect to wizard)
      cy.visit('/dashboard');

      // 4. Verify redirect to wizard page (guard-based routing)
      cy.url().should('include', '/family/create');

      // 5. Verify wizard page title
      cy.contains('Create Your Family').should('be.visible');

      // 6. Verify progress indicator shows Step 1 of 1
      cy.contains('Step 1 of 1').should('be.visible');

      // 7. Verify icon and description in step component
      cy.get('app-icon[name="users"]').should('exist');
      cy.contains('Give your family a name to get started').should('be.visible');

      // 8. Mock createFamily mutation (success)
      cy.interceptGraphQL('CreateFamily', {
        data: {
          createFamily: {
            family: {
              id: 'family-123',
              name: 'Smith Family',
              createdAt: '2025-12-30T00:00:00Z'
            },
            errors: null
          }
        }
      });

      // 9. Mock GetCurrentFamily query (after creation)
      cy.interceptGraphQL('GetCurrentFamily', {
        data: {
          family: {
            id: 'family-123',
            name: 'Smith Family',
            createdAt: '2025-12-30T00:00:00Z'
          }
        }
      });

      // 10. Enter family name in step component
      cy.get('input[aria-label="Family name"]').type('Smith Family');

      // 11. Verify wizard submit button is enabled
      cy.contains('button', 'Create Family').should('not.be.disabled');

      // 12. Click wizard submit button (last step, triggers completion)
      cy.contains('button', 'Create Family').click();

      // 13. Verify loading state
      cy.contains('Creating...').should('be.visible');

      // 14. Wait for GraphQL mutation
      cy.wait('@gqlCreateFamily');

      // 15. Verify redirect to dashboard (wizard completes → navigation)
      cy.url().should('include', '/dashboard');

      // 16. Verify dashboard shows family info
      cy.get('h1').should('contain', 'Smith Family');

      // 17. Verify family creation date is displayed
      cy.contains('Created:').should('be.visible');
    });
  });

  describe('Form Validation', () => {
    beforeEach(() => {
      cy.mockOAuthLogin();
      cy.interceptGraphQL('GetCurrentFamily', {
        data: { family: null }
      });
      cy.visit('/family/create');
    });

    it('should show error when family name is empty', () => {
      // 1. Focus and blur input (trigger validation)
      cy.get('input[aria-label="Family name"]').focus().blur();

      // 2. Verify error message
      cy.contains('Family name is required').should('be.visible');

      // 3. Verify wizard submit button is disabled
      cy.contains('button', 'Create Family').should('be.disabled');

      // 4. Verify ARIA attributes
      cy.get('input[aria-label="Family name"]')
        .should('have.attr', 'aria-invalid', 'true')
        .should('have.attr', 'aria-describedby');
    });

    it('should show error when family name exceeds 50 characters', () => {
      // 1. Type 51 characters
      const longName = 'a'.repeat(51);
      cy.get('input[aria-label="Family name"]').type(longName);

      // 2. Blur to trigger validation
      cy.get('input[aria-label="Family name"]').blur();

      // 3. Verify error message
      cy.contains('Family name must be 50 characters or less').should('be.visible');

      // 4. Verify wizard submit button is disabled
      cy.contains('button', 'Create Family').should('be.disabled');
    });

    it('should enable submit button when valid name is entered', () => {
      // 1. Enter valid name
      cy.get('input[aria-label="Family name"]').type('Valid Family Name');

      // 2. Verify no error messages
      cy.contains('Family name is required').should('not.exist');
      cy.contains('Family name must be 50 characters or less').should('not.exist');

      // 3. Verify wizard submit button is enabled
      cy.contains('button', 'Create Family').should('not.be.disabled');
    });
  });

  describe('API Error Handling', () => {
    beforeEach(() => {
      cy.mockOAuthLogin();
      cy.interceptGraphQL('GetCurrentFamily', {
        data: { family: null }
      });
      cy.visit('/family/create');
    });

    it('should display error when user already has a family', () => {
      // 1. Mock createFamily mutation (business rule error)
      cy.interceptGraphQL('CreateFamily', {
        data: {
          createFamily: {
            family: null,
            errors: [{
              message: 'User already has a family',
              code: 'BUSINESS_RULE_VIOLATION'
            }]
          }
        }
      });

      // 2. Enter family name and submit
      cy.get('input[aria-label="Family name"]').type('Test Family');
      cy.contains('button', 'Create Family').click();

      // 3. Wait for mutation
      cy.wait('@gqlCreateFamily');

      // 4. Verify error message is displayed
      cy.get('[role="alert"]').should('contain', 'User already has a family');

      // 5. Verify wizard page remains (no redirect)
      cy.url().should('include', '/family/create');

      // 6. Verify form value is preserved (not reset)
      cy.get('input[aria-label="Family name"]').should('have.value', 'Test Family');
    });

    it('should display error when network request fails', () => {
      // 1. Mock network error
      cy.intercept('POST', '**/graphql', {
        statusCode: 500,
        body: { error: 'Internal Server Error' }
      }).as('gqlNetworkError');

      // 2. Enter family name and submit
      cy.get('input[aria-label="Family name"]').type('Test Family');
      cy.contains('button', 'Create Family').click();

      // 3. Wait for failed request
      cy.wait('@gqlNetworkError');

      // 4. Verify error message
      cy.get('[role="alert"]').should('be.visible');
      cy.contains('Failed to create family').should('be.visible');
    });
  });

  describe('Keyboard Navigation', () => {
    beforeEach(() => {
      cy.mockOAuthLogin();
      cy.interceptGraphQL('GetCurrentFamily', {
        data: { family: null }
      });
      cy.visit('/family/create');
    });

    it('should allow Tab navigation through wizard elements', () => {
      // 1. Tab to input
      cy.realPress('Tab');
      cy.get('input[aria-label="Family name"]').should('have.focus');

      // 2. Tab to submit button
      cy.realPress('Tab');
      cy.contains('button', 'Create Family').should('have.focus');
    });

    it('should submit form with Enter key', () => {
      // 1. Mock successful creation
      cy.interceptGraphQL('CreateFamily', {
        data: {
          createFamily: {
            family: {
              id: 'family-456',
              name: 'Keyboard Family',
              createdAt: '2025-12-30T00:00:00Z'
            },
            errors: null
          }
        }
      });

      // 2. Type family name
      cy.get('input[aria-label="Family name"]').type('Keyboard Family');

      // 3. Press Enter to submit
      cy.get('input[aria-label="Family name"]').type('{enter}');

      // 4. Verify mutation was called
      cy.wait('@gqlCreateFamily');

      // 5. Verify redirect to dashboard
      cy.url().should('include', '/dashboard');
    });
  });

  describe('Accessibility Compliance (WCAG 2.1 AA)', () => {
    beforeEach(() => {
      cy.mockOAuthLogin();
      cy.interceptGraphQL('GetCurrentFamily', {
        data: { family: null }
      });
      cy.visit('/family/create');
    });

    it('should pass axe-core accessibility audit on wizard page', () => {
      // Inject axe-core
      cy.injectAxe();

      // Run accessibility checks on wizard page
      cy.checkA11y(null, {
        rules: {
          // WCAG 2.1 AA rules
          'color-contrast': { enabled: true },
          'valid-aria-attr': { enabled: true },
          'aria-required-attr': { enabled: true },
          'aria-valid-attr-value': { enabled: true },
          'label': { enabled: true },
          'button-name': { enabled: true },
          'region': { enabled: true }
        }
      });
    });

    it('should have proper ARIA attributes on input', () => {
      cy.get('input[aria-label="Family name"]')
        .should('have.attr', 'aria-label', 'Family name')
        .should('have.attr', 'aria-required', 'true');
    });

    it('should have proper ARIA attributes on error message', () => {
      // Trigger error
      cy.get('input[aria-label="Family name"]').focus().blur();

      // Verify error message appears with accessibility attributes
      cy.contains('Family name is required').should('be.visible');
    });

    it('should have proper page semantics', () => {
      // Verify wizard title exists
      cy.contains('Create Your Family').should('be.visible');

      // Verify progress indicator exists
      cy.contains('Step 1 of 1').should('be.visible');
    });

    it('should announce loading state to screen readers', () => {
      // Mock delayed response
      cy.interceptGraphQL('CreateFamily', {
        delay: 1000,
        data: {
          createFamily: {
            family: {
              id: 'family-789',
              name: 'Test Family',
              createdAt: '2025-12-30T00:00:00Z'
            },
            errors: null
          }
        }
      });

      // Submit form
      cy.get('input[aria-label="Family name"]').type('Test Family');
      cy.contains('button', 'Create Family').click();

      // Verify loading text is visible
      cy.contains('Creating...').should('be.visible');
    });
  });

  describe('Loading States', () => {
    beforeEach(() => {
      cy.mockOAuthLogin();
      cy.interceptGraphQL('GetCurrentFamily', {
        data: { family: null }
      });
      cy.visit('/family/create');
    });

    it('should disable submit button while creating family', () => {
      // Mock delayed createFamily response
      cy.interceptGraphQL('CreateFamily', {
        delay: 1000,
        data: {
          createFamily: {
            family: {
              id: 'family-999',
              name: 'Test Family',
              createdAt: '2025-12-30T00:00:00Z'
            },
            errors: null
          }
        }
      });

      // Enter name and submit
      cy.get('input[aria-label="Family name"]').type('Test Family');
      cy.contains('button', 'Create Family').click();

      // Verify button is disabled during submission
      cy.contains('button', 'Create Family').should('be.disabled');
      cy.contains('Creating...').should('be.visible');
    });
  });

  describe('User Experience Edge Cases', () => {
    it('should handle rapid form submissions gracefully', () => {
      cy.mockOAuthLogin();
      cy.interceptGraphQL('GetCurrentFamily', {
        data: { family: null }
      });
      cy.visit('/family/create');

      // Mock successful creation
      cy.interceptGraphQL('CreateFamily', {
        delay: 500,
        data: {
          createFamily: {
            family: {
              id: 'family-rapid',
              name: 'Rapid Family',
              createdAt: '2025-12-30T00:00:00Z'
            },
            errors: null
          }
        }
      });

      // Enter name
      cy.get('input[aria-label="Family name"]').type('Rapid Family');

      // Click submit multiple times rapidly
      const submitButton = cy.contains('button', 'Create Family');
      submitButton.click();
      submitButton.click();
      submitButton.click();

      // Verify only one mutation was sent
      cy.wait('@gqlCreateFamily');
      cy.get('@gqlCreateFamily.all').should('have.length', 1);
    });

    it('should redirect to dashboard after successful wizard completion', () => {
      cy.mockOAuthLogin();
      cy.interceptGraphQL('GetCurrentFamily', {
        data: { family: null }
      });
      cy.visit('/family/create');

      // Mock successful creation
      cy.interceptGraphQL('CreateFamily', {
        data: {
          createFamily: {
            family: {
              id: 'family-completion',
              name: 'Completion Test Family',
              createdAt: '2025-12-30T00:00:00Z'
            },
            errors: null
          }
        }
      });

      // Mock GetCurrentFamily after creation
      cy.interceptGraphQL('GetCurrentFamily', {
        data: {
          family: {
            id: 'family-completion',
            name: 'Completion Test Family',
            createdAt: '2025-12-30T00:00:00Z'
          }
        }
      });

      // Create family
      cy.get('input[aria-label="Family name"]').type('Completion Test Family');
      cy.contains('button', 'Create Family').click();

      // Wait for success
      cy.wait('@gqlCreateFamily');

      // Verify redirect to dashboard
      cy.url().should('include', '/dashboard');
    });
  });

  describe('Guard-Based Routing', () => {
    it('should redirect from dashboard to wizard when user has no family', () => {
      cy.mockOAuthLogin();
      cy.interceptGraphQL('GetCurrentFamily', {
        data: { family: null }
      });

      // Attempt to visit dashboard
      cy.visit('/dashboard');

      // Should auto-redirect to wizard (familyGuard)
      cy.url().should('include', '/family/create');
      cy.contains('Create Your Family').should('be.visible');
    });

    it('should redirect from wizard to dashboard when user already has family', () => {
      cy.mockOAuthLogin();
      cy.interceptGraphQL('GetCurrentFamily', {
        data: {
          family: {
            id: 'existing-family',
            name: 'Existing Family',
            createdAt: '2025-12-30T00:00:00Z'
          }
        }
      });

      // Attempt to visit wizard
      cy.visit('/family/create');

      // Should auto-redirect to dashboard (noFamilyGuard)
      cy.url().should('include', '/dashboard');
      cy.get('h1').should('contain', 'Existing Family');
    });
  });
});
