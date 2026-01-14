# GraphQL Subscription Events Integration Guide

**Status:** Phase 4 Complete - Event Publishing Infrastructure Ready
**Issue:** #84 - GraphQL Subscriptions for Real-Time Family Updates

---

## Overview

This document explains how to integrate GraphQL subscription events into command handlers for real-time UI updates via WebSocket connections.

## Current Architecture

```
Command Handler → SubscriptionEventPublisher → IRedisSubscriptionPublisher → Redis PubSub → Hot Chocolate → WebSocket → Client
```

### Key Components

| Component | Purpose | Location |
|-----------|---------|----------|
| `IRedisSubscriptionPublisher` | Low-level Redis PubSub publisher | `FamilyHub.Infrastructure/Messaging/IRedisSubscriptionPublisher.cs` |
| `RedisSubscriptionPublisher` | Implementation using Hot Chocolate's `ITopicEventSender` | `FamilyHub.Infrastructure/Messaging/RedisSubscriptionPublisher.cs` |
| `SubscriptionEventPublisher` | High-level helper for command handlers | `FamilyHub.Infrastructure/Messaging/SubscriptionEventPublisher.cs` |
| `InvitationSubscriptions` | GraphQL subscription resolvers with authorization | `Modules/Auth/Presentation/GraphQL/Subscriptions/InvitationSubscriptions.cs` |

---

## Usage in Command Handlers

### Example: AcceptInvitationCommandHandler

```csharp
public sealed partial class AcceptInvitationCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SubscriptionEventPublisher _subscriptionPublisher; // INJECT THIS

    public async Task<Result<AcceptInvitationResult>> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Perform business logic (accept invitation, update user, etc.)
        invitation.Accept(currentUserId);
        currentUser.UpdateFamily(invitation.FamilyId);
        currentUser.UpdateRole(invitation.Role);

        // 2. Save changes to database
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Publish subscription events for real-time UI updates
        // This triggers WebSocket messages to connected clients
        await _subscriptionPublisher.PublishFamilyMemberAddedAsync(
            invitation.FamilyId,
            new FamilyMemberType
            {
                UserId = currentUser.Id.Value,
                Email = currentUser.Email.Value,
                Role = invitation.Role.Value,
                JoinedAt = DateTime.UtcNow
            },
            cancellationToken
        );

        // ALSO publish invitation removed (invitation no longer pending)
        await _subscriptionPublisher.PublishInvitationRemovedAsync(
            invitation.FamilyId,
            invitation.Token.Value,
            cancellationToken
        );

        return Result.Success(new AcceptInvitationResult { /* ... */ });
    }
}
```

### Example: InviteFamilyMemberCommandHandler

```csharp
public sealed partial class InviteFamilyMemberCommandHandler
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SubscriptionEventPublisher _subscriptionPublisher; // INJECT THIS

    public async Task<Result<InviteFamilyMemberResult>> Handle(
        InviteFamilyMemberCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Create invitation
        var invitation = family.CreateInvitation(request.Email, request.Role);

        // 2. Save to database
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Publish subscription event
        await _subscriptionPublisher.PublishInvitationAddedAsync(
            family.Id,
            new PendingInvitationType
            {
                InvitationToken = invitation.Token.Value,
                Email = invitation.Email.Value,
                Role = invitation.Role.Value,
                CreatedAt = invitation.CreatedAt,
                ExpiresAt = invitation.ExpiresAt
            },
            cancellationToken
        );

        return Result.Success(new InviteFamilyMemberResult { /* ... */ });
    }
}
```

### Example: CancelInvitationCommandHandler

```csharp
public sealed partial class CancelInvitationCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SubscriptionEventPublisher _subscriptionPublisher; // INJECT THIS

    public async Task<Result> Handle(
        CancelInvitationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Cancel invitation
        invitation.Cancel();

        // 2. Save to database
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Publish subscription event
        await _subscriptionPublisher.PublishInvitationRemovedAsync(
            invitation.FamilyId,
            invitation.Token.Value,
            cancellationToken
        );

        return Result.Success();
    }
}
```

---

## Available Helper Methods

### FamilyMemberType Changes

```csharp
// Member added (e.g., invitation accepted)
await subscriptionPublisher.PublishFamilyMemberAddedAsync(
    FamilyId familyId,
    FamilyMemberType member,
    CancellationToken cancellationToken
);

// Member removed (e.g., left family)
await subscriptionPublisher.PublishFamilyMemberRemovedAsync(
    FamilyId familyId,
    UserId memberId,
    CancellationToken cancellationToken
);
```

### PendingInvitationType Changes

```csharp
// Invitation created
await subscriptionPublisher.PublishInvitationAddedAsync(
    FamilyId familyId,
    PendingInvitationType invitation,
    CancellationToken cancellationToken
);

// Invitation accepted or canceled
await subscriptionPublisher.PublishInvitationRemovedAsync(
    FamilyId familyId,
    string invitationToken,
    CancellationToken cancellationToken
);
```

---

## Subscription Topics

| Topic Pattern | Purpose | Authorization |
|---------------|---------|---------------|
| `family-members-changed:{familyId}` | Family member updates (ADDED, REMOVED) | Requires family membership (any role) |
| `pending-invitations-changed:{familyId}` | Invitation updates (ADDED, REMOVED) | Requires OWNER or ADMIN role |

---

## Error Handling

**Subscription publishing is best-effort** - errors are logged but NOT thrown. This ensures that subscription delivery failures do NOT impact the primary operation (e.g., accepting an invitation).

