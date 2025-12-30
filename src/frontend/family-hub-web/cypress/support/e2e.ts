// ***********************************************************
// This file is processed and loaded automatically before test files.
// Use this file to add global configuration and behavior.
// ***********************************************************

import './commands';
import 'cypress-axe';
import 'cypress-real-events';

// Prevent Cypress from failing on uncaught exceptions
Cypress.on('uncaught:exception', (err, runnable) => {
  // Return false to prevent Cypress from failing the test
  // Useful for application errors that don't affect E2E test flow
  return false;
});
