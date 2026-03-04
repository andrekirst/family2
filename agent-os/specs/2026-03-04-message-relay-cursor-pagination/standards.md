# Standards for Relay Cursor Pagination and Scroll UX for Messaging

The following standards apply to this work.

---

## backend/graphql-input-command

Separate Input DTOs (primitives) from Commands (Vogen). See ADR-003.

Each query/mutation has its own type extension file. Maps primitives to Vogen, dispatches via `ICommandBus` or `IQueryBus`.

```csharp
[ExtendObjectType(typeof(MessagingQuery))]
public class QueryType
{
    [Authorize]
    public async Task<Connection<MessageDto>> Messages(...)
    {
        var user = await userService.GetCurrentUser(...);
        var query = new GetFamilyMessagesConnectionQuery(...);
        var result = await queryBus.QueryAsync(query, ct);
        // Build Connection<MessageDto> from result
    }
}
```

**Relevance:** The GraphQL resolver change from `List<MessageDto>` to `Connection<MessageDto>` follows this pattern.

---

## backend/vogen-value-objects

Always use Vogen for domain value objects. Never use primitives in commands/domain.

```csharp
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct MessageId;
```

**Relevance:** `MessageId` is used in the composite cursor. The cursor encode/decode extracts `.Value` (Guid) for Base64 encoding and reconstructs via `MessageId.From()` for repository queries.

---

## database/ef-core-migrations

EF Core migrations with Data/ folder for configurations.

```bash
dotnet ef migrations add UpdateMessageIndexForCompositeCursor \
  --project src/FamilyHub.Api \
  --startup-project src/FamilyHub.Api
```

**Relevance:** The composite index change from `(FamilyId, SentAt DESC)` to `(FamilyId, SentAt DESC, Id DESC)` requires a new migration.

---

## frontend/angular-components

All components are standalone. Use Angular Signals for state.

```typescript
@Component({
  standalone: true,
  imports: [CommonModule],
  ...
})
export class MessageListComponent {
  showJumpToBottom = signal(false);
  unreadCount = signal(0);
}
```

**Relevance:** Scroll state (`showJumpToBottom`, `unreadCount`, `isLoadingOlder`) managed with signals. Jump-to-bottom FAB and unread separator use signal-driven template rendering.

---

## frontend/apollo-graphql

Use Apollo Client for GraphQL with typed operations.

```typescript
const GET_FAMILY_MESSAGES = gql`
  query GetFamilyMessages($first: Int, $after: String) {
    messaging {
      messages(first: $first, after: $after) {
        edges { cursor node { id content sentAt ... } }
        pageInfo { hasNextPage endCursor }
      }
    }
  }
`;
```

**Relevance:** GraphQL operations change from flat list to Relay connection query. Service returns `MessagePageResult { messages, pageInfo }` with proper typing.

---

## testing/unit-testing

xUnit + FluentAssertions with fake repository pattern.

```csharp
// MessageCursor roundtrip test
var encoded = MessageCursor.Encode(sentAt, messageId);
var (decodedSentAt, decodedId) = MessageCursor.Decode(encoded);
decodedSentAt.Should().Be(sentAt);
decodedId.Should().Be(messageId);
```

**Relevance:** New tests for `MessageCursor` utility and `GetFamilyMessagesConnectionQueryHandler`. Update `FakeMessageRepository` to support paged method. FluentAssertions for all assertions.
