# Family Hub E2E Testing Guide

## Overview

This directory contains end-to-end (E2E) tests for the Family Hub web application using Cypress with accessibility testing integration.

## Test Coverage

### Family Creation Flow (`e2e/family-creation.cy.ts`)

**Total Test Cases: 20+**

1. **Happy Path** (1 test)
   - Complete family creation from login to dashboard
   - Verifies modal appearance, form submission, and dashboard update

2. **Form Validation** (4 tests)
   - Empty name validation
   - Max length validation (50 characters)
   - Character counter real-time updates
   - Submit button state management

3. **API Error Handling** (2 tests)
   - Business rule violation (user already has family)
   - Network errors (500 Internal Server Error)

4. **Keyboard Navigation** (3 tests)
   - Tab navigation through modal elements
   - Form submission with Enter key
   - Escape key handling (modal cannot be closed)

5. **Accessibility Compliance** (5 tests)
   - WCAG 2.1 AA compliance with axe-core
   - ARIA attributes on input fields
   - ARIA attributes on error messages
   - Modal semantics (role="dialog", aria-modal)
   - Screen reader announcements for loading states

6. **Loading States** (2 tests)
   - Loading overlay when fetching families
   - Submit button disabled during creation

7. **User Experience Edge Cases** (2 tests)
   - Rapid form submissions (prevents duplicate requests)
   - Form reset after successful creation

## Test Technology Stack

| Tool | Version | Purpose |
|------|---------|---------|
| **Cypress** | 15.8.1 | E2E testing framework |
| **cypress-axe** | 1.7.0 | Accessibility testing (axe-core integration) |
| **axe-core** | 4.11.0 | WCAG 2.1 AA validation engine |
| **cypress-real-events** | 1.15.0 | Realistic keyboard/mouse events |

## Running Tests

### Prerequisites

1. **Install dependencies:**
   ```bash
   npm install
   ```

2. **Start development server** (in separate terminal):
   ```bash
   npm start
   ```
   Server must be running at `http://localhost:4200`

### Interactive Mode (Cypress UI)

```bash
npm run e2e
```

Opens Cypress Test Runner GUI where you can:
- Select and run individual tests
- Watch tests in real-time
- Debug with time-travel snapshots
- Inspect DOM and network requests

### Headless Mode (CI/CD)

```bash
npm run e2e:headless
```

Runs all tests in headless Chrome:
- Faster execution
- Suitable for CI/CD pipelines
- Generates video recordings (if enabled)
- Screenshots on failure

### CI Pipeline Integration

```bash
npm run e2e:ci
```

Automatically:
1. Starts dev server at `http://localhost:4200`
2. Waits for server to be ready
3. Runs all E2E tests headless
4. Kills server after tests complete

## Custom Cypress Commands

### `cy.mockOAuthLogin()`

Mocks OAuth authentication by setting JWT token and user data in localStorage.

**Usage:**
```typescript
cy.mockOAuthLogin();
cy.visit('/dashboard');
```

**Mock Data:**
- Access Token: `mock-jwt-token-for-testing`
- User ID: `6dc37d75-f300-4576-aef0-dfdd4f71edbb`
- Email: `test@example.com`
- Email Verified: `true`

### `cy.interceptGraphQL(operationName, response)`

Intercepts GraphQL operations by operation name and returns mocked response.

**Usage:**
```typescript
cy.interceptGraphQL('GetUserFamilies', {
  data: {
    getUserFamilies: {
      families: [{
        familyId: { value: 'family-123' },
        name: 'Smith Family',
        memberCount: 1,
        createdAt: '2025-12-30T00:00:00Z'
      }]
    }
  }
});
```

**Supported Operations:**
- `GetUserFamilies` - Query user's families
- `CreateFamily` - Mutation to create family

## Accessibility Testing with axe-core

### What is axe-core?

