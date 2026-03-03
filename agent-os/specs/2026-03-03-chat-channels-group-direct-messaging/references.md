# References for Chat Channels with Group and Direct Messaging

## Similar Implementations

### Family-Wide Messaging Channel (Existing)

- **Location:** `src/FamilyHub.Api/Features/Messaging/`
- **Relevance:** This is the module being extended. All existing patterns (Message aggregate, SendMessage command, subscriptions) are the foundation.
- **Key patterns:**
  - Message as AggregateRoot with factory `Create()` method raising domain events
  - MutationType resolves user via IUserService, publishes to ITopicEventSender
  - Cursor-based pagination via `GetByFamilyAsync(familyId, limit, before?)`
  - GraphQL subscription with topic `MessageSent_{familyId}`
  - Frontend: MessagingService with Apollo query/mutation/subscription methods

### Family Module (Membership Patterns)

- **Location:** `src/FamilyHub.Api/Features/Family/`
- **Relevance:** FamilyMember entity pattern informs ChannelParticipant design. FamilyInvitation aggregate shows how to manage participant lifecycle.
- **Key patterns:**
  - FamilyMember as owned entity with FamilyMemberId VO
  - Authorization via FamilyAuthorizationService
  - Domain events for state changes (InvitationAcceptedEvent triggers user-family assignment)

### FileManagement FamilyCreatedEventHandler

- **Location:** `src/FamilyHub.Api/Features/FileManagement/Application/EventHandlers/FamilyCreatedEventHandler.cs`
- **Relevance:** Reference pattern for cross-module event handling. Messaging module will follow the same pattern for auto-creating family channels.
- **Key patterns:**
  - IDomainEventHandler interface
  - Idempotency guard (check if already exists)
  - Explicit SaveChanges in event handler

### Message Attachments Spec

- **Location:** `agent-os/specs/2026-03-03-message-attachments/`
- **Relevance:** Recent messaging spec showing how to extend the module. Channel-agnostic design mentioned there aligns with this feature.

## Key Source Files

| File | Role |
|------|------|
| `src/FamilyHub.Api/Features/Messaging/Domain/Entities/Message.cs` | Message aggregate — will gain ChannelId |
| `src/FamilyHub.Api/Features/Messaging/Application/Commands/SendMessage/MutationType.cs` | GraphQL resolver — must validate channel participation |
| `src/FamilyHub.Api/Features/Messaging/GraphQL/MessagingSubscriptions.cs` | Subscription — topic changes to per-channel |
| `src/FamilyHub.Api/Features/Messaging/Data/MessageConfiguration.cs` | EF config — must add ChannelId column |
| `src/FamilyHub.Api/Features/Family/Domain/Events/FamilyCreatedEvent.cs` | Trigger for auto-creating family channel |
| `src/FamilyHub.Api/Features/Auth/Domain/Events/UserFamilyAssignedEvent.cs` | Trigger for auto-adding to family channel |
| `src/FamilyHub.Api/Common/Database/AppDbContext.cs` | Shared DbContext — needs new DbSets |
| `src/frontend/.../features/messaging/services/messaging.service.ts` | Frontend data layer — must be channel-aware |
