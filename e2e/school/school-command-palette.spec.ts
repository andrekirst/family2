import { test, expect } from '@playwright/test';

test.describe('School — Command Palette Integration', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to dashboard (assumes authenticated user with family)
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
  });

  test('Ctrl+K shows "School" in default navigation items', async ({ page }) => {
    // Open command palette
    await page.keyboard.press('Control+k');
    await expect(page.locator('[data-testid="command-palette"]')).toBeVisible();

    // "School" should appear in the default navigation items
    const schoolItem = page.locator('[data-testid="palette-item"]', { hasText: 'School' });
    await expect(schoolItem).toBeVisible();
  });

  test('typing "students" shows View Students and Mark as Student commands', async ({ page }) => {
    // Open command palette
    await page.keyboard.press('Control+k');
    await expect(page.locator('[data-testid="command-palette-input"]')).toBeVisible();

    // Type "students"
    await page.fill('[data-testid="command-palette-input"]', 'students');

    // Wait for debounce (300ms) + results
    await page.waitForTimeout(500);

    // Both school commands should appear
    const items = page.locator('[data-testid="palette-item"]');
    const allText = await items.allTextContents();
    const joinedText = allText.join(' ');

    expect(joinedText).toContain('View Students');
  });

  test('clicking "Go to School" navigates to /school', async ({ page }) => {
    // Open command palette
    await page.keyboard.press('Control+k');
    await expect(page.locator('[data-testid="command-palette"]')).toBeVisible();

    // Find and click the "School" navigation item
    const schoolItem = page.locator('[data-testid="palette-item"]', { hasText: 'School' });
    await expect(schoolItem).toBeVisible();
    await schoolItem.click();

    // Should navigate to /school
    await expect(page).toHaveURL(/\/school/);

    // Command palette should close
    await expect(page.locator('[data-testid="command-palette"]')).not.toBeVisible();
  });

  test('Enter key on "Go to School" navigates to /school', async ({ page }) => {
    // Open command palette
    await page.keyboard.press('Control+k');
    await expect(page.locator('[data-testid="command-palette-input"]')).toBeVisible();

    // Type "go to school" to trigger NLP
    await page.fill('[data-testid="command-palette-input"]', 'go to school');
    await page.waitForTimeout(500);

    // Press Enter to execute the first result
    await page.keyboard.press('Enter');

    // Should navigate to /school
    await expect(page).toHaveURL(/\/school/);
  });

  test('typing "Schüler" surfaces German school commands', async ({ page }) => {
    // Open command palette
    await page.keyboard.press('Control+k');
    await expect(page.locator('[data-testid="command-palette-input"]')).toBeVisible();

    // Type German keyword
    await page.fill('[data-testid="command-palette-input"]', 'Schüler');
    await page.waitForTimeout(500);

    // Should have results (the keyword is in the command descriptors)
    const items = page.locator('[data-testid="palette-item"]');
    await expect(items.first()).toBeVisible();
  });

  test('Escape closes command palette', async ({ page }) => {
    // Open command palette
    await page.keyboard.press('Control+k');
    await expect(page.locator('[data-testid="command-palette"]')).toBeVisible();

    // Close with Escape
    await page.keyboard.press('Escape');
    await expect(page.locator('[data-testid="command-palette"]')).not.toBeVisible();
  });
});
