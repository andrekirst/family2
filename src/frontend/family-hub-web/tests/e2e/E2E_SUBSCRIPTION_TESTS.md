# E2E Subscription Tests Strategy

**Status:** Phase 5 - Documentation Complete (Implementation Deferred)
**Issue:** #84 - GraphQL Subscriptions for Real-Time Family Updates

---

## Overview

This document outlines the E2E testing strategy for GraphQL subscriptions using Playwright. These tests validate the complete WebSocket subscription flow from frontend to backend.

## Test Scenarios

### 1. Family Members Subscription

**File:** `tests/e2e/subscriptions/family-members-subscription.spec.ts`

```typescript
import { test, expect } from '@playwright/test';
import { GraphQLHelper } from '../helpers/graphql-helper';

test.describe('Family Members Subscription', () => {
  let graphqlHelper: GraphQLHelper;

  test.beforeEach(async ({ page }) => {
    graphqlHelper = new GraphQLHelper(page);
    await graphqlHelper.authenticateUser('testuser@example.com');
  });

  test('should receive ADDED event when member accepts invitation', async ({ page, context }) => {
    // Arrange: Subscribe to family members changes
    const familyId = '123e4567-e89b-12d3-a456-426614174000';
    const subscriptionPromise = graphqlHelper.subscribe(`
      subscription {
        familyMembersChanged(familyId: "${familyId}") {
          changeType
          member {
            userId
            email
            role
            joinedAt
          }
        }
      }
    `);

    // Act: Open new tab and accept invitation
    const invitationPage = await context.newPage();
    const invitationHelper = new GraphQLHelper(invitationPage);
    await invitationHelper.authenticateUser('newmember@example.com');

    await invitationHelper.mutate(`
      mutation {
        acceptInvitation(input: { token: "test-token-123" }) {
          familyId
          familyName
          role
        }
      }
    `);

    // Assert: Original page receives ADDED event
    const event = await subscriptionPromise;
    expect(event.changeType).toBe('ADDED');
    expect(event.member.email).toBe('newmember@example.com');
    expect(event.member.role).toBe('MEMBER');
  });

  test('should NOT receive events for unauthorized family', async ({ page }) => {
    // Arrange: Subscribe to family user is not member of
    const unauthorizedFamilyId = '999e9999-e99b-99d9-a999-999999999999';

    let receivedEvents = 0;
    const subscriptionPromise = graphqlHelper.subscribe(
      `
        subscription {
          familyMembersChanged(familyId: "${unauthorizedFamilyId}") {
            changeType
          }
        }
      `,
      (event) => { receivedEvents++; }
    );

    // Wait a bit to ensure subscription is active
    await page.waitForTimeout(1000);

    // Assert: Subscription terminates immediately (yield break)
    expect(receivedEvents).toBe(0);
  });

  test('should handle multiple concurrent subscriptions', async ({ page }) => {
    // Arrange: Subscribe to two different families
    const family1Id = '111e1111-e11b-11d1-a111-111111111111';
    const family2Id = '222e2222-e22b-22d2-a222-222222222222';

    const events1: any[] = [];
    const events2: any[] = [];

    const subscription1 = graphqlHelper.subscribe(
      `subscription { familyMembersChanged(familyId: "${family1Id}") { changeType } }`,
      (event) => events1.push(event)
    );

    const subscription2 = graphqlHelper.subscribe(
      `subscription { familyMembersChanged(familyId: "${family2Id}") { changeType } }`,
      (event) => events2.push(event)
    );

    // Act: Trigger events in both families
    // (via API calls or separate browser contexts)

    // Assert: Each subscription receives only its family's events
    expect(events1).toHaveLength(1);
    expect(events2).toHaveLength(1);
  });
});
```

### 2. Pending Invitations Subscription

**File:** `tests/e2e/subscriptions/pending-invitations-subscription.spec.ts`

