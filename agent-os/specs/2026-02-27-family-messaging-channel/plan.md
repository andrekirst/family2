# Family-Wide Messaging Channel with Real-Time Chat

**Created**: 2026-02-27
**GitHub Issue**: #203
**Spec**: `agent-os/specs/2026-02-27-family-messaging-channel/`

## Context

Family Hub currently has no in-app messaging capability. The Communication Service is defined in the domain model but only covers notifications. Family members need a way to chat within their family — like a Slack #general channel — with messages appearing in real-time via GraphQL subscriptions.

**Scope:** One shared chat channel per family. All members see all messages. Slack-style flat feed layout (avatar, name, timestamp, content). No threading, reactions, read receipts, or file attachments in this iteration.

---

## Files to Modify (Shared)

| File | Change |
|------|--------|
| `src/FamilyHub.Api/Program.cs` | `RegisterModule<MessagingModule>()`, add subscription type + `AddInMemorySubscriptions()` + `app.UseWebSockets()` |
| `src/FamilyHub.Api/Common/Database/AppDbContext.cs` | Add `DbSet<Message> Messages` |
| `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/RootQuery.cs` | Add `Messaging()` method |
| `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/RootMutation.cs` | Add `Messaging()` method |
| `src/FamilyHub.Api/FamilyHub.slnx` | Add `FamilyHub.Messaging.Tests` project |
| `src/frontend/.../app.routes.ts` | Add `/messages` route with `familyMemberGuard` |
| `src/frontend/.../app.config.ts` | Add `...provideMessagingFeature()` |
| `src/frontend/.../core/graphql/apollo.config.ts` | Add WebSocket split link (`graphql-ws`) |
| `src/frontend/.../shared/layout/sidebar/sidebar.component.ts` | Add Messages nav item |

---

## Implementation Tasks

### Task 1: Save Spec, Commit, and Create GitHub Issue

1. Write spec files to `agent-os/specs/2026-02-27-family-messaging-channel/`
2. Create GitHub issue with labels
3. Commit and push

### Task 2: Backend Domain Layer

Create the Messaging module's domain model following the Family module patterns exactly.

**New files:**

- `src/FamilyHub.Common/Domain/ValueObjects/MessageId.cs` — Vogen `[ValueObject<Guid>]` with `New()` factory
- `src/FamilyHub.Api/Features/Messaging/Domain/ValueObjects/MessageContent.cs` — Vogen `[ValueObject<string>]` with max 4000 char validation
- `src/FamilyHub.Api/Features/Messaging/Domain/Entities/Message.cs` — `AggregateRoot<MessageId>` with `Create(familyId, senderId, content)` factory raising `MessageSentEvent`
- `src/FamilyHub.Api/Features/Messaging/Domain/Events/MessageSentEvent.cs` — `sealed record : DomainEvent`
- `src/FamilyHub.Api/Features/Messaging/Domain/Repositories/IMessageRepository.cs` — `GetByIdAsync`, `GetByFamilyAsync(familyId, limit, before?)` (cursor pagination), `AddAsync`, `SaveChangesAsync`

### Task 3: Backend Infrastructure (EF Core + Repository)

**New files:**

- `src/FamilyHub.Api/Features/Messaging/Data/MessageConfiguration.cs` — `ToTable("messages", "messaging")`, composite index `(family_id, sent_at DESC)`
- `src/FamilyHub.Api/Features/Messaging/Infrastructure/Repositories/MessageRepository.cs` — EF Core implementation

**Modified:**

- `AppDbContext.cs` — add `DbSet<Message> Messages`

**Run:** `dotnet ef migrations add AddMessagingModule`

### Task 4: Backend Application Layer (Command + Query + Handlers)

**New files:**

- `src/FamilyHub.Api/Features/Messaging/Models/MessageDto.cs` — id, familyId, senderId, senderName, senderAvatarId, content, sentAt
- `src/FamilyHub.Api/Features/Messaging/Models/SendMessageRequest.cs` — content field
- `src/FamilyHub.Api/Features/Messaging/Models/GetFamilyMessagesRequest.cs` — limit (default 50), before cursor
- `src/FamilyHub.Api/Features/Messaging/Application/Mappers/MessageMapper.cs` — `ToDto(message, senderName, senderAvatarId)`
- `src/FamilyHub.Api/Features/Messaging/Application/Commands/SendMessage/` — Command.cs, Result.cs, Handler.cs, Validator.cs
- `src/FamilyHub.Api/Features/Messaging/Application/Queries/GetFamilyMessages/` — Query.cs, Handler.cs

**Handler details:**

- `SendMessageCommandHandler` — creates `Message.Create()`, adds to repo. Transaction behavior handles SaveChanges.
- `GetFamilyMessagesQueryHandler` — fetches paginated messages, resolves sender names via `IUserRepository`, returns `List<MessageDto>`

### Task 5: Backend GraphQL Layer

**New files:**

