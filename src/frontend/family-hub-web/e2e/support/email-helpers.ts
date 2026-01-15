/**
 * MailHog Email Testing Helper
 *
 * Provides programmatic access to MailHog REST API for E2E email verification.
 * MailHog captures emails sent via SMTP (localhost:1025) and exposes them via
 * REST API (localhost:8025).
 *
 * Usage:
 * ```typescript
 * const mailHog = new MailHogClient();
 * await mailHog.clearEmails(); // Clean state before test
 *
 * // Trigger email sending in test
 * // ...
 *
 * // Wait for email to arrive
 * const email = await mailHog.waitForEmail(
 *   e => e.To[0].Mailbox === 'invitee',
 *   5000
 * );
 *
 * expect(email).not.toBeNull();
 * expect(email!.Content.Headers.Subject[0]).toContain('invited you');
 * ```
 */

/**
 * MailHog email address structure
 */
export interface MailHogAddress {
  Mailbox: string;
  Domain: string;
  Params: string;
}

/**
 * MailHog email content structure
 */
export interface MailHogContent {
  Headers: Record<string, string[]>;
  Body: string;
  Size: number;
  MIME: any;
}

/**
 * MailHog MIME structure
 */
export interface MailHogMIME {
  Parts: {
    Headers: Record<string, string[]>;
    Body: string;
    Size: number;
    MIME: any;
  }[];
}

/**
 * MailHog raw email data
 */
export interface MailHogRaw {
  From: string;
  To: string[];
  Data: string;
  Helo: string;
}

/**
 * MailHog email message structure
 * Matches MailHog API v2 response format
 */
export interface MailHogEmail {
  ID: string;
  From: MailHogAddress;
  To: MailHogAddress[];
  Content: MailHogContent;
  Created: string;
  MIME: MailHogMIME | null;
  Raw: MailHogRaw;
}

/**
 * MailHog API response structure
 */
export interface MailHogResponse {
  total: number;
  count: number;
  start: number;
  items: MailHogEmail[];
}

/**
 * MailHog REST API client for E2E email verification
 */
export class MailHogClient {
  private readonly baseUrl: string;

  constructor(baseUrl = 'http://localhost:8025') {
    this.baseUrl = baseUrl;
  }

  /**
   * Get all emails from MailHog inbox
   * @returns Array of emails
   */
  async getEmails(): Promise<MailHogEmail[]> {
    try {
      const response = await fetch(`${this.baseUrl}/api/v2/messages`);
      if (!response.ok) {
        console.error(`MailHog API error: ${response.status} ${response.statusText}`);
        return [];
      }

      const data: MailHogResponse = await response.json();
      return data.items || [];
    } catch (error) {
      console.error('Failed to fetch emails from MailHog:', error);
      return [];
    }
  }

  /**
   * Get email by recipient email address
   * @param email - Full email address (e.g., "user@example.com")
   * @returns Email or null if not found
   */
  async getEmailByRecipient(email: string): Promise<MailHogEmail | null> {
    const emails = await this.getEmails();
    return emails.find((e) => e.To.some((to) => `${to.Mailbox}@${to.Domain}` === email)) || null;
  }

  /**
   * Get email by sender email address
   * @param email - Full email address (e.g., "noreply@familyhub.local")
   * @returns Email or null if not found
   */
  async getEmailBySender(email: string): Promise<MailHogEmail | null> {
    const emails = await this.getEmails();
    return emails.find((e) => `${e.From.Mailbox}@${e.From.Domain}` === email) || null;
  }

  /**
   * Get email by subject line (case-insensitive contains)
   * @param subject - Subject text to search for
   * @returns Email or null if not found
   */
  async getEmailBySubject(subject: string): Promise<MailHogEmail | null> {
    const emails = await this.getEmails();
    return (
      emails.find((e) =>
        e.Content.Headers.Subject?.[0]?.toLowerCase().includes(subject.toLowerCase())
      ) || null
    );
  }