```typescript
test.describe('Pending Invitations Subscription', () => {
  test('should receive ADDED event when invitation created', async ({ page, context }) => {
    // Arrange: Subscribe as OWNER/ADMIN
    const familyId = '123e4567-e89b-12d3-a456-426614174000';
    const subscriptionPromise = graphqlHelper.subscribe(`
      subscription {
        pendingInvitationsChanged(familyId: "${familyId}") {
          changeType
          invitation {
            email
            role
            expiresAt
          }
        }
      }
    `);

    // Act: Create invitation
    await graphqlHelper.mutate(`
      mutation {
        inviteFamilyMembersByEmail(input: {
          familyId: "${familyId}"
          invitations: [{
            email: "newuser@example.com"
            role: MEMBER
            message: "Welcome!"
          }]
        }) {
          successCount
        }
      }
    `);

    // Assert: Receive ADDED event
    const event = await subscriptionPromise;
    expect(event.changeType).toBe('ADDED');
    expect(event.invitation.email).toBe('newuser@example.com');
  });

  test('should receive REMOVED event when invitation canceled', async ({ page }) => {
    // Similar pattern for invitation cancellation
  });

  test('should NOT allow MEMBER role to subscribe', async ({ page }) => {
    // Arrange: Authenticate as MEMBER (not OWNER/ADMIN)
    await graphqlHelper.authenticateUser('member@example.com');
    const familyId = '123e4567-e89b-12d3-a456-426614174000';

    let receivedEvents = 0;
    await graphqlHelper.subscribe(
      `subscription { pendingInvitationsChanged(familyId: "${familyId}") { changeType } }`,
      (event) => { receivedEvents++; }
    );

    // Wait to ensure subscription is terminated
    await page.waitForTimeout(1000);

    // Assert: No events received (yield break for unauthorized role)
    expect(receivedEvents).toBe(0);
  });
});
```

### 3. WebSocket Connection Management

**File:** `tests/e2e/subscriptions/websocket-connection.spec.ts`

```typescript
test.describe('WebSocket Connection Management', () => {
  test('should establish WebSocket connection with JWT', async ({ page }) => {
    // Arrange
    const jwtToken = await graphqlHelper.getAuthToken();

    // Act: Connect to GraphQL with WebSocket
    await page.goto('http://localhost:4200/family/dashboard');

    // Assert: WebSocket connection established
    const wsConnection = await page.waitForEvent('websocket', {
      predicate: ws => ws.url().includes('/graphql')
    });
    expect(wsConnection).toBeDefined();
  });

  test('should reconnect after connection loss', async ({ page }) => {
    // Arrange: Establish subscription
    await graphqlHelper.subscribe('subscription { ... }');

    // Act: Simulate network interruption
    await page.route('**/graphql', route => route.abort());
    await page.waitForTimeout(1000);
    await page.unroute('**/graphql');

    // Assert: Subscription re-established
    // (Apollo Client handles automatic reconnection)
  });

  test('should close connection on logout', async ({ page }) => {
    // Arrange: Active subscription
    await graphqlHelper.subscribe('subscription { ... }');

    // Act: Logout
    await page.click('[data-testid="logout-button"]');

    // Assert: WebSocket closed
    await expect(page.locator('[data-testid="ws-status"]')).toContainText('Disconnected');
  });
});
```

---

## Helper Utilities

### GraphQL Helper

**File:** `tests/e2e/helpers/graphql-helper.ts`

```typescript
import { Page } from '@playwright/test';

export class GraphQLHelper {
  constructor(private page: Page) {}

  /**
   * Authenticate user and store JWT token.
   */
  async authenticateUser(email: string): Promise<void> {
    // Implementation depends on OAuth flow
    // For E2E tests, might use test user credentials or mock auth
  }

  /**
   * Subscribe to a GraphQL subscription.
   * Returns a promise that resolves with the first event received.
   */
  async subscribe<T = any>(
    query: string,
    onEvent?: (event: T) => void
  ): Promise<T> {
    return new Promise((resolve) => {
      this.page.evaluate((subscriptionQuery) => {
        const client = (window as any).__apolloClient__;
        const subscription = client.subscribe({
          query: subscriptionQuery
        }).subscribe({
          next: (result: any) => {
            if (onEvent) onEvent(result.data);
            resolve(result.data);
          },
          error: (err: Error) => {
            console.error('Subscription error:', err);
          }
        });
      }, query);
    });
  }

  /**
   * Execute a GraphQL mutation.
   */
  async mutate<T = any>(mutation: string, variables?: any): Promise<T> {
    return this.page.evaluate(
      ({ mutationQuery, vars }) => {
        const client = (window as any).__apolloClient__;
        return client.mutate({
          mutation: mutationQuery,
          variables: vars
        }).then((result: any) => result.data);
      },
      { mutationQuery: mutation, vars: variables }
    );
  }

  /**
   * Get current auth token.
   */
  async getAuthToken(): Promise<string> {
    return this.page.evaluate(() => {
      return localStorage.getItem('auth_token') || '';
    });
  }
}
```

---

## Test Infrastructure Requirements

### 1. Apollo Client Exposure

**File:** `src/app/app.component.ts` (or Apollo module)