- `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/MessagingQuery.cs` — empty namespace type
- `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/MessagingMutation.cs` — empty namespace type
- `src/FamilyHub.Api/Features/Messaging/Application/Commands/SendMessage/MutationType.cs` — `[ExtendObjectType(typeof(MessagingMutation))]`, extracts user from claims, sends command, publishes to `ITopicEventSender` for real-time delivery
- `src/FamilyHub.Api/Features/Messaging/Application/Queries/GetFamilyMessages/QueryType.cs` — `[ExtendObjectType(typeof(MessagingQuery))]`, extracts user's familyId, sends query
- `src/FamilyHub.Api/Features/Messaging/GraphQL/MessagingSubscriptions.cs` — `[ExtendObjectType("Subscription")]` with `[Topic("MessageSent_{familyId}")]`

**Modified:**

- `RootQuery.cs` — add `[Authorize] public MessagingQuery Messaging() => new();`
- `RootMutation.cs` — add `[Authorize] public MessagingMutation Messaging() => new();`

### Task 6: Backend Module Registration + Subscription Infrastructure

**New files:**

- `src/FamilyHub.Api/Features/Messaging/MessagingModule.cs` — registers `IMessageRepository` as scoped

**Modified `Program.cs`** (3 changes):

1. `builder.Services.RegisterModule<MessagingModule>(builder.Configuration);` (after GoogleIntegration)
2. GraphQL builder: add `.AddSubscriptionType(d => d.Name("Subscription"))` + `.AddInMemorySubscriptions()`
3. Middleware: add `app.UseWebSockets();` before `app.MapGraphQL()`

### Task 7: Frontend Feature Scaffold

**New files:**

- `src/frontend/.../features/messaging/messaging.routes.ts` — `MESSAGING_ROUTES` with single page route
- `src/frontend/.../features/messaging/messaging.providers.ts` — `provideMessagingFeature()`

**Modified:**

- `app.routes.ts` — add `/messages` path with `familyMemberGuard` + `loadChildren`
- `app.config.ts` — add `...provideMessagingFeature()`

### Task 8: Frontend Service + GraphQL Operations + Apollo WebSocket

**Install:** `npm install graphql-ws` in frontend

**New files:**

- `src/frontend/.../features/messaging/graphql/messaging.operations.ts` — `GET_FAMILY_MESSAGES` query, `SEND_MESSAGE` mutation, `ON_MESSAGE_SENT` subscription
- `src/frontend/.../features/messaging/services/messaging.service.ts` — `getMessages(limit, before?)`, `sendMessage(content)`, `subscribeToMessages(familyId)`

**Modified:**

- `src/frontend/.../core/graphql/apollo.config.ts` — add `GraphQLWsLink` + `split()` link routing subscriptions to WS, queries/mutations to HTTP

### Task 9: Frontend Components

**New files:**

- `src/frontend/.../features/messaging/components/messaging-page/messaging-page.component.ts` — container: loads messages, subscribes to real-time, manages state with signals
- `src/frontend/.../features/messaging/components/message-list/message-list.component.ts` — scrollable feed, "load older" on scroll-to-top
- `src/frontend/.../features/messaging/components/message-item/message-item.component.ts` — Slack-style: avatar left, bold sender + gray timestamp, content below (flat, no bubbles)
- `src/frontend/.../features/messaging/components/message-input/message-input.component.ts` — textarea + send button, Enter to send, Shift+Enter for newline

**Modified:**

- Sidebar component — add Messages nav item with chat icon

### Task 10: Backend Unit Tests

**New project:** `tests/FamilyHub.Messaging.Tests/FamilyHub.Messaging.Tests.csproj`
**New shared fake:** `tests/FamilyHub.TestCommon/Fakes/FakeMessageRepository.cs`

**Test files:**

- `MessageAggregateTests.cs` — Create sets properties, raises event, generates unique IDs
- `MessageContentTests.cs` — valid creation, empty/whitespace rejection, max length rejection
- `SendMessageCommandHandlerTests.cs` — happy path, repo called, correct values
- `GetFamilyMessagesQueryHandlerTests.cs` — returns messages, empty list, respects limit

**Modified:** `FamilyHub.slnx` — add test project

### Task 11: Verification

1. `dotnet build` — all projects compile
2. `dotnet test` — all tests pass (existing 77 + new messaging tests)
3. Start backend → verify GraphQL schema has `messaging` namespace in queries/mutations and `messageSent` subscription
4. Start frontend → navigate to `/messages` → verify page loads
5. Send message → verify it appears in list
6. Open two tabs → verify real-time delivery

---

## Key Design Decisions

- **In-memory subscriptions** for MVP (single instance). Upgrade to `AddRedisSubscriptions()` when scaling.
- **Cursor pagination** via `before` timestamp — efficient for "load older" pattern, avoids offset issues.
- **`MessageId` in FamilyHub.Common** — follows existing pattern where cross-module IDs live in the shared project.
- **Sender name resolved at query time** — denormalized in DTO, not stored in Message aggregate (stays normalized).
- **No authorization service yet** — all family members can send/read messages. Family membership is enforced by extracting `user.FamilyId` from the authenticated user context.
