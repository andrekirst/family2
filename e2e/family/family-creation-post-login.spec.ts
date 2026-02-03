import { test, expect } from '@playwright/test';

test.describe('Family Creation Post-Login', () => {
  test.beforeEach(async ({ page }) => {
    // Assumption: Test helpers exist for OAuth login
    // This would typically use a test user that doesn't have a family
    await page.goto('/');
  });

  test('shows create family dialog when user has no family', async ({ page }) => {
    // Step 1: Login as user without family
    await page.click('[data-testid="login-button"]');

    // Wait for OAuth redirect and callback
    // (Implementation depends on your OAuth test setup)
    await page.waitForURL('/dashboard');

    // Step 2: Verify dialog appears after delay
    await page.waitForSelector('[data-testid="family-name-input"]', { timeout: 2000 });

    const dialogVisible = await page.isVisible('[data-testid="family-name-input"]');
    expect(dialogVisible).toBe(true);
  });

  test('creates family from post-login dialog', async ({ page }) => {
    // Step 1: Login and wait for dialog
    await page.goto('/dashboard'); // Assume already authenticated
    await page.waitForSelector('[data-testid="family-name-input"]');

    // Step 2: Fill in family name
    await page.fill('[data-testid="family-name-input"]', 'Test Family');

    // Step 3: Submit form
    await page.click('[data-testid="create-family-button"]');

    // Step 4: Verify dialog closes and family appears on dashboard
    await expect(page.locator('[data-testid="family-name-input"]')).not.toBeVisible();
    await expect(page.locator('text=Family: Test Family')).toBeVisible();
  });

  test('dismisses dialog and allows manual creation later', async ({ page }) => {
    // Step 1: Login and wait for dialog
    await page.goto('/dashboard');
    await page.waitForSelector('[data-testid="family-name-input"]');

    // Step 2: Click "Skip for Now"
    await page.click('text=Skip for Now');

    // Step 3: Verify dialog closes
    await expect(page.locator('[data-testid="family-name-input"]')).not.toBeVisible();

    // Step 4: Verify "Create Family" button is still available
    await expect(page.locator('text=Create Family')).toBeVisible();

    // Step 5: Click button to reopen dialog
    await page.click('text=Create Family');

    // Step 6: Verify dialog reopens
    await expect(page.locator('[data-testid="family-name-input"]')).toBeVisible();
  });

  test('shows error when family name is empty', async ({ page }) => {
    // Step 1: Open dialog
    await page.goto('/dashboard');
    await page.waitForSelector('[data-testid="family-name-input"]');

    // Step 2: Submit without entering name
    await page.click('[data-testid="create-family-button"]');

    // Step 3: Verify error message
    await expect(page.locator('text=Family name is required')).toBeVisible();
  });
});