  /**
   * Clear all emails from MailHog inbox
   * Useful for test cleanup to ensure clean state
   */
  async clearEmails(): Promise<void> {
    try {
      const response = await fetch(`${this.baseUrl}/api/v1/messages`, {
        method: 'DELETE',
      });

      if (!response.ok) {
        console.error(`Failed to clear MailHog emails: ${response.status} ${response.statusText}`);
      }
    } catch (error) {
      console.error('Failed to clear MailHog emails:', error);
    }
  }

  /**
   * Wait for an email matching the predicate to arrive
   * Polls MailHog API at 500ms intervals until email found or timeout
   *
   * @param predicate - Function to filter emails
   * @param timeout - Max wait time in milliseconds (default: 5000)
   * @returns Email or null if timeout
   *
   * @example
   * // Wait for invitation email
   * const email = await mailHog.waitForEmail(
   *   e => e.Content.Body.includes('invited you'),
   *   5000
   * );
   */
  async waitForEmail(
    predicate: (email: MailHogEmail) => boolean,
    timeout = 5000
  ): Promise<MailHogEmail | null> {
    const startTime = Date.now();

    while (Date.now() - startTime < timeout) {
      const emails = await this.getEmails();
      const found = emails.find(predicate);

      if (found) {
        console.log(`✅ Email found after ${Date.now() - startTime}ms`);
        return found;
      }

      // Poll every 500ms
      await new Promise((resolve) => setTimeout(resolve, 500));
    }

    console.warn(`⏱️ Timeout: No email found after ${timeout}ms`);
    return null;
  }

  /**
   * Wait for multiple emails matching the predicate
   * Useful for testing batch operations (e.g., multiple invitations)
   *
   * @param count - Expected number of emails
   * @param predicate - Function to filter emails
   * @param timeout - Max wait time in milliseconds (default: 5000)
   * @returns Array of emails (may be less than count if timeout)
   *
   * @example
   * // Wait for 3 invitation emails
   * const emails = await mailHog.waitForEmails(
   *   3,
   *   e => e.Content.Body.includes('invited you'),
   *   10000
   * );
   * expect(emails.length).toBe(3);
   */
  async waitForEmails(
    count: number,
    predicate: (email: MailHogEmail) => boolean,
    timeout = 5000
  ): Promise<MailHogEmail[]> {
    const startTime = Date.now();

    while (Date.now() - startTime < timeout) {
      const emails = await this.getEmails();
      const matching = emails.filter(predicate);

      if (matching.length >= count) {
        console.log(`✅ ${count} emails found after ${Date.now() - startTime}ms`);
        return matching.slice(0, count);
      }

      // Poll every 500ms
      await new Promise((resolve) => setTimeout(resolve, 500));
    }

    console.warn(
      `⏱️ Timeout: Only ${
        (await this.getEmails()).filter(predicate).length
      }/${count} emails found after ${timeout}ms`
    );
    return (await this.getEmails()).filter(predicate);
  }

  /**
   * Extract invitation token from email body
   * Searches for token parameter in URL (e.g., /accept-invitation?token=ABC123)
   *
   * @param email - Email to extract token from
   * @returns Token string or null if not found
   *
   * @example
   * const email = await mailHog.getEmailByRecipient('user@example.com');
   * const token = mailHog.extractInvitationToken(email!);
   * await page.goto(`/accept-invitation?token=${token}`);
   */
  extractInvitationToken(email: MailHogEmail): string | null {
    const tokenMatch = email.Content.Body.match(/token=([a-zA-Z0-9-]+)/);
    return tokenMatch ? tokenMatch[1] : null;
  }

  /**
   * Extract all URLs from email body
   * Useful for verifying link presence and navigating to them
   *
   * @param email - Email to extract URLs from
   * @returns Array of URLs
   */
  extractUrls(email: MailHogEmail): string[] {
    const urlRegex = /https?:\/\/[^\s<>"]+/g;
    const matches = email.Content.Body.match(urlRegex);
    return matches || [];
  }

  /**
   * Get email body as plain text (strips HTML tags)
   * Useful for simpler content assertions
   *
   * @param email - Email to extract text from
   * @returns Plain text body
   */
  getPlainTextBody(email: MailHogEmail): string {
    // Simple HTML tag removal (for basic cases)
    return email.Content.Body.replace(/<[^>]*>/g, '').trim();
  }
}