```typescript
// Expose Apollo Client for E2E tests
if (typeof window !== 'undefined') {
  (window as any).__apolloClient__ = this.apollo;
}
```

### 2. WebSocket Test Configuration

**File:** `playwright.config.ts`

```typescript
export default defineConfig({
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
    video: 'on-first-retry',
    // Allow WebSocket connections in tests
    extraHTTPHeaders: {
      'Accept': 'application/json'
    }
  },
  webServer: [
    {
      command: 'cd ../.. && npm run start', // Start Angular dev server
      port: 4200,
      reuseExistingServer: true
    },
    {
      command: 'cd ../../src/api && dotnet run --project FamilyHub.Api', // Start API
      port: 5002,
      reuseExistingServer: true
    }
  ]
});
```

### 3. Test Data Setup

**File:** `tests/e2e/fixtures/test-data.sql`

```sql
-- Seed test users, families, and invitations for E2E tests
INSERT INTO auth.users (id, email, external_provider, external_user_id)
VALUES
  ('11111111-1111-1111-1111-111111111111', 'testuser@example.com', 'test', 'test-1'),
  ('22222222-2222-2222-2222-222222222222', 'newmember@example.com', 'test', 'test-2');

-- Create test family
INSERT INTO family.families (id, name, created_by_user_id)
VALUES ('123e4567-e89b-12d3-a456-426614174000', 'Test Family', '11111111-1111-1111-1111-111111111111');

-- Set up family membership
UPDATE auth.users
SET family_id = '123e4567-e89b-12d3-a456-426614174000', role = 'OWNER'
WHERE id = '11111111-1111-1111-1111-111111111111';
```

---

## Running E2E Tests

```bash
# Install dependencies
cd src/frontend/family-hub-web
npm install

# Start infrastructure
docker-compose up -d

# Run all E2E subscription tests
npx playwright test tests/e2e/subscriptions/

# Run specific test file
npx playwright test tests/e2e/subscriptions/family-members-subscription.spec.ts

# Run in UI mode (interactive debugging)
npx playwright test --ui

# Run in headed mode (see browser)
npx playwright test --headed
```

---

## Debugging Subscription Tests

### 1. Check WebSocket Connection

```typescript
// In Playwright test
const wsLogs: any[] = [];
page.on('websocket', ws => {
  ws.on('framereceived', event => wsLogs.push({ type: 'received', data: event.payload }));
  ws.on('framesent', event => wsLogs.push({ type: 'sent', data: event.payload }));
});

// After test
console.log('WebSocket traffic:', wsLogs);
```

### 2. Monitor GraphQL Errors

```typescript
page.on('console', msg => {
  if (msg.text().includes('GraphQL error')) {
    console.error('GraphQL Error:', msg.text());
  }
});
```

### 3. Capture Network Traffic

```bash
# Run with network logging
PWDEBUG=1 npx playwright test --headed
```

---

## Known Limitations

1. **Test Isolation**: Subscriptions maintain state across tests. Use unique family IDs per test or reset database between tests.
2. **Timing Issues**: WebSocket connections take time to establish. Use `page.waitForTimeout()` or wait for specific events.
3. **Authentication**: E2E tests require valid JWT tokens. Consider test user credentials or mock OAuth flow.
4. **Parallel Execution**: WebSocket subscriptions may conflict when running tests in parallel. Consider serial execution: `--workers=1`

---

## Future Enhancements

1. **Subscription Fixture**: Create reusable subscription fixtures for common scenarios
2. **Event Assertions**: Build helper to assert on subscription events with timeout
3. **Multi-User Tests**: Simulate multiple users with separate browser contexts
4. **Performance Tests**: Measure subscription latency and throughput
5. **Reconnection Tests**: Test WebSocket resilience under network failures

---

## Related Documentation

- **Unit Tests:** `src/api/tests/FamilyHub.Tests.Unit/Auth/Presentation/GraphQL/Subscriptions/InvitationSubscriptionsTests.cs`
- **Integration Tests:** `src/api/tests/FamilyHub.Tests.Unit/Infrastructure/Messaging/RedisSubscriptionPublisherTests.cs`
- **Subscription Implementation:** `src/api/Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/Subscriptions/InvitationSubscriptions.cs`
- **Playwright Docs:** https://playwright.dev/docs/intro

---

**Last Updated:** 2026-01-14 (Phase 5 Documentation Complete)
**Implementation Status:** Deferred - Requires frontend Apollo Client setup
**Priority:** Medium - Can be implemented after Phase 6 (Verification & Documentation)