[axe-core](https://github.com/dequelabs/axe-core) is the industry-standard accessibility testing engine that validates against WCAG 2.1 AA standards.

### Running Accessibility Audits

```typescript
// Inject axe-core into page
cy.injectAxe();

// Run accessibility checks
cy.checkA11y('[role="dialog"]', {
  rules: {
    'color-contrast': { enabled: true },
    'valid-aria-attr': { enabled: true },
    'aria-required-attr': { enabled: true },
    'label': { enabled: true }
  }
});
```

### WCAG 2.1 AA Coverage

| Category | Rules Tested |
|----------|--------------|
| **Color Contrast** | Text must have 4.5:1 contrast ratio |
| **ARIA Attributes** | Valid ARIA roles, states, properties |
| **Form Labels** | All inputs have accessible labels |
| **Keyboard Navigation** | All interactive elements focusable |
| **Screen Reader Support** | Semantic HTML and ARIA announcements |

### Accessibility Failures

When axe-core detects violations, tests fail with detailed reports:

```
Expected 0 accessibility violations but found 2:

1. color-contrast: Elements must have sufficient color contrast
   - Fix: Ensure text color has 4.5:1 contrast ratio with background

2. label: Form elements must have labels
   - Fix: Add aria-label or <label> element to input
```

## Test Best Practices

### 1. Use Data-Test Attributes (Future Enhancement)

```html
<!-- Add data-cy attributes for stable selectors -->
<button data-cy="submit-button" type="submit">Create Family</button>
```

```typescript
// More resilient than class or text selectors
cy.get('[data-cy=submit-button]').click();
```

### 2. Mock GraphQL Responses

Always intercept GraphQL requests to avoid:
- Flaky tests from network issues
- Dependency on backend being available
- Slow test execution

### 3. Test User Flows, Not Implementation

❌ **Bad:** Test internal component state
```typescript
expect(component.familyForm.valid).toBe(true);
```

✅ **Good:** Test user-visible behavior
```typescript
cy.get('button[type="submit"]').should('not.be.disabled');
```

### 4. Use Descriptive Test Names

```typescript
// Clear test intent
it('should show error when family name exceeds 50 characters', () => {
  // ...
});

// Not clear
it('should validate input', () => {
  // ...
});
```

### 5. Clean Up Between Tests

```typescript
beforeEach(() => {
  cy.clearLocalStorage();
  cy.clearCookies();
  // Reset application state
});
```

## Debugging Failed Tests

### 1. Cypress Test Runner (Interactive Mode)

```bash
npm run e2e
```

- Click on failed test to see execution
- Hover over commands to see DOM snapshots
- Inspect network requests in DevTools

### 2. Screenshots

Failed tests automatically capture screenshots:
```
cypress/screenshots/family-creation.cy.ts/should-show-error-when-family-name-is-empty (failed).png
```

### 3. Video Recordings

Enable video recording in `cypress.config.ts`:
```typescript
export default defineConfig({
  e2e: {
    video: true, // Set to true
  }
});
```

### 4. Console Logs

```typescript
cy.get('input[aria-label="Family name"]').then(($input) => {
  console.log('Input value:', $input.val());
});
```

## CI/CD Integration (GitHub Actions Example)

```yaml
name: E2E Tests

on: [push, pull_request]

jobs:
  cypress-run:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v3

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'

      - name: Install dependencies
        run: npm ci

      - name: Run E2E tests
        run: npm run e2e:ci

      - name: Upload screenshots
        if: failure()
        uses: actions/upload-artifact@v3
        with:
          name: cypress-screenshots
          path: cypress/screenshots

      - name: Upload videos
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: cypress-videos
          path: cypress/videos
```

## Test Maintenance

### When to Update Tests

1. **UI Changes** - Update selectors if component structure changes
2. **GraphQL Schema Changes** - Update mock responses to match new schema
3. **New Features** - Add test coverage for new functionality
4. **Bug Fixes** - Add regression tests to prevent recurrence

### Keeping Tests Fast

- Mock all external API calls
- Use `cy.intercept()` instead of actual network requests
- Avoid unnecessary `cy.wait()` - use `cy.should()` instead
- Run tests in headless mode for CI/CD

## Resources

- [Cypress Documentation](https://docs.cypress.io/)
- [cypress-axe GitHub](https://github.com/component-driven/cypress-axe)
- [axe-core Rules](https://github.com/dequelabs/axe-core/blob/develop/doc/rule-descriptions.md)
- [WCAG 2.1 AA Guidelines](https://www.w3.org/WAI/WCAG21/quickref/?currentsidebar=%23col_customize&levels=aaa)

## Next Steps

### Planned Enhancements

1. **Visual Regression Testing** - Add `cypress-image-snapshot` for visual diffs
2. **Performance Testing** - Measure page load times, Time to Interactive
3. **Mobile Testing** - Test responsive behavior at various viewports
4. **Cross-Browser Testing** - Run tests in Firefox, Safari, Edge
5. **API Contract Testing** - Validate GraphQL schema compliance

### Coverage Goals

- **E2E Coverage**: 80%+ of critical user flows
- **Accessibility**: 100% WCAG 2.1 AA compliance
- **Regression Tests**: 100% of bugs have regression tests

---

**Created:** December 2025
**Last Updated:** Phase 5 Implementation
**Test Count:** 20+ tests across 7 scenarios
