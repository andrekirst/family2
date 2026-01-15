/**
 * E2E Test Suite: Invitation Acceptance Flow (Playwright)
 *
 * Tests the complete invitation acceptance experience including:
 * - Public invitation preview (no auth required)
 * - "Accept First" flow (accept → authenticate → auto-acceptance)
 * - Authenticated immediate acceptance
 * - Token validation and error handling
 * - Expired/revoked/already-accepted invitation states
 * - Session storage management for pending tokens
 * - GraphQL query and mutation integration
 * - Toast notifications and navigation
 * - Accessibility compliance (WCAG 2.1 AA)
 * - RabbitMQ event verification (InvitationAcceptedEvent)
 *
 * Test Coverage:
 * - Happy path: Unauthenticated user "Accept First" flow
 * - Happy path: Authenticated user immediate acceptance
 * - Error handling: Invalid token
 * - Error handling: Expired invitation
 * - Error handling: Already accepted invitation
 * - Session storage: Token persistence during auth flow
 * - UI states: Loading, error, success, warning
 * - Accessibility: ARIA labels, keyboard navigation
 * - RabbitMQ events: InvitationAcceptedEvent published on acceptance
 */

import { test, expect } from '@playwright/test';
import { test as rabbitmqTest } from '../fixtures/rabbitmq.fixture';
import { URLS, STORAGE_KEYS, TEST_DATA } from '../support/constants';

