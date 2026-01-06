/**
 * E2E Test Suite: Sidebar Layout Navigation
 *
 * Tests the newly implemented MainLayoutComponent with sidebar navigation
 * across dashboard and family management pages.
 *
 * Test Coverage:
 * - Sidebar presence and structure
 * - Navigation items (Dashboard, Family)
 * - Active route highlighting (purple accent)
 * - Navigation between pages
 * - Header consistency (family name, logout button)
 * - Light theme consistency
 * - Visual regression via screenshots
 *
 * Manual Test Run:
 * - Requires frontend at http://localhost:4200
 * - Requires backend at http://localhost:5002
 * - Run with: npx playwright test sidebar-layout.spec.ts --headed
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

test.describe('Sidebar Layout Navigation', () => {
  test.describe('Dashboard Page Layout', () => {
    test('should display sidebar with navigation items on dashboard', async ({ page }) => {
      await test.step('Setup: Authenticate and navigate to dashboard', async () => {
        await setupAuthenticatedSession(page);
        await page.goto('/dashboard');
        await page.waitForLoadState('networkidle');
      });

      await test.step('Verify: Header displays family name and user info', async () => {
        // Check family name in header
        await expect(page.locator('header h1')).toContainText('Test Family');

        // Check logout button exists
        await expect(page.getByRole('button', { name: /sign out/i })).toBeVisible();
      });

      await test.step('Verify: Sidebar structure and navigation items', async () => {
        // Sidebar should be visible
        const sidebar = page.locator('aside');
        await expect(sidebar).toBeVisible();

        // Check for Dashboard navigation item
        const dashboardLink = page.locator('a[href="/dashboard"]');
        await expect(dashboardLink).toBeVisible();
        await expect(dashboardLink).toContainText('Dashboard');

        // Check for Family navigation item
        const familyLink = page.locator('a[href="/family/manage"]');
        await expect(familyLink).toBeVisible();
        await expect(familyLink).toContainText('Family');
      });

      await test.step('Verify: Active route highlighting on dashboard', async () => {
        const dashboardLink = page.locator('a[href="/dashboard"]');

        // Dashboard should have purple background (active state)
        await expect(dashboardLink).toHaveClass(/bg-purple-50/);
        await expect(dashboardLink).toHaveClass(/text-purple-700/);

        // Family link should NOT have purple background
        const familyLink = page.locator('a[href="/family/manage"]');
        await expect(familyLink).not.toHaveClass(/bg-purple-50/);
        await expect(familyLink).toHaveClass(/text-gray-700/);
      });

      await test.step('Verify: Dashboard content renders correctly', async () => {
        // Check main content area
        await expect(page.getByText('Welcome to your Family Hub!')).toBeVisible();
        await expect(page.getByRole('heading', { name: 'Your Family', exact: true })).toBeVisible();
        // Family name appears in both header and content, so check in main
        await expect(page.getByRole('main').getByText('Test Family')).toBeVisible();
      });

      await test.step('Screenshot: Dashboard with sidebar', async () => {
        await page.screenshot({
          path: 'playwright-report/screenshots/dashboard-sidebar.png',
          fullPage: true
        });
      });
    });
  });

  test.describe('Family Management Page Layout', () => {
    test('should display sidebar with navigation items on family management page', async ({ page }) => {
      await test.step('Setup: Authenticate and navigate to family management', async () => {
        await setupAuthenticatedSession(page);
        await page.goto('/family/manage');
        await page.waitForLoadState('networkidle');
      });

      await test.step('Verify: Header displays family name (consistent with dashboard)', async () => {
        // Check family name in header
        await expect(page.locator('header h1')).toContainText('Test Family');

        // Check logout button exists
        await expect(page.getByRole('button', { name: /sign out/i })).toBeVisible();
      });

      await test.step('Verify: Sidebar structure matches dashboard', async () => {
        // Sidebar should be visible
        const sidebar = page.locator('aside');
        await expect(sidebar).toBeVisible();

        // Check both navigation items
        await expect(page.locator('a[href="/dashboard"]')).toBeVisible();
        await expect(page.locator('a[href="/family/manage"]')).toBeVisible();
      });

      await test.step('Verify: Active route highlighting on family management', async () => {
        const familyLink = page.locator('a[href="/family/manage"]');

        // Family should have purple background (active state)
        await expect(familyLink).toHaveClass(/bg-purple-50/);
        await expect(familyLink).toHaveClass(/text-purple-700/);

        // Dashboard link should NOT have purple background
        const dashboardLink = page.locator('a[href="/dashboard"]');
        await expect(dashboardLink).not.toHaveClass(/bg-purple-50/);
        await expect(dashboardLink).toHaveClass(/text-gray-700/);
      });

      await test.step('Verify: Light theme applied (no dark mode classes)', async () => {
        // Check background is light gray, not dark
        const mainContainer = page.locator('div.min-h-screen');
        await expect(mainContainer).toHaveClass(/bg-gray-50/);
        await expect(mainContainer).not.toHaveClass(/bg-gray-900/);

        // Check header is white
        const header = page.locator('header');
        await expect(header).toHaveClass(/bg-white/);
      });

      await test.step('Screenshot: Family management with sidebar', async () => {
        await page.screenshot({
          path: 'playwright-report/screenshots/family-management-sidebar.png',
          fullPage: true
        });
      });
    });
  });

  test.describe('Navigation Between Pages', () => {
    test('should navigate from dashboard to family management via sidebar', async ({ page }) => {
      await test.step('Setup: Start on dashboard', async () => {
        await setupAuthenticatedSession(page);
        await page.goto('/dashboard');
        await page.waitForLoadState('networkidle');
      });

      await test.step('Verify: Currently on dashboard', async () => {
        await expect(page).toHaveURL(/\/dashboard/);
        await expect(page.getByText('Welcome to your Family Hub!')).toBeVisible();
      });

      await test.step('Action: Click Family link in sidebar', async () => {
        const familyLink = page.locator('a[href="/family/manage"]');
        await familyLink.click();
        await page.waitForLoadState('networkidle');
      });

      await test.step('Verify: Navigated to family management', async () => {
        await expect(page).toHaveURL(/\/family\/manage/);

        // Family link should now be active
        const familyLink = page.locator('a[href="/family/manage"]');
        await expect(familyLink).toHaveClass(/bg-purple-50/);
        await expect(familyLink).toHaveClass(/text-purple-700/);

        // Dashboard link should not be active
        const dashboardLink = page.locator('a[href="/dashboard"]');
        await expect(dashboardLink).not.toHaveClass(/bg-purple-50/);
      });

      await test.step('Screenshot: After navigation', async () => {
        await page.screenshot({
          path: 'playwright-report/screenshots/after-navigation-to-family.png',
          fullPage: true
        });
      });
    });

    test('should navigate from family management to dashboard via sidebar', async ({ page }) => {
      await test.step('Setup: Start on family management', async () => {
        await setupAuthenticatedSession(page);
        await page.goto('/family/manage');
        await page.waitForLoadState('networkidle');
      });

      await test.step('Verify: Currently on family management', async () => {
        await expect(page).toHaveURL(/\/family\/manage/);
      });

      await test.step('Action: Click Dashboard link in sidebar', async () => {
        const dashboardLink = page.locator('a[href="/dashboard"]');
        await dashboardLink.click();
        await page.waitForLoadState('networkidle');
      });

      await test.step('Verify: Navigated to dashboard', async () => {
        await expect(page).toHaveURL(/\/dashboard/);
        await expect(page.getByText('Welcome to your Family Hub!')).toBeVisible();

        // Dashboard link should now be active
        const dashboardLink = page.locator('a[href="/dashboard"]');
        await expect(dashboardLink).toHaveClass(/bg-purple-50/);
        await expect(dashboardLink).toHaveClass(/text-purple-700/);
      });

      await test.step('Screenshot: After navigation', async () => {
        await page.screenshot({
          path: 'playwright-report/screenshots/after-navigation-to-dashboard.png',
          fullPage: true
        });
      });
    });
  });

  test.describe('Visual Design Consistency', () => {
    test('should maintain consistent header across both pages', async ({ page }) => {
      await setupAuthenticatedSession(page);

      await test.step('Verify: Dashboard header', async () => {
        await page.goto('/dashboard');
        await page.waitForLoadState('networkidle');

        const headerText = await page.locator('header h1').textContent();
        expect(headerText).toContain('Test Family');
      });

      await test.step('Verify: Family management header (same as dashboard)', async () => {
        await page.goto('/family/manage');
        await page.waitForLoadState('networkidle');

        const headerText = await page.locator('header h1').textContent();
        expect(headerText).toContain('Test Family');
      });
    });

    test('should use light theme consistently (no dark mode)', async ({ page }) => {
      await setupAuthenticatedSession(page);

      await test.step('Verify: Dashboard uses light theme', async () => {
        await page.goto('/dashboard');
        await page.waitForLoadState('networkidle');

        // Check light background
        const container = page.locator('div.min-h-screen').first();
        await expect(container).toHaveClass(/bg-gray-50/);
      });

      await test.step('Verify: Family management uses light theme', async () => {
        await page.goto('/family/manage');
        await page.waitForLoadState('networkidle');

        // Check light background
        const container = page.locator('div.min-h-screen').first();
        await expect(container).toHaveClass(/bg-gray-50/);
      });
    });

    test('should display purple accent for active navigation items', async ({ page }) => {
      await setupAuthenticatedSession(page);

      await test.step('Verify: Active styles on dashboard', async () => {
        await page.goto('/dashboard');
        await page.waitForLoadState('networkidle');

        const activeLink = page.locator('a[href="/dashboard"]');
        const inactiveLink = page.locator('a[href="/family/manage"]');

        // Active: purple background and text
        await expect(activeLink).toHaveClass(/bg-purple-50/);
        await expect(activeLink).toHaveClass(/text-purple-700/);

        // Inactive: gray text, no purple background
        await expect(inactiveLink).toHaveClass(/text-gray-700/);
        await expect(inactiveLink).not.toHaveClass(/bg-purple-50/);
      });

      await test.step('Verify: Active styles on family management', async () => {
        await page.goto('/family/manage');
        await page.waitForLoadState('networkidle');

        const activeLink = page.locator('a[href="/family/manage"]');
        const inactiveLink = page.locator('a[href="/dashboard"]');

        // Active: purple background and text
        await expect(activeLink).toHaveClass(/bg-purple-50/);
        await expect(activeLink).toHaveClass(/text-purple-700/);

        // Inactive: gray text, no purple background
        await expect(inactiveLink).toHaveClass(/text-gray-700/);
        await expect(inactiveLink).not.toHaveClass(/bg-purple-50/);
      });
    });
  });

  test.describe('Logout Functionality', () => {
    test('should logout from dashboard and redirect to login', async ({ page }) => {
      await test.step('Setup: Navigate to dashboard', async () => {
        await setupAuthenticatedSession(page);
        await page.goto('/dashboard');
        await page.waitForLoadState('networkidle');
      });

      await test.step('Action: Click Sign Out button', async () => {
        const logoutButton = page.getByRole('button', { name: /sign out/i });
        await logoutButton.click();
      });

      await test.step('Verify: Redirected to login page', async () => {
        // Should redirect to /login after logout
        await expect(page).toHaveURL(/\/login/);
      });
    });

    test('should logout from family management and redirect to login', async ({ page }) => {
      await test.step('Setup: Navigate to family management', async () => {
        await setupAuthenticatedSession(page);
        await page.goto('/family/manage');
        await page.waitForLoadState('networkidle');
      });

      await test.step('Action: Click Sign Out button', async () => {
        const logoutButton = page.getByRole('button', { name: /sign out/i });
        await logoutButton.click();
      });

      await test.step('Verify: Redirected to login page', async () => {
        // Should redirect to /login after logout
        await expect(page).toHaveURL(/\/login/);
      });
    });
  });
});
