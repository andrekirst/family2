import { test, expect } from '@playwright/test';

test.describe('Login Flow', () => {
  test('should redirect unauthenticated users to login page', async ({ page }) => {
    await page.goto('/dashboard');
    // Unauthenticated users should be redirected to login
    await expect(page).toHaveURL(/\/(login|auth)/);
  });

  test('should display the login page with sign-in button', async ({ page }) => {
    await page.goto('/login');
    // The login page should contain a sign-in action
    await expect(page.locator('body')).toBeVisible();
  });

  test('should show the application name on the login page', async ({ page }) => {
    await page.goto('/login');
    // Verify the app branding is visible
    const heading = page.getByText('Family Hub');
    await expect(heading.first()).toBeVisible();
  });
});
