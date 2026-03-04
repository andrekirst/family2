# References for Relay Cursor Pagination and Scroll UX for Messaging

## Similar Implementations

### Existing Messaging Module (Backend)

- **Location:** `src/FamilyHub.Api/Features/Messaging/`
- **Relevance:** The module we're modifying -- already has basic cursor pagination
- **Key patterns:**
  - `IMessageRepository.GetByFamilyAsync(familyId, limit, before)` -- current pagination interface
  - `MessageRepository` -- EF Core implementation with `OrderByDescending(SentAt).Take(limit)`
  - `QueryType` using `[ExtendObjectType(typeof(MessagingQuery))]` pattern
  - `GetFamilyMessagesQueryHandler` -- existing handler resolving sender info

### Existing Messaging Module (Frontend)

- **Location:** `src/frontend/family-hub-web/src/app/features/messaging/`
- **Relevance:** The frontend we're enhancing
- **Key patterns:**
  - `MessagingService.getMessages(limit, before)` -- current service API
  - `MessagingPageComponent` -- orchestrator with `rawMessages` signal, subscription handling, deduplication
  - `MessageListComponent` -- scroll container with `scrollTop === 0` detection, auto-scroll-to-bottom logic
  - `MessageItemComponent` -- individual message rendering with avatar, timestamp formatting

### Family Module (Relay Connection Reference)

- **Location:** `src/FamilyHub.Api/Features/Family/`
- **Relevance:** Reference for Hot Chocolate patterns used in the project (GraphQL type extensions, service injection, authorization)
- **Key patterns:**
  - `[Authorize]` attribute on resolvers
  - `IUserService.GetCurrentUser()` for user context
  - `ICommandBus` / `IQueryBus` dispatch

### Test Infrastructure

- **Location:** `tests/FamilyHub.TestCommon/Fakes/`
- **Relevance:** Fake repository pattern to extend
- **Key patterns:**
  - `FakeMessageRepository` -- in-memory message store with LINQ-based filtering
  - Shared across test projects via `FamilyHub.TestCommon` reference
  - `FakeFamilyRepository`, `FakeUserRepository` as reference implementations

### Chat Channels Spec (Issue #210)

- **Location:** `agent-os/specs/2026-03-03-chat-channels-group-direct-messaging/`
- **Relevance:** Future feature that will reuse pagination infrastructure
- **Key patterns:**
  - `Channel` aggregate with `ChannelId` scoping
  - Per-channel subscriptions replacing per-family
  - Our pagination must be channel-agnostic to support this
