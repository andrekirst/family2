/**
 * Screenshot Capture Test
 *
 * Manual test to capture screenshots of the sidebar layout for documentation.
 * Run with: npx playwright test screenshot-capture.spec.ts --project=chromium
 */

import { test, expect, Page } from '@playwright/test';

/**
 * Helper: Setup authenticated session with family
 */
async function setupAuthenticatedSession(page: Page): Promise<void> {
  // Mock OAuth authentication
  const mockAccessToken = 'mock-jwt-token-sidebar-test';
  const mockExpiresAt = new Date(Date.now() + 3600000).toISOString();

  await page.addInitScript(({ token, expires }) => {
    window.localStorage.setItem('family_hub_access_token', token);
    window.localStorage.setItem('family_hub_token_expires', expires);
  }, { token: mockAccessToken, expires: mockExpiresAt });

  // Mock GetCurrentFamily - user has a family
  await page.route('http://localhost:5002/graphql', async (route) => {
    const request = route.request();
    const postData = request.postDataJSON();

    if (postData?.query?.includes('GetCurrentFamily')) {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: {
            family: {
              id: 'test-family-123',
              name: 'Test Family',
              createdAt: '2026-01-05T00:00:00Z',
              auditInfo: {
                createdAt: '2026-01-05T00:00:00Z',
                updatedAt: '2026-01-05T00:00:00Z'
              }
            }
          }
        })
      });
    } else if (postData?.query?.includes('GetFamilyMembers')) {
      // Mock family members for family management page
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: {
            familyMembers: [
              {
                id: 'member-1',
                name: 'John Doe',
                email: 'john@example.com',
                role: 'OWNER',
                joinedAt: '2026-01-05T00:00:00Z'
              }
            ]
          }
        })
      });
    } else if (postData?.query?.includes('GetPendingInvitations')) {
      // Mock pending invitations
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: {
            pendingInvitations: []
          }
        })
      });
    } else {
      await route.continue();
    }
  });
}

test.describe('Screenshot Capture', () => {
  test('capture dashboard page with sidebar', async ({ page }) => {
    await setupAuthenticatedSession(page);
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Wait a bit for everything to render
    await page.waitForTimeout(1000);

    // Capture full page screenshot
    await page.screenshot({
      path: 'playwright-report/dashboard-with-sidebar.png',
      fullPage: true
    });

    console.log('Screenshot saved: dashboard-with-sidebar.png');
  });

  test('capture family management page with sidebar', async ({ page }) => {
    await setupAuthenticatedSession(page);
    await page.goto('/family/manage');
    await page.waitForLoadState('networkidle');

    // Wait a bit for everything to render
    await page.waitForTimeout(1000);

    // Capture full page screenshot
    await page.screenshot({
      path: 'playwright-report/family-management-with-sidebar.png',
      fullPage: true
    });

    console.log('Screenshot saved: family-management-with-sidebar.png');
  });

  test('capture navigation flow', async ({ page }) => {
    await setupAuthenticatedSession(page);

    // Start at dashboard
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Click Family link
    await page.locator('a[href="/family/manage"]').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Capture family page with active highlight
    await page.screenshot({
      path: 'playwright-report/family-management-active-state.png',
      fullPage: true
    });

    console.log('Screenshot saved: family-management-active-state.png');
  });
});
