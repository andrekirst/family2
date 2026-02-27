# Standards for Family-Wide Messaging Channel

The following standards apply to this work.

---

## backend/graphql-input-command

Separate Input DTOs (primitives) from Commands (Vogen). See ADR-003.

- Input DTOs in `Models/` with primitives (`SendMessageRequest` with `string Content`)
- Commands in `Commands/{Name}/` with Vogen types (`SendMessageCommand` with `MessageContent`)
- One MutationType per command (not centralized)
- Dispatch via `ICommandBus.SendAsync()`

---

## backend/permission-system

Role-based permissions with VO methods, defense-in-depth enforcement.

For messaging MVP: all family members can send and read messages. No fine-grained permissions needed yet. Family membership enforced by checking `user.FamilyId` from JWT claims in GraphQL resolvers.

Future consideration: `messaging:send`, `messaging:read` permissions if role-based chat access is needed.

---

## backend/domain-events

Events extend `DomainEvent` base record. Raised via `RaiseDomainEvent()` on aggregates.

- `MessageSentEvent(MessageId, FamilyId, SenderId, Content, SentAt)` — raised in `Message.Create()`
- Used for: real-time subscription publishing, future event chain triggers
- Location: `Features/Messaging/Domain/Events/MessageSentEvent.cs`

---

## backend/vogen-value-objects

Always use Vogen for domain value objects. Never use primitives in commands/domain.

- `MessageId` — `[ValueObject<Guid>]` with `New()` factory, in `FamilyHub.Common/Domain/ValueObjects/`
- `MessageContent` — `[ValueObject<string>]` with validation (non-empty, max 4000 chars), in `Features/Messaging/Domain/ValueObjects/`

---

## backend/user-context

Accessing the current authenticated user from JWT claims.

- Use `ClaimNames.Sub` to extract external user ID from `ClaimsPrincipal`
- Resolve internal user via `IUserRepository.GetByExternalIdAsync()`
- Access `user.FamilyId` for family-scoped operations
- Apply `[Authorize]` on all GraphQL resolvers

---

## database/ef-core-migrations

EF Core migrations with Data/ folder for configurations.

- Schema: `messaging`
- Table: `messages` with columns: id, family_id, sender_id, content, sent_at
- Configuration: `Data/MessageConfiguration.cs` with `IEntityTypeConfiguration<Message>`
- Index: composite `(family_id, sent_at DESC)` for efficient timeline queries
- Vogen converters: `HasConversion(id => id.Value, value => MessageId.From(value))`

---

## database/rls-policies

PostgreSQL Row-Level Security for multi-tenancy.

Future consideration: enable RLS on `messaging.messages` table with family isolation policy:

```sql
CREATE POLICY family_message_isolation ON messaging.messages
    USING (family_id = current_setting('app.current_family_id', true)::uuid);
```

Not implemented in MVP — application-level filtering via `WHERE family_id = @familyId` is sufficient initially.

---

## frontend/angular-components

Standalone components with `inject()` DI and computed signals.

- All components use `standalone: true`
- Signal-based state: `signal<T>()` for reactive state, `computed()` for derived
- DI via `inject(Service)` (not constructor injection)
- New Angular control flow: `@if`, `@for`
- Components: `MessagingPageComponent`, `MessageListComponent`, `MessageItemComponent`, `MessageInputComponent`

---

## frontend/apollo-graphql

Apollo Client with typed GraphQL operations.

- Operations in `graphql/messaging.operations.ts` using `gql` tagged templates
- Service methods return typed observables via `apollo.query()`, `apollo.mutate()`, `apollo.subscribe()`
- WebSocket support via `graphql-ws` package + `GraphQLWsLink` + `split()` link
- Error handling: `catchError` with console logging, return defaults

---

## testing/unit-testing

xUnit + FluentAssertions with fake repository pattern.

- Test project: `tests/FamilyHub.Messaging.Tests/`
- Shared fake: `FakeMessageRepository` in `tests/FamilyHub.TestCommon/Fakes/`
- Test domain aggregate: `Message.Create()` properties + domain events
- Test value objects: `MessageContent` validation (empty, whitespace, too long)
- Test handlers: `SendMessageCommandHandler`, `GetFamilyMessagesQueryHandler`
- Pattern: Arrange-Act-Assert with FluentAssertions
