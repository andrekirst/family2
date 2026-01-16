/**
 * E2E Test Suite: Invitation Email Verification (Playwright + MailHog)
 *
 * ⚠️ **TESTS SKIPPED - BLOCKED BY ISSUE #91** ⚠️
 *
 * **Why Skipped:**
 * These E2E tests require real backend authentication (valid JWT tokens) to work properly.
 * GraphQL mocking prevents the backend from creating invitations in the database, so
 * no emails are sent by the background service (InvitationEmailService).
 *
 * **Alternative Testing:**
 * Email verification is already covered by backend integration tests:
 * `src/api/tests/FamilyHub.Tests.Integration/Family/Infrastructure/InvitationEmailIntegrationTests.cs`
 *
 * These 9 integration tests verify:
 * - ✅ Email delivery to MailHog
 * - ✅ Subject line content
 * - ✅ Personal message inclusion
 * - ✅ Valid invitation token links
 * - ✅ Role-specific content (ADMIN vs MEMBER)
 * - ✅ Multiple invitations (batch processing)
 * - ✅ HTML + plain text MIME parts
 * - ✅ From address configuration
 *
 * **When to Re-enable:**
 * Once Issue #91 (E2E Authentication) is implemented, these tests can be refactored to:
 * 1. Use real OAuth tokens (not mocked localStorage)
 * 2. Call real backend GraphQL API (not mocked routes)
 * 3. Verify emails from complete end-to-end flow
 *
 * **References:**
 * - Issue #91: E2E Authentication implementation
 * - Issue #87: Family invitation flow E2E tests (parent issue)
 * - Backend integration tests: InvitationEmailIntegrationTests.cs
 *
 * **Technical Details:**
 * Email flow requires: GraphQL → Database (email_outbox) → Background Service → SMTP → MailHog
 * Mocking GraphQL blocks database writes, preventing emails from being sent.
 */

import { test, expect } from '@playwright/test';
import { MailHogClient } from '../support/email-helpers';