test.describe('Invitation Acceptance Flow', () => {
  const validToken = 'valid-test-token-abc123';
  const invalidToken = 'invalid-token-xyz789';
  const expiredToken = 'expired-token-def456';
  const acceptedToken = 'accepted-token-ghi789';

  test.beforeEach(async ({ context }) => {
    // Reset application state
    await context.clearCookies();
    await context.clearPermissions();
  });

  /**
   * Helper function to setup OAuth mocks for authenticated tests
   */
  async function setupAuthenticatedSession(page: any) {
    const mockAccessToken = TEST_DATA.MOCK_ACCESS_TOKEN;
    const mockExpiresAt = new Date(Date.now() + 3600000).toISOString();

    await page.addInitScript(
      ({ token, expires }: { token: string; expires: string }) => {
        window.localStorage.setItem('family_hub_access_token', token);
        window.localStorage.setItem('family_hub_token_expires', expires);
      },
      { token: mockAccessToken, expires: mockExpiresAt }
    );
  }

  /**
   * Helper function to setup GraphQL mocks for invitation queries and mutations
   */
  async function setupInvitationMocks(
    page: any,
    options: {
      tokenToReturn?: string;
      invitationStatus?: 'PENDING' | 'ACCEPTED' | 'EXPIRED' | 'REVOKED';
      familyName?: string;
      memberCount?: number;
      acceptSuccess?: boolean;
      acceptError?: string;
    } = {}
  ) {
    const {
      tokenToReturn = validToken,
      invitationStatus = 'PENDING',
      familyName = 'Test Family',
      memberCount = 3,
      acceptSuccess = true,
      acceptError = '',
    } = options;

    await page.route('http://localhost:5002/graphql', async (route: any) => {
      const request = route.request();
      const postData = request.postDataJSON();

      // GetInvitationByToken query (PUBLIC - no auth)
      if (postData?.query?.includes('GetInvitationByToken')) {
        const requestedToken = postData.variables?.token;

        if (requestedToken === invalidToken) {
          // Invalid token - return null
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              data: {
                invitations: {
                  byToken: null,
                },
              },
            }),
          });
        } else if (requestedToken === tokenToReturn) {
          // Valid token - return invitation details
          const now = new Date();
          const expiresAt =
            invitationStatus === 'EXPIRED'
              ? new Date(now.getTime() - 24 * 60 * 60 * 1000).toISOString() // 1 day ago
              : new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000).toISOString(); // 7 days from now

          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              data: {
                invitations: {
                  byToken: {
                    id: 'invitation-123',
                    email: 'invitee@example.com',
                    role: 'MEMBER',
                    status: invitationStatus,
                    expiresAt: expiresAt,
                    message: 'Welcome to our family!',
                    displayCode: 'ABC-DEF-123',
                    family: {
                      id: 'family-123',
                      name: familyName,
                    },
                    memberCount: memberCount,
                  },
                },
              },
            }),
          });
        } else {
          await route.continue();
        }
      }
      // AcceptInvitation mutation (AUTHENTICATED)
      else if (postData?.query?.includes('AcceptInvitation')) {
        if (acceptSuccess) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              data: {
                acceptInvitation: {
                  familyId: 'family-123',
                  familyName: familyName,
                  role: 'MEMBER',
                  errors: [],
                },
              },
            }),
          });
        } else {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              data: {
                acceptInvitation: {
                  familyId: null,
                  familyName: null,
                  role: null,
                  errors: [
                    {
                      __typename: 'BusinessError',
                      message: acceptError || 'Failed to accept invitation',
                      code: 'INVITATION_ERROR',
                    },
                  ],
                },
              },
            }),
          });
        }
      }
      // GetCurrentFamily query (for post-acceptance family loading)
      else if (postData?.query?.includes('GetCurrentFamily')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: {
              family: {
                id: 'family-123',
                name: familyName,
                createdAt: '2026-01-14T00:00:00Z',
              },
            },
          }),
        });
      }
      // CompleteZitadelLogin mutation (for OAuth callback)
      else if (postData?.query?.includes('CompleteZitadelLogin')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: {
              completeZitadelLogin: {
                authenticationResult: {
                  accessToken: 'new-access-token',
                  expiresAt: new Date(Date.now() + 3600000).toISOString(),
                  user: {
                    id: 'user-123',
                    email: 'test@example.com',
                    emailVerified: true,
                    firstName: 'Test',
                    lastName: 'User',
                  },
                },
                errors: [],
              },
            },
          }),
        });
      } else {
        await route.continue();
      }
    });

    // Wait for route handler to be registered
    await page.waitForTimeout(50);
  }

  test.describe('Happy Path: Unauthenticated "Accept First" Flow', () => {
    test('should show invitation preview, store token, redirect to login, auto-accept after auth', async ({
      page,
    }) => {
      await test.step('Setup mocks (no auth)', async () => {
        await setupInvitationMocks(page, {
          familyName: 'Smith Family',
          memberCount: 3,
        });
      });

      await test.step('Navigate to invitation link', async () => {
        await page.goto(`/accept-invitation?token=${validToken}`);
      });

      await test.step('Verify invitation preview displayed (public - no auth)', async () => {
        // Verify loading state disappears
        await expect(page.getByText('Loading invitation details...')).not.toBeVisible({
          timeout: 5000,
        });

        // Verify invitation details
        await expect(page.getByText("You're Invited!")).toBeVisible();
        await expect(page.getByText('Join Smith Family')).toBeVisible();
        await expect(page.getByText('Smith Family')).toBeVisible();
        await expect(page.getByText(/3 members/)).toBeVisible();
        await expect(page.getByText(/Member/)).toBeVisible(); // Role badge
        await expect(page.getByText('Welcome to our family!')).toBeVisible(); // Personal message
        await expect(page.getByText('ABC-DEF-123')).toBeVisible(); // Display code
      });

      await test.step('Verify "Accept Invitation" button is enabled', async () => {
        const acceptButton = page.getByRole('button', { name: 'Accept Invitation' });
        await expect(acceptButton).toBeEnabled();
      });

      await test.step('Click "Accept Invitation" (not authenticated)', async () => {
        await page.getByRole('button', { name: 'Accept Invitation' }).click();
      });

      await test.step('Verify token stored in sessionStorage', async () => {
        const storedToken = await page.evaluate(() =>
          sessionStorage.getItem('pending_invitation_token')
        );
        expect(storedToken).toBe(validToken);
      });

      await test.step('Verify redirect to login', async () => {
        await expect(page).toHaveURL(/\/login/);
      });

      await test.step('Verify toast notification shown', async () => {
        await expect(page.getByText('Please sign in to accept this invitation')).toBeVisible();
      });

      // Simulate OAuth login flow
      await test.step('Simulate OAuth callback with pending token', async () => {
        // Set up authenticated session
        await setupAuthenticatedSession(page);

        // Navigate to callback with code and state
        await page.goto('/auth/callback?code=test-code&state=test-state');
      });

      await test.step('Verify auto-acceptance and redirect to dashboard', async () => {
        await expect(page).toHaveURL(/\/dashboard/, { timeout: 10000 });
      });

      await test.step('Verify success toast shown', async () => {
        await expect(page.getByText(/Welcome to Smith Family!/)).toBeVisible();
      });

      await test.step('Verify token cleared from sessionStorage', async () => {
        const storedToken = await page.evaluate(() =>
          sessionStorage.getItem('pending_invitation_token')
        );
        expect(storedToken).toBeNull();
      });
    });
  });

  test.describe('Happy Path: Authenticated Immediate Acceptance', () => {
    test('should accept invitation immediately when already authenticated', async ({ page }) => {
      await test.step('Setup mocks (with auth)', async () => {
        await setupAuthenticatedSession(page);
        await setupInvitationMocks(page, {
          familyName: 'Johnson Family',
          memberCount: 5,
        });
      });

      await test.step('Navigate to invitation link', async () => {
        await page.goto(`/accept-invitation?token=${validToken}`);
      });

      await test.step('Verify invitation preview displayed', async () => {
        await expect(page.getByText("You're Invited!")).toBeVisible();
        await expect(page.getByText('Join Johnson Family')).toBeVisible();
      });

      await test.step('Click "Accept Invitation"', async () => {
        await page.getByRole('button', { name: 'Accept Invitation' }).click();
      });

      await test.step('Verify immediate redirect to dashboard', async () => {
        await expect(page).toHaveURL(/\/dashboard/, { timeout: 5000 });
      });

      await test.step('Verify success toast shown', async () => {
        await expect(page.getByText(/Welcome to Johnson Family!/)).toBeVisible();
      });

      await test.step('Verify no token in sessionStorage (direct acceptance)', async () => {
        const storedToken = await page.evaluate(() =>
          sessionStorage.getItem('pending_invitation_token')
        );
        expect(storedToken).toBeNull();
      });
    });
  });

  test.describe('Error Handling: Invalid Token', () => {
    test('should show error for invalid or non-existent token', async ({ page }) => {
      await test.step('Setup mocks', async () => {
        await setupInvitationMocks(page);
      });

      await test.step('Navigate with invalid token', async () => {
        await page.goto(`/accept-invitation?token=${invalidToken}`);
      });

      await test.step('Verify error state displayed', async () => {
        await expect(page.getByText('Invitation Error')).toBeVisible();
        await expect(page.getByText('Invitation not found or has been revoked')).toBeVisible();
      });

      await test.step('Verify "Back to Login" button visible', async () => {
        await expect(page.getByRole('button', { name: 'Back to Login' })).toBeVisible();
      });

      await test.step('Click "Back to Login"', async () => {
        await page.getByRole('button', { name: 'Back to Login' }).click();
      });

      await test.step('Verify redirect to login', async () => {
        await expect(page).toHaveURL(/\/login/);
      });
    });

    test('should show error when token query parameter is missing', async ({ page }) => {
      await test.step('Navigate without token parameter', async () => {
        await page.goto('/accept-invitation');
      });

      await test.step('Verify error displayed', async () => {
        await expect(page.getByText('Missing invitation token')).toBeVisible();
      });
    });
  });

  test.describe('Error Handling: Expired Invitation', () => {
    test('should show warning banner and disable accept button for expired invitation', async ({
      page,
    }) => {
      await test.step('Setup mocks with expired invitation', async () => {
        await setupInvitationMocks(page, {
          tokenToReturn: expiredToken,
          invitationStatus: 'EXPIRED',
          familyName: 'Expired Family',
        });
      });

      await test.step('Navigate to expired invitation', async () => {
        await page.goto(`/accept-invitation?token=${expiredToken}`);
      });

      await test.step('Verify invitation details still visible', async () => {
        await expect(page.getByText("You're Invited!")).toBeVisible();
        await expect(page.getByText('Join Expired Family')).toBeVisible();
      });

      await test.step('Verify red warning banner displayed', async () => {
        await expect(page.getByText('This invitation has expired')).toBeVisible();
        await expect(
          page.getByText('Please contact the family owner for a new invitation')
        ).toBeVisible();
      });

      await test.step('Verify "Accept Invitation" button is disabled', async () => {
        const acceptButton = page.getByRole('button', { name: 'Accept Invitation' });
        await expect(acceptButton).toBeDisabled();
      });

      await test.step('Verify "Cancel" button still works', async () => {
        await page.getByRole('button', { name: 'Cancel' }).click();
        await expect(page).toHaveURL(/\/login/);
      });
    });
  });

  test.describe('Error Handling: Already Accepted Invitation', () => {
    test('should show info banner and disable accept button for already accepted invitation', async ({
      page,
    }) => {
      await test.step('Setup mocks with accepted invitation', async () => {
        await setupInvitationMocks(page, {
          tokenToReturn: acceptedToken,
          invitationStatus: 'ACCEPTED',
          familyName: 'Accepted Family',
        });
      });

      await test.step('Navigate to accepted invitation', async () => {
        await page.goto(`/accept-invitation?token=${acceptedToken}`);
      });

      await test.step('Verify invitation details still visible', async () => {
        await expect(page.getByText("You're Invited!")).toBeVisible();
      });

      await test.step('Verify yellow info banner displayed', async () => {
        await expect(page.getByText('This invitation has already been accepted')).toBeVisible();
      });

      await test.step('Verify "Accept Invitation" button is disabled', async () => {
        const acceptButton = page.getByRole('button', { name: 'Accept Invitation' });
        await expect(acceptButton).toBeDisabled();
      });
    });
  });

  test.describe('Role Display', () => {
    test('should display admin role with purple badge', async ({ page }) => {
      await test.step('Setup mocks with admin role', async () => {
        await setupInvitationMocks(page);

        // Override to set ADMIN role
        await page.route('http://localhost:5002/graphql', async (route: any) => {
          const postData = route.request().postDataJSON();
          if (postData?.query?.includes('GetInvitationByToken')) {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  invitations: {
                    byToken: {
                      id: 'invitation-123',
                      email: 'admin@example.com',
                      role: 'ADMIN',
                      status: 'PENDING',
                      expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
                      message: 'You will be an admin!',
                      displayCode: 'ADMIN-123',
                      family: { id: 'family-123', name: 'Admin Family' },
                      memberCount: 2,
                    },
                  },
                },
              }),
            });
          } else {
            await route.continue();
          }
        });
      });

      await test.step('Navigate to invitation', async () => {
        await page.goto(`/accept-invitation?token=${validToken}`);
      });

      await test.step('Verify admin role badge displayed', async () => {
        await expect(page.getByText('Administrator')).toBeVisible();
      });

      await test.step('Verify admin role description', async () => {
        await expect(page.getByText(/As an Administrator.*manage family members/i)).toBeVisible();
      });
    });

    test('should display member role with green badge', async ({ page }) => {
      await test.step('Setup mocks with member role', async () => {
        await setupInvitationMocks(page);
      });

      await test.step('Navigate to invitation', async () => {
        await page.goto(`/accept-invitation?token=${validToken}`);
      });

      await test.step('Verify member role badge displayed', async () => {
        await expect(page.getByText('Member')).toBeVisible();
      });

      await test.step('Verify member role description', async () => {
        await expect(page.getByText(/As a Member.*access to family features/i)).toBeVisible();
      });
    });
  });

  test.describe('Accessibility', () => {
    test('should have proper ARIA labels and semantic HTML', async ({ page }) => {
      await test.step('Setup mocks', async () => {
        await setupInvitationMocks(page);
      });

      await test.step('Navigate to invitation', async () => {
        await page.goto(`/accept-invitation?token=${validToken}`);
        await expect(page.getByText("You're Invited!")).toBeVisible();
      });

      await test.step('Verify buttons have accessible labels', async () => {
        const acceptButton = page.getByRole('button', { name: 'Accept Invitation' });
        await expect(acceptButton).toBeVisible();

        const cancelButton = page.getByRole('button', { name: 'Cancel' });
        await expect(cancelButton).toBeVisible();
      });

      await test.step('Verify heading hierarchy', async () => {
        const mainHeading = page.locator('h1').filter({ hasText: "You're Invited!" });
        await expect(mainHeading).toBeVisible();
      });
    });

    test('should support keyboard navigation', async ({ page }) => {
      await test.step('Setup mocks with auth', async () => {
        await setupAuthenticatedSession(page);
        await setupInvitationMocks(page);
      });

      await test.step('Navigate to invitation', async () => {
        await page.goto(`/accept-invitation?token=${validToken}`);
        await expect(page.getByText("You're Invited!")).toBeVisible();
      });

      await test.step('Tab to Cancel button', async () => {
        await page.keyboard.press('Tab');
        await expect(page.getByRole('button', { name: 'Cancel' })).toBeFocused();
      });

      await test.step('Tab to Accept button', async () => {
        await page.keyboard.press('Tab');
        await expect(page.getByRole('button', { name: 'Accept Invitation' })).toBeFocused();
      });

      await test.step('Activate Accept button with Enter', async () => {
        await page.keyboard.press('Enter');
        await expect(page).toHaveURL(/\/dashboard/, { timeout: 5000 });
      });
    });
  });

  test.describe('Loading States', () => {
    test('should show loading spinner while fetching invitation', async ({ page }) => {
      await test.step('Setup delayed response', async () => {
        await page.route('http://localhost:5002/graphql', async (route) => {
          const postData = route.request().postDataJSON();

          if (postData?.query?.includes('GetInvitationByToken')) {
            // Delay response by 1 second
            await new Promise((resolve) => setTimeout(resolve, 1000));
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  invitations: {
                    byToken: {
                      id: 'invitation-123',
                      email: 'test@example.com',
                      role: 'MEMBER',
                      status: 'PENDING',
                      expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
                      message: null,
                      displayCode: 'LOAD-123',
                      family: { id: 'family-123', name: 'Loading Family' },
                      memberCount: 1,
                    },
                  },
                },
              }),
            });
          } else {
            await route.continue();
          }
        });
      });

      await test.step('Navigate to invitation', async () => {
        await page.goto(`/accept-invitation?token=${validToken}`);
      });

      await test.step('Verify loading spinner visible', async () => {
        await expect(page.getByText('Loading invitation details...')).toBeVisible();
      });

      await test.step('Verify invitation displayed after loading', async () => {
        await expect(page.getByText("You're Invited!")).toBeVisible({ timeout: 3000 });
        await expect(page.getByText('Loading invitation details...')).not.toBeVisible();
      });
    });

    test('should show loading state on Accept button when accepting', async ({ page }) => {
      await test.step('Setup authenticated session and delayed acceptance', async () => {
        await setupAuthenticatedSession(page);

        await page.route('http://localhost:5002/graphql', async (route) => {
          const postData = route.request().postDataJSON();

          if (postData?.query?.includes('GetInvitationByToken')) {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  invitations: {
                    byToken: {
                      id: 'invitation-123',
                      email: 'test@example.com',
                      role: 'MEMBER',
                      status: 'PENDING',
                      expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
                      message: null,
                      displayCode: 'BTN-123',
                      family: { id: 'family-123', name: 'Button Family' },
                      memberCount: 1,
                    },
                  },
                },
              }),
            });
          } else if (postData?.query?.includes('AcceptInvitation')) {
            // Delay acceptance
            await new Promise((resolve) => setTimeout(resolve, 1000));
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  acceptInvitation: {
                    familyId: 'family-123',
                    familyName: 'Button Family',
                    role: 'MEMBER',
                    errors: [],
                  },
                },
              }),
            });
          } else if (postData?.query?.includes('GetCurrentFamily')) {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  family: {
                    id: 'family-123',
                    name: 'Button Family',
                    createdAt: '2026-01-14T00:00:00Z',
                  },
                },
              }),
            });
          } else {
            await route.continue();
          }
        });
      });

      await test.step('Navigate and click accept', async () => {
        await page.goto(`/accept-invitation?token=${validToken}`);
        await expect(page.getByText("You're Invited!")).toBeVisible();
        await page.getByRole('button', { name: 'Accept Invitation' }).click();
      });

      await test.step('Verify button shows loading state', async () => {
        await expect(page.getByRole('button', { name: 'Accepting...' })).toBeVisible();
      });

      await test.step('Verify button disabled during acceptance', async () => {
        const acceptButton = page.getByRole('button', { name: /Accept/ });
        await expect(acceptButton).toBeDisabled();
      });
    });
  });
});

