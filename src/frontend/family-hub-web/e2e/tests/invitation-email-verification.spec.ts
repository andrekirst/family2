/**
 * E2E Test Suite: Invitation Email Verification (Playwright + MailHog)
 *
 * **Issue #91 IMPLEMENTED** - These tests use header-based authentication.
 *
 * **How it Works:**
 * With FAMILYHUB_TEST_MODE=true, the backend accepts X-Test-User-Id and
 * X-Test-User-Email headers instead of requiring valid JWT tokens.
 * This enables full end-to-end testing without Zitadel OAuth.
 *
 * **Test Flow:**
 * 1. Use graphqlClient fixture (authenticated with test headers)
 * 2. Call real backend GraphQL API (CreateFamily, InviteFamilyMembers)
 * 3. Backend creates invitations in database
 * 4. Background service sends emails to SMTP
 * 5. MailHog captures emails for verification
 *
 * **Also Covered by Backend Integration Tests:**
 * `src/api/tests/FamilyHub.Tests.Integration/Family/Infrastructure/InvitationEmailIntegrationTests.cs`
 *
 * @see Issue #91 - E2E Authentication for API-First Testing
 */

import { test, expect } from '../fixtures/auth.fixture';
import { MailHogClient } from '../support/email-helpers';
import {
  createFamilyViaAPI,
  inviteFamilyMembersViaAPI,
  getCurrentFamilyViaAPI,
} from '../support/api-helpers';
import { TEST_USERS } from '../support/constants';

/**
 * Email verification tests using real backend authentication
 *
 * These tests verify the complete invitation email flow:
 * CreateFamily → InviteFamilyMembers → EmailOutbox → SMTP → MailHog
 */