```csharp
// If Redis is unavailable, this logs a warning but doesn't throw
await subscriptionPublisher.PublishFamilyMemberAddedAsync(...);
// ✅ Invitation is still accepted in the database
// ⚠️  Real-time UI update may be missed (client can refresh)
```

---

## Future Migration: Domain Event Handlers

**Current Approach (Interim):**

- Command handlers directly call `SubscriptionEventPublisher`
- Works well but couples subscription logic to command handlers

**Future Approach (Ideal):**

- Aggregates raise domain events (e.g., `InvitationAcceptedEvent`)
- Domain event handlers (MediatR `INotificationHandler<TEvent>`) publish subscription messages
- Decouples subscription logic from command handlers

### Example Future Domain Event Handler

```csharp
// Future: When domain events are implemented in aggregates
public sealed class InvitationAcceptedSubscriptionHandler
    : INotificationHandler<InvitationAcceptedEvent>
{
    private readonly IRedisSubscriptionPublisher _publisher;

    public async Task Handle(
        InvitationAcceptedEvent notification,
        CancellationToken cancellationToken)
    {
        var payload = new FamilyMembersChangedPayload
        {
            FamilyId = notification.FamilyId.Value,
            ChangeType = ChangeType.Added,
            Member = notification.Member
        };

        await _publisher.PublishAsync(
            $"family-members-changed:{notification.FamilyId.Value}",
            payload,
            cancellationToken
        );
    }
}
```

### Migration Checklist

When domain events are implemented:

1. ✅ Add domain events to aggregates (e.g., `User.AcceptInvitation()` raises `InvitationAcceptedEvent`)
2. ✅ Create domain event handlers (implement `INotificationHandler<TEvent>`)
3. ✅ MediatR auto-discovers and registers handlers (already configured in `AuthModuleServiceRegistration`)
4. ✅ Remove `SubscriptionEventPublisher` calls from command handlers
5. ✅ Delete `SubscriptionEventPublisher` class (no longer needed)

---

## Testing

### Unit Tests

```csharp
[Theory, AutoNSubstituteData]
public async Task Handle_ValidCommand_PublishesSubscriptionEvent(
    [Frozen] Mock<SubscriptionEventPublisher> publisherMock,
    AcceptInvitationCommandHandler sut,
    AcceptInvitationCommand command)
{
    // Act
    await sut.Handle(command, CancellationToken.None);

    // Assert
    publisherMock.Verify(
        p => p.PublishFamilyMemberAddedAsync(
            It.IsAny<FamilyId>(),
            It.IsAny<FamilyMemberType>(),
            It.IsAny<CancellationToken>()),
        Times.Once);
}
```

### Integration Tests

```csharp
// Test that subscription messages are published to Redis
[Fact]
public async Task AcceptInvitation_PublishesToRedis()
{
    // Arrange: Set up TestWebApplicationFactory with Redis
    var client = _factory.CreateClient();

    // Act: Execute GraphQL mutation
    var response = await client.AcceptInvitationMutationAsync(...);

    // Assert: Verify Redis received subscription message
    // (Requires Redis integration test infrastructure)
}
```

### E2E Tests (Playwright)

```typescript
// Test that WebSocket subscription receives real-time updates
test('accepting invitation triggers subscription update', async ({ page }) => {
  // Subscribe to family members changes
  const subscriptionPromise = page.graphqlSubscribe(`
    subscription {
      familyMembersChanged(familyId: "${familyId}") {
        changeType
        member { email role }
      }
    }
  `);

  // Trigger mutation in another tab/user
  await page.graphqlMutation(`
    mutation {
      acceptInvitation(input: { token: "${token}" }) {
        familyId
      }
    }
  `);

  // Assert: Subscription received ADDED event
  const event = await subscriptionPromise;
  expect(event.changeType).toBe('ADDED');
  expect(event.member.email).toBe('newmember@example.com');
});
```

---

## Troubleshooting

### Subscription Messages Not Received

1. **Check Redis connection:**

   ```bash
   docker exec familyhub-redis redis-cli ping
   # Expected: PONG
   ```

2. **Monitor Redis PubSub:**

   ```bash
   docker exec -it familyhub-redis redis-cli
   > MONITOR
   # Trigger mutation and watch for messages
   ```

3. **Check Seq logs:**
   - Search for: `"Published subscription message"`
   - Warnings: `"Failed to publish subscription message"`

4. **Verify WebSocket connection:**
   - GraphQL Playground: Check WebSocket connection status
   - Browser DevTools: Network tab → WS filter

### Authorization Failures

Subscription terminates immediately (no error in UI):

1. Check `InvitationSubscriptions` logs in Seq:
   - `"User {UserId} attempted to subscribe without membership"`
   - `"User {UserId} with role {Role} attempted to subscribe (requires OWNER or ADMIN)"`

2. Verify user membership:

   ```graphql
   query {
     currentUser {
       familyMemberships {
         familyId
         role
       }
     }
   }
   ```

---

## Related Documentation

- **Phase 1-3 Implementation:** See `/home/andrekirst/.claude/plans/dynamic-wiggling-wozniak.md`
- **ADR-011:** DataLoader Performance Targets
- **GraphQL Schema:** `Modules/Auth/Presentation/GraphQL/INVITATION_SCHEMA.md`
- **Domain Events (Future):** `docs/architecture/PATTERNS.md`

---

**Last Updated:** 2026-01-14 (Phase 4 Complete)
**Next Steps:** Integrate `SubscriptionEventPublisher` into command handlers (see examples above)
