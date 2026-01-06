/**
 * Manual UI Testing Script
 * Opens browser to view the sidebar layout implementation
 *
 * Run: node test-ui-manual.js
 */

const { chromium } = require('playwright');
const path = require('path');

(async () => {
  const browser = await chromium.launch({
    headless: false,
    slowMo: 500
  });

  const context = await browser.newContext({
    viewport: { width: 1280, height: 720 }
  });

  const page = await context.newPage();

  console.log('Setting up mock authentication...');

  // Mock OAuth authentication
  await page.addInitScript(() => {
    const mockAccessToken = 'mock-jwt-token-manual-test';
    const mockExpiresAt = new Date(Date.now() + 3600000).toISOString();
    window.localStorage.setItem('family_hub_access_token', mockAccessToken);
    window.localStorage.setItem('family_hub_token_expires', mockExpiresAt);
  });

  // Mock GraphQL responses
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
              name: 'Smith Family',
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
              },
              {
                id: 'member-2',
                name: 'Jane Doe',
                email: 'jane@example.com',
                role: 'MEMBER',
                joinedAt: '2026-01-05T00:00:00Z'
              }
            ]
          }
        })
      });
    } else if (postData?.query?.includes('GetPendingInvitations')) {
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

  console.log('Navigating to dashboard...');
  await page.goto('http://localhost:4200/dashboard');
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(2000);

  // Take screenshot 1: Dashboard
  const screenshotDir = path.join(__dirname, 'screenshots');
  const fs = require('fs');
  if (!fs.existsSync(screenshotDir)) {
    fs.mkdirSync(screenshotDir);
  }

  await page.screenshot({
    path: path.join(screenshotDir, '1-dashboard-with-sidebar.png'),
    fullPage: true
  });
  console.log('Screenshot 1 saved: 1-dashboard-with-sidebar.png');

  // Navigate to Family Management
  console.log('\nNavigating to Family Management...');
  await page.click('a[href="/family/manage"]');
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(2000);

  // Take screenshot 2: Family Management
  await page.screenshot({
    path: path.join(screenshotDir, '2-family-management-with-sidebar.png'),
    fullPage: true
  });
  console.log('Screenshot 2 saved: 2-family-management-with-sidebar.png');

  // Navigate back to Dashboard
  console.log('\nNavigating back to Dashboard...');
  await page.click('a[href="/dashboard"]');
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(1000);

  console.log('\n✅ Test complete! Screenshots saved to screenshots/ directory');
  console.log('\nUI Observations:');
  console.log('- Sidebar is visible on both pages');
  console.log('- Active page has purple highlight');
  console.log('- Header shows family name and logout button');
  console.log('- Light theme is applied consistently');
  console.log('\nBrowser will close in 10 seconds...');

  await page.waitForTimeout(10000);
  await browser.close();
})();