// ⚠️ TESTS SKIPPED - See file header for explanation
// Email verification is covered by backend integration tests (InvitationEmailIntegrationTests.cs)
// These E2E tests require Issue #91 (E2E Authentication) to be implemented first
test.describe.skip('Invitation Email Verification', () => {
  let mailHog: MailHogClient;

  test.beforeEach(async ({ context }) => {
    // Initialize MailHog client
    mailHog = new MailHogClient();

    // Clear all emails before each test for clean state
    await mailHog.clearEmails();

    // Reset application state
    await context.clearCookies();
    await context.clearPermissions();
  });

  /**
   * Setup authenticated session with mock OAuth tokens
   */
  async function setupAuthenticatedSession(page: any) {
    await page.addInitScript(() => {
      window.localStorage.setItem('family_hub_access_token', 'mock-jwt-token-for-testing');
      window.localStorage.setItem(
        'family_hub_token_expires',
        new Date(Date.now() + 3600000).toISOString()
      );
    });
  }

  /**
   * Setup GraphQL mocking for family creation wizard
   * TEMPORARY: Mocks backend responses until E2E auth is implemented
   */
  async function setupFamilyCreationMocks(page: any) {
    await page.route('http://localhost:5002/graphql', async (route: any) => {
      const request = route.request();
      const postData = request.postDataJSON();

      // GetCurrentUser query
      if (postData?.query?.includes('GetCurrentUser')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: {
              currentUser: {
                id: 'user-email-test-123',
                email: 'owner@example.com',
                emailVerified: true,
                firstName: 'Test',
                lastName: 'Owner',
              },
            },
          }),
        });
      }
      // GetCurrentFamily query
      else if (postData?.query?.includes('GetCurrentFamily')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: {
              family: null,
            },
          }),
        });
      }
      // CreateFamily mutation
      else if (postData?.query?.includes('CreateFamily')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: {
              createFamily: {
                createdFamilyDto: {
                  id: 'mock-family-id-123',
                  name: postData.variables.input.name,
                },
                errors: [],
              },
            },
          }),
        });
      }
      // InviteFamilyMembers mutation
      else if (postData?.query?.includes('InviteFamilyMembers')) {
        const invitations = postData.variables.input.invitations;
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: {
              inviteFamilyMembers: {
                successCount: invitations.length,
                failedCount: 0,
                errors: [],
              },
            },
          }),
        });
      }
      // Let other requests through
      else {
        await route.continue();
      }
    });

    // Wait for route handler registration
    await page.waitForTimeout(50);
  }

  /**
   * Helper function to complete the family creation wizard with invitations
   * Uses GraphQL mocking (temporary until E2E auth is implemented)
   */
  async function completeFamilyCreationWizard(
    page: any,
    options: {
      familyName: string;
      invitations: { email: string; role: 'MEMBER' | 'ADMIN'; message?: string }[];
    }
  ) {
    const { familyName, invitations } = options;

    // Step 1: Navigate to wizard
    await page.goto('/create-family');
    await expect(page.getByRole('heading', { name: 'Create Your Family' })).toBeVisible();

    // Step 2: Fill in family name and click Next
    await page.getByLabel('Family Name').fill(familyName);
    await page.getByRole('button', { name: 'Next' }).click();

    // Step 3: Wait for Step 2 (Invite Members)
    await expect(page.getByRole('heading', { name: 'Invite Family Members' })).toBeVisible();

    // Step 4: Fill in invitations
    for (let i = 0; i < invitations.length; i++) {
      const invitation = invitations[i];

      // Fill in email
      await page.getByLabel(`Email address ${i + 1}`).fill(invitation.email);

      // Select role
      await page.locator(`select[id="role-${i}"]`).selectOption(invitation.role);

      // Fill in message if provided
      if (invitation.message) {
        await page.getByLabel('Personal message (optional)').fill(invitation.message);
      }

      // Add another invitation if not the last one
      if (i < invitations.length - 1) {
        await page.getByRole('button', { name: 'Add another email invitation' }).click();
      }
    }

    // Step 5: Submit wizard (mocked GraphQL responses)
    await page.getByRole('button', { name: 'Create Family' }).click();

    // Step 6: Confirm invitations in dialog
    await expect(page.getByRole('dialog', { name: 'Confirm Invitations' })).toBeVisible();
    await page.getByRole('button', { name: 'Send Invitations' }).click();

    // Step 7: Wait for success
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 10000 });
  }

  test('should send email with correct recipient and subject', async ({ page }) => {
    await test.step('Setup authenticated session and mocks', async () => {
      await setupAuthenticatedSession(page);
      await setupFamilyCreationMocks(page);
    });

    await test.step('Complete wizard with single invitation', async () => {
      await completeFamilyCreationWizard(page, {
        familyName: 'Email Test Family',
        invitations: [{ email: 'invitee@example.com', role: 'MEMBER' }],
      });
    });

    await test.step('Wait for email to arrive in MailHog', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === 'invitee@example.com'),
        5000
      );

      expect(email).not.toBeNull();
    });

    await test.step('Verify email subject', async () => {
      const email = await mailHog.getEmailByRecipient('invitee@example.com');
      expect(email).not.toBeNull();
      expect(email!.Content.Headers.Subject[0]).toContain('invited you');
      expect(email!.Content.Headers.Subject[0]).toContain('Email Test Family');
    });

    await test.step('Verify email recipient', async () => {
      const email = await mailHog.getEmailByRecipient('invitee@example.com');
      expect(email!.To[0].Mailbox).toBe('invitee');
      expect(email!.To[0].Domain).toBe('example.com');
    });
  });

  test('should include personal message in email body', async ({ page }) => {
    await test.step('Setup authenticated session and mocks', async () => {
      await setupAuthenticatedSession(page);
      await setupFamilyCreationMocks(page);
    });
    await test.step('Complete wizard with personal message', async () => {
      const personalMessage = 'Welcome to our family circle! We are excited to have you join us.';

      await completeFamilyCreationWizard(page, {
        familyName: 'Message Test Family',
        invitations: [
          {
            email: 'invitee-message@example.com',
            role: 'MEMBER',
            message: personalMessage,
          },
        ],
      });
    });

    await test.step('Wait for email with personal message', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.Content.Body.includes('Welcome to our family circle'),
        5000
      );

      expect(email).not.toBeNull();
      expect(email!.Content.Body).toContain('Welcome to our family circle!');
      expect(email!.Content.Body).toContain('We are excited to have you join us.');
    });
  });

  test('should include valid invitation token link', async ({ page }) => {
    await test.step('Setup authenticated session and mocks', async () => {
      await setupAuthenticatedSession(page);
      await setupFamilyCreationMocks(page);
    });
    await test.step('Complete wizard with invitation', async () => {
      await completeFamilyCreationWizard(page, {
        familyName: 'Token Test Family',
        invitations: [{ email: 'invitee-token@example.com', role: 'MEMBER' }],
      });
    });

    await test.step('Retrieve email from MailHog', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === 'invitee-token@example.com'),
        5000
      );
      expect(email).not.toBeNull();

      // Extract token from email body
      const token = mailHog.extractInvitationToken(email!);
      expect(token).not.toBeNull();
      expect(token!.length).toBeGreaterThan(10); // Token should be reasonably long
    });

    await test.step('Verify token link format in email', async () => {
      const email = await mailHog.getEmailByRecipient('invitee-token@example.com');
      const urls = mailHog.extractUrls(email!);

      expect(urls.length).toBeGreaterThan(0);
      expect(urls.some((url) => url.includes('/accept-invitation?token='))).toBe(true);
    });
  });

  test('should display ADMIN role correctly in email', async ({ page }) => {
    await test.step('Setup authenticated session and mocks', async () => {
      await setupAuthenticatedSession(page);
      await setupFamilyCreationMocks(page);
    });
    await test.step('Complete wizard with ADMIN role invitation', async () => {
      await completeFamilyCreationWizard(page, {
        familyName: 'Admin Role Family',
        invitations: [{ email: 'admin@example.com', role: 'ADMIN' }],
      });
    });

    await test.step('Verify admin role mentioned in email', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === 'admin@example.com'),
        5000
      );
      expect(email).not.toBeNull();

      const plainTextBody = mailHog.getPlainTextBody(email!);
      expect(plainTextBody.toLowerCase()).toContain('admin');
    });
  });

  test('should display MEMBER role correctly in email', async ({ page }) => {
    await test.step('Setup authenticated session and mocks', async () => {
      await setupAuthenticatedSession(page);
      await setupFamilyCreationMocks(page);
    });
    await test.step('Complete wizard with MEMBER role invitation', async () => {
      await completeFamilyCreationWizard(page, {
        familyName: 'Member Role Family',
        invitations: [{ email: 'member@example.com', role: 'MEMBER' }],
      });
    });

    await test.step('Verify member role mentioned in email', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === 'member@example.com'),
        5000
      );
      expect(email).not.toBeNull();

      const plainTextBody = mailHog.getPlainTextBody(email!);
      expect(plainTextBody.toLowerCase()).toContain('member');
    });
  });

  test('should send multiple emails when inviting multiple members', async ({ page }) => {
    await test.step('Setup authenticated session and mocks', async () => {
      await setupAuthenticatedSession(page);
      await setupFamilyCreationMocks(page);
    });

    await test.step('Complete wizard with 3 invitations', async () => {
      await completeFamilyCreationWizard(page, {
        familyName: 'Multi Invite Family',
        invitations: [
          { email: 'alice@example.com', role: 'ADMIN' },
          { email: 'bob@example.com', role: 'MEMBER' },
          { email: 'charlie@example.com', role: 'MEMBER' },
        ],
      });
    });

    await test.step('Wait for all 3 emails (background service batch processing)', async () => {
      const emails = await mailHog.waitForEmails(
        3,
        (e) => e.Content.Headers.Subject[0].includes('invited you'),
        5000
      );

      expect(emails.length).toBe(3);
    });

    await test.step('Verify each email has correct recipient', async () => {
      const email1 = await mailHog.getEmailByRecipient('alice@example.com');
      const email2 = await mailHog.getEmailByRecipient('bob@example.com');
      const email3 = await mailHog.getEmailByRecipient('charlie@example.com');

      expect(email1).not.toBeNull();
      expect(email2).not.toBeNull();
      expect(email3).not.toBeNull();

      // Verify ADMIN role in Alice's email
      expect(mailHog.getPlainTextBody(email1!).toLowerCase()).toContain('admin');

      // Verify MEMBER role in Bob's and Charlie's emails
      expect(mailHog.getPlainTextBody(email2!).toLowerCase()).toContain('member');
      expect(mailHog.getPlainTextBody(email3!).toLowerCase()).toContain('member');
    });
  });

  test('should include family name in email body', async ({ page }) => {
    await test.step('Setup authenticated session and mocks', async () => {
      await setupAuthenticatedSession(page);
      await setupFamilyCreationMocks(page);
    });
    await test.step('Complete wizard with distinct family name', async () => {
      await completeFamilyCreationWizard(page, {
        familyName: 'The Awesome Smith Family',
        invitations: [{ email: 'family-name-test@example.com', role: 'MEMBER' }],
      });
    });

    await test.step('Verify family name in email', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === 'family-name-test@example.com'),
        5000
      );
      expect(email).not.toBeNull();

      expect(email!.Content.Body).toContain('The Awesome Smith Family');
    });
  });

  test('should include invitation display code in email', async ({ page }) => {
    await test.step('Setup authenticated session and mocks', async () => {
      await setupAuthenticatedSession(page);
      await setupFamilyCreationMocks(page);
    });
    await test.step('Complete wizard with invitation', async () => {
      await completeFamilyCreationWizard(page, {
        familyName: 'Display Code Family',
        invitations: [{ email: 'display-code@example.com', role: 'MEMBER' }],
      });
    });

    await test.step('Verify display code format in email', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === 'display-code@example.com'),
        5000
      );
      expect(email).not.toBeNull();

      // Display code format: ABC-DEF-123 (3 groups of 3 characters separated by hyphens)
      const displayCodeMatch = email!.Content.Body.match(/[A-Z0-9]{3}-[A-Z0-9]{3}-[0-9]{3}/);
      expect(displayCodeMatch).not.toBeNull();
    });
  });

  test('should include sender information in email headers', async ({ page }) => {
    await test.step('Setup authenticated session and mocks', async () => {
      await setupAuthenticatedSession(page);
      await setupFamilyCreationMocks(page);
    });
    await test.step('Complete wizard with invitation', async () => {
      await completeFamilyCreationWizard(page, {
        familyName: 'Sender Test Family',
        invitations: [{ email: 'sender-test@example.com', role: 'MEMBER' }],
      });
    });

    await test.step('Verify sender email address', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === 'sender-test@example.com'),
        5000
      );
      expect(email).not.toBeNull();

      // Verify sender is from FamilyHub
      expect(`${email!.From.Mailbox}@${email!.From.Domain}`).toContain('familyhub');
    });
  });

  test('should have HTML email structure with proper formatting', async ({ page }) => {
    await test.step('Setup authenticated session and mocks', async () => {
      await setupAuthenticatedSession(page);
      await setupFamilyCreationMocks(page);
    });
    await test.step('Complete wizard with invitation', async () => {
      await completeFamilyCreationWizard(page, {
        familyName: 'HTML Test Family',
        invitations: [{ email: 'html-test@example.com', role: 'MEMBER' }],
      });
    });

    await test.step('Verify HTML structure', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === 'html-test@example.com'),
        5000
      );
      expect(email).not.toBeNull();

      // Basic HTML structure checks
      expect(email!.Content.Body).toContain('<html');
      expect(email!.Content.Body).toContain('<body');
      expect(email!.Content.Body).toContain('</html>');

      // Should contain invitation link as anchor tag
      expect(email!.Content.Body).toContain('<a');
      expect(email!.Content.Body).toContain('href=');
    });
  });
});
