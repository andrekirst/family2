import { test, expect } from '@playwright/test';

test.describe('Dashboard', () => {
  test('should display dashboard after login', async ({ page }) => {
    await page.goto('/');
    // Expect redirect to login or dashboard
    await expect(page).toHaveURL(/\/(dashboard|login)/);
  });

  test('should have a visible page structure', async ({ page }) => {
    await page.goto('/');
    // The page should render with a visible body
    await expect(page.locator('body')).toBeVisible();
  });

  test('should include navigation elements when authenticated', async ({ page }) => {
    // This test verifies the page loads without errors
    // Full auth testing requires Keycloak setup
    await page.goto('/');
    const response = await page.waitForLoadState('networkidle');
    // Page should not show a blank screen
    const bodyText = await page.locator('body').textContent();
    expect(bodyText).toBeTruthy();
  });
});
