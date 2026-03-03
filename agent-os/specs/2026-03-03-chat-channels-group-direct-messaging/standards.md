# Standards for Chat Channels with Group and Direct Messaging

The following standards apply to this work.

---

## architecture/ddd-modules

Modular monolith architecture with bounded contexts, each self-contained.

- One PostgreSQL schema per module (`messaging` schema for all channel tables)
- Cross-module communication via domain events (no direct FK constraints)
- Reference IDs only across module boundaries (e.g., `UserId` from Auth, `FamilyId` from Family)
- Module layout: `Domain/` → `Application/` → `Data/` → `Infrastructure/` → `Models/` → `GraphQL/`
- **Feature-specific:** Channel, ChannelParticipant, and related VOs all live within the Messaging module. ChannelId is in FamilyHub.Common for cross-module reference.

---

## backend/graphql-input-command

Separate input DTOs (primitives) from commands (Vogen types); one MutationType per command.

- Input DTOs use primitives (`string`, `Guid`), located in `Models/` with suffix `Request`
- Commands use Vogen types (`ChannelId`, `ChannelName`), implement `ICommand<TResult>`
- Each command subfolder: `Command.cs`, `Handler.cs`, `Validator.cs`, `Result.cs`, `MutationType.cs`
- MutationType maps input→command, extracts user via `IUserService`, dispatches via `ICommandBus.SendAsync()`
- **Feature-specific:** CreateChannel, AddChannelMember, RemoveChannelMember, CreateOrGetDirectChannel each get a dedicated subfolder.

---

## backend/domain-events

Events extend DomainEvent base record; raised via `aggregate.RaiseDomainEvent()`.

- Events are sealed records extending `DomainEvent`, use past tense names
- Publishing: `aggregate.RaiseDomainEvent(new XyzEvent(...))`
- Location: `Features/{Module}/Domain/Events/{Name}Event.cs`
- Handlers: `Features/{Module}/Application/EventHandlers/{Name}Handler.cs`
- **Feature-specific:** `ChannelCreatedEvent`, `ChannelParticipantAddedEvent`, `ChannelParticipantRemovedEvent`. Cross-module handlers for `FamilyCreatedEvent` and `UserFamilyAssignedEvent`.

---

## backend/vogen-value-objects

Always use Vogen for domain value objects with EF Core converter.

- Attribute: `[ValueObject<T>(conversions: Conversions.EfCoreValueConverter)]`
- Implement `private static Validation Validate(value)` for business rules
- Creation: `ChannelId.New()` for GUIDs, `ChannelName.From(string)` with validation
- EF Core config: `Property(x => x.Id).HasConversion(new ChannelId.EfCoreValueConverter())`
- **Feature-specific:** `ChannelId` (Guid), `ChannelType` (string, constrained to family/group/direct), `ChannelName` (string, 2-100 chars), `ChannelParticipantId` (Guid).

---

## backend/user-context

Access authenticated user from JWT claims via IUserService.

- `IUserService.GetCurrentUser(ClaimsPrincipal, IUserRepository, CancellationToken)` → User
- Always decorate mutations/queries with `[Authorize]`
- In GraphQL resolvers: inject `IUserService`, use `claimsPrincipal` parameter
- **Feature-specific:** All channel mutations/queries require authenticated user. SendMessage MutationType must resolve user to validate channel participation.

---

## database/ef-core-migrations

EF Core migrations with schema separation.

- All tables in `messaging` schema: `messaging.channels`, `messaging.channel_participants`
- Migration naming: `{Timestamp}_{Description}` (e.g., `AddChannelEntities`)
- Two-step nullable migration for adding `channel_id` to existing `messages` table
- Composite indexes for query performance: `(family_id)` on channels, `(channel_id, sent_at DESC)` on messages, `(channel_id, user_id)` unique on participants
- **Feature-specific:** Data migration creates family channels for existing families and links existing messages.

---

## frontend/angular-components

Standalone components with signals-based state.

- Always set `standalone: true` in `@Component` decorator
- Import dependencies in `imports` array
- Use Angular Signals for state (`signal()`, `computed()`, `signal.update()`)
- **Feature-specific:** ChatLayoutComponent (split view), ChannelListComponent (sidebar), ChannelChatComponent (per-channel feed), CreateChannelDialogComponent, DmInitiationComponent. All standalone with signal inputs/outputs.

---

## frontend/apollo-graphql

Apollo Client with typed GraphQL operations.

- Query: `const QUERY = gql'...'; this.apollo.query({ query: QUERY }).pipe(map(...))`
- Mutation: `apollo.mutate({ mutation: MUTATION, variables: { input } })`
- Subscription: `apollo.subscribe({ query: SUBSCRIPTION, variables: { channelId } })`
- Error handling: `catchError(error)` → return safe default
- **Feature-specific:** New operations: `GET_MY_CHANNELS`, `GET_CHANNEL_MESSAGES`, `CREATE_CHANNEL`, `CREATE_OR_GET_DIRECT_CHANNEL`, `CHANNEL_MESSAGE_SENT_SUBSCRIPTION`. Updated: `SEND_MESSAGE` with channelId.

---

## testing/unit-testing

xUnit + FluentAssertions with fake repository pattern.

- Always use FluentAssertions (never xUnit Assert)
- Fake repos: in-memory implementations in `tests/FamilyHub.TestCommon/Fakes/`
- Call handlers directly with fake dependencies
- Domain event assertions: `aggregate.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<Event>()`
- **Feature-specific:** New `FakeChannelRepository`. Tests for Channel aggregate, ChannelType/ChannelName VOs, CreateChannel handler, CreateOrGetDirectChannel handler, GetMyChannels/GetChannelMessages query handlers.