test.describe('Invitation Email Verification', () => {
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
   * Helper to ensure test user has a family (creates one if needed)
   * Returns the family ID for use in invitation tests
   */
  async function ensureFamilyExists(
    graphqlClient: any,
    familyName: string
  ): Promise<{ familyId: string; familyName: string }> {
    // Check if user already has a family
    const existingFamily = await getCurrentFamilyViaAPI(graphqlClient);
    if (existingFamily) {
      console.log(`Using existing family: ${existingFamily.name} (${existingFamily.id})`);
      return { familyId: existingFamily.id, familyName: existingFamily.name };
    }

    // Create a new family
    const family = await createFamilyViaAPI(graphqlClient, familyName);
    console.log(`Created new family: ${family.name} (${family.id})`);
    return { familyId: family.id, familyName: family.name };
  }

  test('should send email with correct recipient and subject', async ({ graphqlClient }) => {
    const timestamp = Date.now();
    const testFamilyName = `Email Test Family ${timestamp}`;
    const inviteeEmail = `invitee-${timestamp}@example.com`;

    await test.step('Create family and send invitation via API', async () => {
      const { familyId, familyName } = await ensureFamilyExists(graphqlClient, testFamilyName);

      const result = await inviteFamilyMembersViaAPI(graphqlClient, familyId, [
        { email: inviteeEmail, role: 'MEMBER' },
      ]);

      expect(result.successfulInvitations).toHaveLength(1);
      expect(result.failedInvitations).toHaveLength(0);
      console.log(`✅ Invitation sent to ${inviteeEmail}`);
    });

    await test.step('Wait for email to arrive in MailHog', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === inviteeEmail),
        10000 // Longer timeout for real email pipeline
      );

      expect(email).not.toBeNull();
      console.log('✅ Email received in MailHog');
    });

    await test.step('Verify email subject', async () => {
      const email = await mailHog.getEmailByRecipient(inviteeEmail);
      expect(email).not.toBeNull();
      expect(email!.Content.Headers.Subject[0]).toContain('invited you');
    });

    await test.step('Verify email recipient', async () => {
      const email = await mailHog.getEmailByRecipient(inviteeEmail);
      const [mailbox, domain] = inviteeEmail.split('@');
      expect(email!.To[0].Mailbox).toBe(mailbox);
      expect(email!.To[0].Domain).toBe(domain);
    });
  });

  test('should include personal message in email body', async ({ graphqlClient }) => {
    const timestamp = Date.now();
    const testFamilyName = `Message Test Family ${timestamp}`;
    const inviteeEmail = `invitee-msg-${timestamp}@example.com`;
    const personalMessage = 'Welcome to our family circle! We are excited to have you join us.';

    await test.step('Create family and send invitation with personal message', async () => {
      const { familyId } = await ensureFamilyExists(graphqlClient, testFamilyName);

      const result = await inviteFamilyMembersViaAPI(
        graphqlClient,
        familyId,
        [{ email: inviteeEmail, role: 'MEMBER' }],
        personalMessage
      );

      expect(result.successfulInvitations).toHaveLength(1);
    });

    await test.step('Wait for email with personal message', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.Content.Body.includes('Welcome to our family circle'),
        10000
      );

      expect(email).not.toBeNull();
      expect(email!.Content.Body).toContain('Welcome to our family circle!');
      expect(email!.Content.Body).toContain('We are excited to have you join us.');
    });
  });

  test('should include valid invitation token link', async ({ graphqlClient }) => {
    const timestamp = Date.now();
    const testFamilyName = `Token Test Family ${timestamp}`;
    const inviteeEmail = `invitee-token-${timestamp}@example.com`;

    await test.step('Create family and send invitation', async () => {
      const { familyId } = await ensureFamilyExists(graphqlClient, testFamilyName);

      const result = await inviteFamilyMembersViaAPI(graphqlClient, familyId, [
        { email: inviteeEmail, role: 'MEMBER' },
      ]);

      expect(result.successfulInvitations).toHaveLength(1);
      // API returns the token directly - we can verify it matches email
      const apiToken = result.successfulInvitations[0].token;
      expect(apiToken).toBeTruthy();
      console.log(`API returned token: ${apiToken.substring(0, 10)}...`);
    });

    await test.step('Retrieve email from MailHog', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === inviteeEmail),
        10000
      );
      expect(email).not.toBeNull();

      // Extract token from email body
      const token = mailHog.extractInvitationToken(email!);
      expect(token).not.toBeNull();
      expect(token!.length).toBeGreaterThan(10); // Token should be reasonably long
    });

    await test.step('Verify token link format in email', async () => {
      const email = await mailHog.getEmailByRecipient(inviteeEmail);
      const urls = mailHog.extractUrls(email!);

      expect(urls.length).toBeGreaterThan(0);
      expect(urls.some((url) => url.includes('/accept-invitation?token='))).toBe(true);
    });
  });

  test('should display ADMIN role correctly in email', async ({ graphqlClient }) => {
    const timestamp = Date.now();
    const testFamilyName = `Admin Role Family ${timestamp}`;
    const inviteeEmail = `admin-role-${timestamp}@example.com`;

    await test.step('Create family and send ADMIN invitation', async () => {
      const { familyId } = await ensureFamilyExists(graphqlClient, testFamilyName);

      const result = await inviteFamilyMembersViaAPI(graphqlClient, familyId, [
        { email: inviteeEmail, role: 'ADMIN' },
      ]);

      expect(result.successfulInvitations).toHaveLength(1);
      expect(result.successfulInvitations[0].role).toBe('ADMIN');
    });

    await test.step('Verify admin role mentioned in email', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === inviteeEmail),
        10000
      );
      expect(email).not.toBeNull();

      const plainTextBody = mailHog.getPlainTextBody(email!);
      expect(plainTextBody.toLowerCase()).toContain('admin');
    });
  });

  test('should display MEMBER role correctly in email', async ({ graphqlClient }) => {
    const timestamp = Date.now();
    const testFamilyName = `Member Role Family ${timestamp}`;
    const inviteeEmail = `member-role-${timestamp}@example.com`;

    await test.step('Create family and send MEMBER invitation', async () => {
      const { familyId } = await ensureFamilyExists(graphqlClient, testFamilyName);

      const result = await inviteFamilyMembersViaAPI(graphqlClient, familyId, [
        { email: inviteeEmail, role: 'MEMBER' },
      ]);

      expect(result.successfulInvitations).toHaveLength(1);
      expect(result.successfulInvitations[0].role).toBe('MEMBER');
    });

    await test.step('Verify member role mentioned in email', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === inviteeEmail),
        10000
      );
      expect(email).not.toBeNull();

      const plainTextBody = mailHog.getPlainTextBody(email!);
      expect(plainTextBody.toLowerCase()).toContain('member');
    });
  });

  test('should send multiple emails when inviting multiple members', async ({ graphqlClient }) => {
    const timestamp = Date.now();
    const testFamilyName = `Multi Invite Family ${timestamp}`;
    const invitees = [
      { email: `alice-${timestamp}@example.com`, role: 'ADMIN' as const },
      { email: `bob-${timestamp}@example.com`, role: 'MEMBER' as const },
      { email: `charlie-${timestamp}@example.com`, role: 'MEMBER' as const },
    ];

    await test.step('Create family and send batch invitations', async () => {
      const { familyId } = await ensureFamilyExists(graphqlClient, testFamilyName);

      const result = await inviteFamilyMembersViaAPI(graphqlClient, familyId, invitees);

      expect(result.successfulInvitations).toHaveLength(3);
      expect(result.failedInvitations).toHaveLength(0);
      console.log('✅ All 3 invitations sent successfully');
    });

    await test.step('Wait for all 3 emails (background service batch processing)', async () => {
      const emails = await mailHog.waitForEmails(
        3,
        (e) => e.Content.Headers.Subject[0].includes('invited you'),
        15000 // Longer timeout for batch processing
      );

      expect(emails.length).toBe(3);
    });

    await test.step('Verify each email has correct recipient', async () => {
      const email1 = await mailHog.getEmailByRecipient(invitees[0].email);
      const email2 = await mailHog.getEmailByRecipient(invitees[1].email);
      const email3 = await mailHog.getEmailByRecipient(invitees[2].email);

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

  test('should include family name in email body', async ({ graphqlClient }) => {
    const timestamp = Date.now();
    const testFamilyName = `The Awesome Smith Family ${timestamp}`;
    const inviteeEmail = `family-name-${timestamp}@example.com`;

    await test.step('Create family with distinct name and send invitation', async () => {
      const { familyId, familyName } = await ensureFamilyExists(graphqlClient, testFamilyName);

      const result = await inviteFamilyMembersViaAPI(graphqlClient, familyId, [
        { email: inviteeEmail, role: 'MEMBER' },
      ]);

      expect(result.successfulInvitations).toHaveLength(1);
    });

    await test.step('Verify family name in email', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === inviteeEmail),
        10000
      );
      expect(email).not.toBeNull();

      // Family name should appear in email body
      expect(email!.Content.Body).toContain('The Awesome Smith Family');
    });
  });

  test('should include invitation display code in email', async ({ graphqlClient }) => {
    const timestamp = Date.now();
    const testFamilyName = `Display Code Family ${timestamp}`;
    const inviteeEmail = `display-code-${timestamp}@example.com`;

    await test.step('Create family and send invitation', async () => {
      const { familyId } = await ensureFamilyExists(graphqlClient, testFamilyName);

      const result = await inviteFamilyMembersViaAPI(graphqlClient, familyId, [
        { email: inviteeEmail, role: 'MEMBER' },
      ]);

      expect(result.successfulInvitations).toHaveLength(1);
      // API returns display code - verify format
      const displayCode = result.successfulInvitations[0].displayCode;
      expect(displayCode).toMatch(/^[A-Z0-9]{3}-[A-Z0-9]{3}-[0-9]{3}$/);
      console.log(`API returned display code: ${displayCode}`);
    });

    await test.step('Verify display code format in email', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === inviteeEmail),
        10000
      );
      expect(email).not.toBeNull();

      // Display code format: ABC-DEF-123 (3 groups of 3 characters separated by hyphens)
      const displayCodeMatch = email!.Content.Body.match(/[A-Z0-9]{3}-[A-Z0-9]{3}-[0-9]{3}/);
      expect(displayCodeMatch).not.toBeNull();
    });
  });

  test('should include sender information in email headers', async ({ graphqlClient }) => {
    const timestamp = Date.now();
    const testFamilyName = `Sender Test Family ${timestamp}`;
    const inviteeEmail = `sender-test-${timestamp}@example.com`;

    await test.step('Create family and send invitation', async () => {
      const { familyId } = await ensureFamilyExists(graphqlClient, testFamilyName);

      await inviteFamilyMembersViaAPI(graphqlClient, familyId, [
        { email: inviteeEmail, role: 'MEMBER' },
      ]);
    });

    await test.step('Verify sender email address', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === inviteeEmail),
        10000
      );
      expect(email).not.toBeNull();

      // Verify sender is from FamilyHub
      expect(`${email!.From.Mailbox}@${email!.From.Domain}`).toContain('familyhub');
    });
  });

  test('should have HTML email structure with proper formatting', async ({ graphqlClient }) => {
    const timestamp = Date.now();
    const testFamilyName = `HTML Test Family ${timestamp}`;
    const inviteeEmail = `html-test-${timestamp}@example.com`;

    await test.step('Create family and send invitation', async () => {
      const { familyId } = await ensureFamilyExists(graphqlClient, testFamilyName);

      await inviteFamilyMembersViaAPI(graphqlClient, familyId, [
        { email: inviteeEmail, role: 'MEMBER' },
      ]);
    });

    await test.step('Verify HTML structure', async () => {
      const email = await mailHog.waitForEmail(
        (e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === inviteeEmail),
        10000
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

/**
 * Additional tests for edge cases and error handling
 */
test.describe('Invitation Email Edge Cases', () => {
  let mailHog: MailHogClient;

  test.beforeEach(async ({ context }) => {
    mailHog = new MailHogClient();
    await mailHog.clearEmails();
    await context.clearCookies();
  });

  test('should handle duplicate invitation gracefully', async ({ graphqlClient }) => {
    const timestamp = Date.now();
    const testFamilyName = `Duplicate Test Family ${timestamp}`;
    const inviteeEmail = `duplicate-${timestamp}@example.com`;

    await test.step('Create family and send first invitation', async () => {
      // First, check if user has a family, create if not
      let family = await getCurrentFamilyViaAPI(graphqlClient);
      if (!family) {
        family = await createFamilyViaAPI(graphqlClient, testFamilyName);
      }

      const result = await inviteFamilyMembersViaAPI(graphqlClient, family.id, [
        { email: inviteeEmail, role: 'MEMBER' },
      ]);

      expect(result.successfulInvitations).toHaveLength(1);
    });

    await test.step('Attempt duplicate invitation - should fail', async () => {
      const family = await getCurrentFamilyViaAPI(graphqlClient);
      expect(family).not.toBeNull();

      const result = await inviteFamilyMembersViaAPI(graphqlClient, family!.id, [
        { email: inviteeEmail, role: 'MEMBER' },
      ]);

      // Duplicate should be in failed invitations
      expect(result.failedInvitations).toHaveLength(1);
      expect(result.failedInvitations[0].email).toBe(inviteeEmail);
      console.log(`✅ Duplicate correctly rejected: ${result.failedInvitations[0].errorMessage}`);
    });
  });

  test('should reject invalid email format', async ({ graphqlClient }) => {
    const timestamp = Date.now();
    const testFamilyName = `Invalid Email Family ${timestamp}`;

    await test.step('Create family and attempt invalid email invitation', async () => {
      let family = await getCurrentFamilyViaAPI(graphqlClient);
      if (!family) {
        family = await createFamilyViaAPI(graphqlClient, testFamilyName);
      }

      // This should throw due to Vogen validation on backend
      try {
        await inviteFamilyMembersViaAPI(graphqlClient, family.id, [
          { email: 'not-an-email', role: 'MEMBER' },
        ]);
        // If we get here, test should fail
        expect(true).toBe(false); // Force failure
      } catch (error: any) {
        // Expected - invalid email should be rejected
        expect(error.message).toContain('Invalid');
        console.log('✅ Invalid email correctly rejected');
      }
    });
  });
});
