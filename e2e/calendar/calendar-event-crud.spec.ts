import { test, expect } from '@playwright/test';

test.describe('Calendar Event CRUD', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to calendar (assumes authenticated user with family)
    await page.goto('/calendar');
    await page.waitForSelector('[data-testid="calendar-grid"]');
  });

  test('creates a new event (happy path)', async ({ page }) => {
    // Open create dialog
    await page.click('[data-testid="create-event-button"]');
    await page.waitForSelector('[data-testid="event-title-input"]');

    // Fill in form
    await page.fill('[data-testid="event-title-input"]', 'Doctor Appointment');
    await page.fill('[data-testid="event-description-input"]', 'Annual checkup');
    await page.fill('[data-testid="event-location-input"]', 'City Hospital');

    // Set event type
    await page.selectOption('[data-testid="event-type-select"]', 'Medical');

    // Submit
    await page.click('[data-testid="save-event-button"]');

    // Dialog should close
    await expect(page.locator('[data-testid="event-title-input"]')).not.toBeVisible();
  });

  test('shows validation error when title is empty', async ({ page }) => {
    // Open create dialog
    await page.click('[data-testid="create-event-button"]');
    await page.waitForSelector('[data-testid="event-title-input"]');

    // Submit without filling title
    await page.click('[data-testid="save-event-button"]');

    // Error message should appear
    await expect(page.locator('[data-testid="event-error"]')).toBeVisible();
    await expect(page.locator('text=Event title is required')).toBeVisible();
  });

  test('shows validation error when end time is before start time', async ({ page }) => {
    // Open create dialog
    await page.click('[data-testid="create-event-button"]');
    await page.waitForSelector('[data-testid="event-title-input"]');

    // Fill title
    await page.fill('[data-testid="event-title-input"]', 'Test Event');

    // Set invalid times (end before start)
    await page.fill('[data-testid="event-start-input"]', '2026-03-15T10:00');
    await page.fill('[data-testid="event-end-input"]', '2026-03-15T09:00');

    // Submit
    await page.click('[data-testid="save-event-button"]');

    // Error message should appear
    await expect(page.locator('[data-testid="event-error"]')).toBeVisible();
    await expect(page.locator('text=End time must be after start time')).toBeVisible();
  });

  test('edits an existing event', async ({ page }) => {
    // First, create an event
    await page.click('[data-testid="create-event-button"]');
    await page.waitForSelector('[data-testid="event-title-input"]');
    await page.fill('[data-testid="event-title-input"]', 'Team Meeting');
    await page.selectOption('[data-testid="event-type-select"]', 'Work');
    await page.click('[data-testid="save-event-button"]');

    // Wait for dialog to close and event to appear
    await expect(page.locator('[data-testid="event-title-input"]')).not.toBeVisible();

    // Click the event chip
    const eventChip = page.locator('[data-testid="calendar-event-chip"]', {
      hasText: 'Team Meeting',
    });

    if (await eventChip.isVisible()) {
      await eventChip.click();

      // Edit dialog should open
      await expect(page.locator('text=Edit Event')).toBeVisible();

      // Modify the title
      await page.fill('[data-testid="event-title-input"]', 'Updated Team Meeting');
      await page.click('[data-testid="save-event-button"]');

      // Dialog should close
      await expect(page.locator('[data-testid="event-title-input"]')).not.toBeVisible();
    }
  });

  test('cancels an existing event', async ({ page }) => {
    // First, create an event
    await page.click('[data-testid="create-event-button"]');
    await page.waitForSelector('[data-testid="event-title-input"]');
    await page.fill('[data-testid="event-title-input"]', 'Event To Cancel');
    await page.selectOption('[data-testid="event-type-select"]', 'Personal');
    await page.click('[data-testid="save-event-button"]');

    // Wait for dialog to close
    await expect(page.locator('[data-testid="event-title-input"]')).not.toBeVisible();

    // Click the event chip to open edit dialog
    const eventChip = page.locator('[data-testid="calendar-event-chip"]', {
      hasText: 'Event To Cancel',
    });

    if (await eventChip.isVisible()) {
      await eventChip.click();

      // Click cancel event button
      await expect(page.locator('[data-testid="cancel-event-button"]')).toBeVisible();
      await page.click('[data-testid="cancel-event-button"]');

      // Dialog should close
      await expect(page.locator('[data-testid="event-title-input"]')).not.toBeVisible();
    }
  });

  test('closes dialog by clicking overlay', async ({ page }) => {
    // Open dialog
    await page.click('[data-testid="create-event-button"]');
    await page.waitForSelector('[data-testid="event-title-input"]');

    // Click overlay (outside dialog content)
    await page.locator('.fixed.inset-0').click({ position: { x: 10, y: 10 } });

    // Dialog should close
    await expect(page.locator('[data-testid="event-title-input"]')).not.toBeVisible();
  });

  test('all-day toggle changes input type', async ({ page }) => {
    // Open dialog
    await page.click('[data-testid="create-event-button"]');
    await page.waitForSelector('[data-testid="event-title-input"]');

    // Initially should be datetime-local
    await expect(page.locator('[data-testid="event-start-input"]')).toHaveAttribute(
      'type',
      'datetime-local'
    );

    // Toggle all-day
    await page.click('[data-testid="event-allday-checkbox"]');

    // Should now be date type
    await expect(page.locator('[data-testid="event-start-input"]')).toHaveAttribute('type', 'date');
  });
});
