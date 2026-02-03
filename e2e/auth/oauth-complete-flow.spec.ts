import { test, expect } from '@playwright/test';

/**
 * E2E Test: OAuth 2.0 Complete Login Flow
 *
 * Tests the complete authentication flow from login to dashboard:
 * 1. User navigates to app (redirects to login)
 * 2. User clicks "Sign in with Keycloak"
 * 3. User enters credentials in Keycloak
 * 4. Keycloak redirects to callback with authorization code
 * 5. Frontend exchanges code for tokens (OAuth PKCE)
 * 6. Frontend syncs with backend (RegisterUser mutation)
 * 7. Dashboard loads with user data from backend
 *
 * Prerequisites:
 * - Keycloak running on http://localhost:8080
 * - Backend API running on http://localhost:7000
 * - Frontend running on http://localhost:4200
 * - Test user exists in Keycloak realm
 */

test.describe('OAuth Login Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Clear localStorage to ensure clean test state
    await page.goto('http://localhost:4200');
    await page.evaluate(() => localStorage.clear());
  });

  test('should complete full login flow and display user data from backend', async ({ page }) => {
    // Step 1: Navigate to app (should redirect to login)
    await page.goto('http://localhost:4200');
    await expect(page).toHaveURL(/\/login/);
    await expect(page.getByText('Sign in with Keycloak')).toBeVisible();

    // Step 2: Click login button (redirects to Keycloak)
    await page.click('button:has-text("Sign in with Keycloak")');

    // Step 3: Wait for Keycloak login page
    await page.waitForURL(/localhost:8080/);
    await expect(page.locator('input[name="username"]')).toBeVisible();

    // Step 4: Enter Keycloak credentials
    // NOTE: Replace with actual test user credentials
    await page.fill('input[name="username"]', 'test@example.com');
    await page.fill('input[name="password"]', 'test123');
    await page.click('button[type="submit"]');

    // Step 5: Wait for callback processing
    await expect(page).toHaveURL(/\/callback/);

    // Step 6: Verify progress indicators are shown
    await expect(page.getByText('Exchanging authorization code...')).toBeVisible();
    await expect(page.getByText('Syncing with backend...')).toBeVisible();
    await expect(page.getByText('Loading dashboard...')).toBeVisible();

    // Step 7: Wait for redirect to dashboard (with timeout for backend sync)
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 10000 });

    // Step 8: Verify success message
    await expect(page.getByText('Successfully logged in!')).toBeVisible();

    // Step 9: Verify user data from BACKEND (not just JWT token)
    // This confirms the RegisterUser mutation succeeded and dashboard fetched from backend
    await expect(page.getByText(/Welcome, /)).toBeVisible();
    await expect(page.getByText('test@example.com')).toBeVisible();

    // Step 10: Verify email verification badge
    const emailBadge = page.locator(
      'span:has-text("Email Verified"), span:has-text("Email Not Verified")'
    );
    await expect(emailBadge).toBeVisible();

    // Step 11: Verify family section exists (either has family or "No Family Yet")
    const familySection = page.locator('div:has-text("Family:"), div:has-text("No Family Yet")');
    await expect(familySection).toBeVisible();
  });

  test('should handle backend sync failure gracefully', async ({ page, context }) => {
    // Mock GraphQL request to fail RegisterUser mutation
    await page.route('**/graphql', async (route) => {
      const postData = route.request().postData();
      if (postData && postData.includes('registerUser')) {
        // Simulate backend error
        await route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({
            errors: [{ message: 'Database connection failed' }],
          }),
        });
      } else {
        // Let other requests through
        await route.continue();
      }
    });

    // Navigate and try to login
    await page.goto('http://localhost:4200');
    await page.click('button:has-text("Sign in with Keycloak")');

    // Complete Keycloak login (mocked - adjust based on test environment)
    // In real test: fill credentials and submit

    // Wait for callback
    await expect(page).toHaveURL(/\/callback/);

    // Should show error message
    await expect(page.getByText(/Failed to complete authentication/)).toBeVisible();

    // Should show retry button
    await expect(page.getByText('Try Again')).toBeVisible();

    // Clicking retry should redirect back to login
    await page.click('button:has-text("Try Again")');
    await expect(page).toHaveURL(/localhost:8080/); // Redirects to Keycloak
  });

  test('should display loading state while fetching user from backend', async ({ page }) => {
    // This test verifies the dashboard loading skeleton
    await page.goto('http://localhost:4200/dashboard');

    // Should show loading skeleton (pulse animation)
    const loadingSkeleton = page.locator('div.animate-pulse');
    await expect(loadingSkeleton).toBeVisible({ timeout: 1000 });

    // After loading, should show user data
    await expect(page.getByText(/Welcome, /)).toBeVisible({ timeout: 5000 });
  });

  test('should clear user state on logout', async ({ page }) => {
    // Assume user is already logged in
    await page.goto('http://localhost:4200/dashboard');

    // Wait for user data to load
    await expect(page.getByText(/Welcome, /)).toBeVisible();

    // Click logout
    await page.click('button:has-text("Logout")');

    // Should redirect to Keycloak logout or login page
    await expect(page).toHaveURL(/localhost:8080|\/login/);

    // Verify localStorage is cleared
    const accessToken = await page.evaluate(() => localStorage.getItem('access_token'));
    expect(accessToken).toBeNull();
  });
});

/**
 * Additional Test Scenarios (Optional - Implement as needed)
 */
test.describe('OAuth Edge Cases', () => {
  test.skip('should handle expired JWT token and refresh', async ({ page }) => {
    // TODO: Test token expiration and refresh flow
  });

  test.skip('should handle invalid authorization code', async ({ page }) => {
    // TODO: Navigate to callback with invalid code parameter
  });

  test.skip('should handle missing state parameter (CSRF protection)', async ({ page }) => {
    // TODO: Navigate to callback without state parameter
  });

  test.skip('should handle Keycloak being unavailable', async ({ page }) => {
    // TODO: Mock Keycloak endpoint to return 503
  });
});
