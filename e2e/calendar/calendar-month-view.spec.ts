import { test, expect } from '@playwright/test';

test.describe('Calendar Month View', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to calendar (assumes authenticated user with family)
    await page.goto('/calendar');
  });

  test('renders month grid with day headers', async ({ page }) => {
    await page.waitForSelector('[data-testid="calendar-grid"]');

    // Verify day headers are present
    const grid = page.locator('[data-testid="calendar-grid"]');
    await expect(grid).toBeVisible();

    // Check for day header names
    for (const day of ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']) {
      await expect(grid.locator(`text=${day}`)).toBeVisible();
    }
  });

  test('displays current month label', async ({ page }) => {
    const monthLabel = page.locator('[data-testid="current-month-label"]');
    await expect(monthLabel).toBeVisible();

    // Should contain a month and year (e.g., "February 2026")
    const text = await monthLabel.textContent();
    expect(text).toMatch(/\w+ \d{4}/);
  });

  test('navigates to previous month', async ({ page }) => {
    const monthLabel = page.locator('[data-testid="current-month-label"]');
    const initialText = await monthLabel.textContent();

    await page.click('[data-testid="prev-month"]');

    // Month label should change
    await expect(monthLabel).not.toHaveText(initialText!);
  });

  test('navigates to next month', async ({ page }) => {
    const monthLabel = page.locator('[data-testid="current-month-label"]');
    const initialText = await monthLabel.textContent();

    await page.click('[data-testid="next-month"]');

    // Month label should change
    await expect(monthLabel).not.toHaveText(initialText!);
  });

  test('displays calendar day cells', async ({ page }) => {
    await page.waitForSelector('[data-testid="calendar-grid"]');

    // Grid should have day cells (at least 28 for the shortest month)
    const dayCells = page.locator('[data-testid="calendar-day"]');
    const count = await dayCells.count();
    expect(count).toBeGreaterThanOrEqual(28);
    expect(count).toBeLessThanOrEqual(42);
  });

  test('clicking a day opens create event dialog', async ({ page }) => {
    await page.waitForSelector('[data-testid="calendar-grid"]');

    // Click the first day cell
    await page.locator('[data-testid="calendar-day"]').first().click();

    // Event dialog should appear
    await expect(page.locator('[data-testid="event-title-input"]')).toBeVisible();
    await expect(page.locator('text=New Event')).toBeVisible();
  });

  test('clicking "New Event" button opens create dialog', async ({ page }) => {
    await page.click('[data-testid="create-event-button"]');

    await expect(page.locator('[data-testid="event-title-input"]')).toBeVisible();
    await expect(page.locator('text=New Event')).toBeVisible();
  });

  test('back to dashboard link works', async ({ page }) => {
    await page.click('[data-testid="back-to-dashboard"]');
    await page.waitForURL('/dashboard');
  });
});