/**
 * RabbitMQ Event Verification Tests
 *
 * These tests verify that the backend publishes the correct domain events
 * when an invitation is accepted. This enables event chain automation
 * (e.g., sending welcome emails, updating dashboards, triggering workflows).
 *
 * Events Verified:
 * - InvitationAcceptedEvent: Published when user accepts invitation
 *   - Contains familyId, acceptedByUserId, role
 *   - Triggers downstream automation (welcome email, analytics, etc.)
 *
 * RabbitMQ Fixture:
 * - Automatically connects to localhost:5672 (RabbitMQ from Docker Compose)
 * - Binds to familyhub.test exchange (test-specific routing)
 * - Provides waitForMessage() helper with timeout support
 * - Auto-cleanup after each test
 */
rabbitmqTest.describe('RabbitMQ Event Verification', () => {
  const validToken = 'valid-test-token-abc123';

  /**
   * Helper function to setup authenticated session with RabbitMQ test
   */
  async function setupAuthenticatedSessionForRabbitMQ(page: any) {
    const mockAccessToken = TEST_DATA.MOCK_ACCESS_TOKEN;
    const mockExpiresAt = new Date(Date.now() + 3600000).toISOString();

    await page.addInitScript(
      ({ token, expires }: { token: string; expires: string }) => {
        window.localStorage.setItem('family_hub_access_token', token);
        window.localStorage.setItem('family_hub_token_expires', expires);
      },
      { token: mockAccessToken, expires: mockExpiresAt }
    );
  }

  /**
   * Helper function to setup GraphQL mocks for RabbitMQ tests
   */
  async function setupInvitationMocksForRabbitMQ(page: any) {
    await page.route('http://localhost:5002/graphql', async (route: any) => {
      const request = route.request();
      const postData = request.postDataJSON();

      // GetInvitationByToken query
      if (postData?.query?.includes('GetInvitationByToken')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: {
              invitations: {
                byToken: {
                  id: 'invitation-rabbitmq-123',
                  email: 'rabbitmq@example.com',
                  role: 'MEMBER',
                  status: 'PENDING',
                  expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
                  message: 'Welcome to RabbitMQ test family!',
                  displayCode: 'RMQ-123',
                  family: {
                    id: 'family-rabbitmq-123',
                    name: 'RabbitMQ Test Family',
                  },
                  memberCount: 2,
                },
              },
            },
          }),
        });
      }
      // AcceptInvitation mutation
      else if (postData?.query?.includes('AcceptInvitation')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: {
              acceptInvitation: {
                familyId: 'family-rabbitmq-123',
                familyName: 'RabbitMQ Test Family',
                role: 'MEMBER',
                errors: [],
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
              family: {
                id: 'family-rabbitmq-123',
                name: 'RabbitMQ Test Family',
                createdAt: '2026-01-15T00:00:00Z',
              },
            },
          }),
        });
      } else {
        await route.continue();
      }
    });

    // Wait for route handler to be registered
    await page.waitForTimeout(50);
  }

  rabbitmqTest(
    'should publish InvitationAcceptedEvent when authenticated user accepts invitation',
    async ({ page, rabbitmq }) => {
      await rabbitmqTest.step('Setup authenticated session and mocks', async () => {
        await setupAuthenticatedSessionForRabbitMQ(page);
        await setupInvitationMocksForRabbitMQ(page);
      });

      await rabbitmqTest.step('Navigate to invitation and accept', async () => {
        await page.goto(`/accept-invitation?token=${validToken}`);
        await expect(page.getByText("You're Invited!")).toBeVisible();
        await page.getByRole('button', { name: 'Accept Invitation' }).click();
      });

      await rabbitmqTest.step('Wait for InvitationAcceptedEvent', async () => {
        const event = await rabbitmq.waitForMessage(
          (msg) =>
            msg.eventType === 'InvitationAcceptedEvent' &&
            msg.data.familyId === 'family-rabbitmq-123',
          5000
        );

        expect(event).not.toBeNull();
        expect(event!.data.familyId).toBe('family-rabbitmq-123');
        expect(event!.data.acceptedByUserId).toBeTruthy();
        expect(event!.data.role).toBe('MEMBER');
      });

      await rabbitmqTest.step('Verify navigation to dashboard after acceptance', async () => {
        await expect(page).toHaveURL(/\/dashboard/, { timeout: 5000 });
      });
    }
  );

  rabbitmqTest(
    'should publish both FamilyMemberAddedEvent and InvitationRemovedEvent on acceptance',
    async ({ page, rabbitmq }) => {
      await rabbitmqTest.step('Setup authenticated session and mocks', async () => {
        await setupAuthenticatedSessionForRabbitMQ(page);
        await setupInvitationMocksForRabbitMQ(page);
      });

      await rabbitmqTest.step('Navigate to invitation and accept', async () => {
        await page.goto(`/accept-invitation?token=${validToken}`);
        await expect(page.getByText("You're Invited!")).toBeVisible();
        await page.getByRole('button', { name: 'Accept Invitation' }).click();
      });

      await rabbitmqTest.step('Wait for FamilyMemberAddedEvent', async () => {
        const memberAddedEvent = await rabbitmq.waitForMessage(
          (msg) =>
            msg.eventType === 'FamilyMemberAddedEvent' &&
            msg.data.familyId === 'family-rabbitmq-123',
          5000
        );

        expect(memberAddedEvent).not.toBeNull();
        expect(memberAddedEvent!.data.familyId).toBe('family-rabbitmq-123');
        expect(memberAddedEvent!.data.member).toBeTruthy();
        expect(memberAddedEvent!.data.member.email).toBe('rabbitmq@example.com');
        expect(memberAddedEvent!.data.member.role).toBe('MEMBER');
      });

      await rabbitmqTest.step('Wait for InvitationRemovedEvent', async () => {
        const invitationRemovedEvent = await rabbitmq.waitForMessage(
          (msg) =>
            msg.eventType === 'InvitationRemovedEvent' &&
            msg.data.familyId === 'family-rabbitmq-123',
          5000
        );

        expect(invitationRemovedEvent).not.toBeNull();
        expect(invitationRemovedEvent!.data.familyId).toBe('family-rabbitmq-123');
        expect(invitationRemovedEvent!.data.token).toBe(validToken);
      });

      await rabbitmqTest.step('Verify navigation to dashboard', async () => {
        await expect(page).toHaveURL(/\/dashboard/, { timeout: 5000 });
      });
    }
  );

  rabbitmqTest(
    'should NOT publish events when invitation acceptance fails (business error)',
    async ({ page, rabbitmq }) => {
      await rabbitmqTest.step('Setup authenticated session with failing acceptance', async () => {
        await setupAuthenticatedSessionForRabbitMQ(page);

        // Override to return business error
        await page.route('http://localhost:5002/graphql', async (route: any) => {
          const postData = route.request().postDataJSON();

          if (postData?.query?.includes('GetInvitationByToken')) {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  invitations: {
                    byToken: {
                      id: 'invitation-fail-123',
                      email: 'fail@example.com',
                      role: 'MEMBER',
                      status: 'PENDING',
                      expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
                      message: null,
                      displayCode: 'FAIL-123',
                      family: { id: 'family-fail-123', name: 'Fail Family' },
                      memberCount: 1,
                    },
                  },
                },
              }),
            });
          } else if (postData?.query?.includes('AcceptInvitation')) {
            // Return business error (e.g., invitation already accepted by someone else)
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                data: {
                  acceptInvitation: {
                    familyId: null,
                    familyName: null,
                    role: null,
                    errors: [
                      {
                        __typename: 'BusinessError',
                        message: 'This invitation has already been accepted',
                        code: 'INVITATION_ALREADY_ACCEPTED',
                      },
                    ],
                  },
                },
              }),
            });
          } else {
            await route.continue();
          }
        });
      });

      await rabbitmqTest.step('Navigate to invitation and attempt accept', async () => {
        await page.goto(`/accept-invitation?token=${validToken}`);
        await expect(page.getByText("You're Invited!")).toBeVisible();
        await page.getByRole('button', { name: 'Accept Invitation' }).click();
      });

      await rabbitmqTest.step('Verify error toast displayed', async () => {
        await expect(page.getByText('This invitation has already been accepted')).toBeVisible();
      });

      await rabbitmqTest.step('Verify NO events published on failure', async () => {
        // Wait a bit to ensure no events are published
        await page.waitForTimeout(2000);

        const allEvents = await rabbitmq.consumeMessages();
        const invitationEvents = allEvents.filter(
          (e) =>
            e.eventType === 'InvitationAcceptedEvent' ||
            e.eventType === 'FamilyMemberAddedEvent' ||
            e.eventType === 'InvitationRemovedEvent'
        );

        expect(invitationEvents.length).toBe(0);
      });

      await rabbitmqTest.step('Verify user NOT redirected (stays on invitation page)', async () => {
        await expect(page).toHaveURL(/\/accept-invitation/);
      });
    }
  );
});
