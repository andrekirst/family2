# Standards for Message Attachments

The following standards apply to this work.

---

## architecture/ddd-modules

DDD module structure with bounded contexts. Cross-module communication via domain events and ID references only.

**Key points for this feature:**

- Messaging and File Management are separate bounded contexts
- Reference `FileId` by value only (no FK constraint across modules)
- Use domain events for cross-module coordination (e.g., `FileDeletedEvent`)

### Module Layout

```
Features/{ModuleName}/
├── Domain/
│   ├── Entities/          # Aggregates
│   ├── ValueObjects/      # Vogen types
│   ├── Events/            # Domain events
│   └── Repositories/      # Repository interfaces
├── Commands/{Name}/       # Write operations (subfolder per command)
├── Queries/{Name}/        # Read operations
├── Data/                  # EF Core configurations
└── Models/                # Input DTOs
```

### Rules

- No direct module dependencies
- Reference IDs only (no FK constraints across modules)
- Event-driven cross-module communication

---

## backend/domain-events

Events extend `DomainEvent` base record. Published via `RaiseDomainEvent()` on aggregates.

**Key points for this feature:**

- `MessageAttachmentAddedEvent` — raised when attachment is added to a message
- Can trigger event chains (e.g., auto-organize chat files into a folder)

### Event Definition

```csharp
public sealed record MessageAttachmentAddedEvent(
    MessageId MessageId,
    FileId FileId,
    FamilyId FamilyId,
    UserId SenderId,
    DateTime AttachedAt
) : DomainEvent;
```

### Rules

- Events are sealed records extending `DomainEvent`
- Location: `Features/{Module}/Domain/Events/{Name}Event.cs`
- Use past tense: Created, Sent, Added, Deleted

---

## backend/graphql-input-command

Separate Input DTOs (primitives) from Commands (Vogen types). See ADR-003.

**Key points for this feature:**

- Enhance `SendMessageRequest` input to include optional `fileIds: [String!]`
- `SendMessageCommand` maps `fileIds` strings to `FileId` Vogen types
- One MutationType per command in subfolder layout

### File Organization

```
Commands/SendMessage/
  SendMessageCommand.cs
  SendMessageCommandHandler.cs
  SendMessageCommandValidator.cs
  SendMessageResult.cs
  MutationType.cs
```

### Rules

- Input DTOs use primitives (string, Guid)
- Commands use Vogen types (FileId, MessageContent)
- Dispatch via `ICommandBus.SendAsync()`

---

## backend/vogen-value-objects

Always use Vogen for domain value objects. Never use primitives in commands/domain.

**Key points for this feature:**

- `FileId` — GUID VO for cross-module file references (may already exist in File Management)
- `MimeType` — string VO with validation for MIME type format
- Include `EfCoreValueConverter` for all VOs used in persistence

### Definition Pattern

```csharp
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FileId;
```

### Rules

- Always include `conversions: Conversions.EfCoreValueConverter`
- Implement `Validate()` for business rules
- Location: `Domain/ValueObjects/{Name}.cs`

---

## database/ef-core-migrations

EF Core migrations with schema separation per module.

**Key points for this feature:**

- New table `messaging.message_attachments` in the `messaging` schema
- Owned entity configuration for `MessageAttachment` on `Message`
- RLS policy for family isolation

### Schema

```sql
CREATE TABLE messaging.message_attachments (
    id UUID PRIMARY KEY,
    message_id UUID NOT NULL REFERENCES messaging.messages(id) ON DELETE CASCADE,
    file_id UUID NOT NULL,  -- Cross-module reference, no FK
    file_name VARCHAR(255) NOT NULL,
    mime_type VARCHAR(127) NOT NULL,
    file_size BIGINT NOT NULL,
    attached_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_message_attachments_message_id
    ON messaging.message_attachments(message_id);
```

### Rules

- One PostgreSQL schema per module
- Enable RLS on tenant-isolated tables
- No FK constraints across module schemas

---

## frontend/angular-components

Standalone components with `inject()` DI and Angular Signals.

**Key points for this feature:**

- `FileCardComponent` — standalone, reusable file card display
- Use signals for drag-drop state (`isDragging`), upload progress
- Follow atomic design: FileCard is a molecule, attachment area is an organism

### Rules

- Always use `standalone: true`
- Use Angular Signals for state
- Import dependencies in `imports` array

---

## frontend/apollo-graphql

Apollo Client with typed GraphQL operations.

**Key points for this feature:**

- Update `SendMessage` mutation to include `fileIds` variable
- Update message query/subscription to include `attachments` field
- WebSocket subscription payload includes attachment data for real-time display

### Updated Query Pattern

```typescript
const GET_FAMILY_MESSAGES = gql`
  query GetFamilyMessages($limit: Int!, $before: DateTime) {
    messaging {
      getFamilyMessages(limit: $limit, before: $before) {
        id
        content
        senderName
        sentAt
        attachments {
          fileId
          fileName
          mimeType
          fileSize
          attachedAt
        }
      }
    }
  }
`;
```

### Rules

- Use `inject(Apollo)` for dependency injection
- Handle errors with catchError
- Use typed operations

---

## testing/unit-testing

xUnit + FluentAssertions with fake repository pattern.

**Key points for this feature:**

- Test `Message.Create()` with attachments
- Test `MessageAttachment` value object creation and validation
- Test deletion policy logic (keep vs. delete_with_message)
- Fake repository for `IMessageRepository` with attachment support

### Test Pattern

```csharp
[Fact]
public void Message_Create_WithAttachments_ShouldIncludeAttachments()
{
    var attachments = new[] {
        new MessageAttachment(FileId.New(), "photo.jpg", "image/jpeg", 1024)
    };

    var message = Message.Create(familyId, senderId, content, attachments);

    message.Attachments.Should().HaveCount(1);
    message.DomainEvents.Should().Contain(e => e is MessageAttachmentAddedEvent);
}
```

### Rules

- FluentAssertions for all assertions
- Fake repositories as inner classes
- Arrange-Act-Assert pattern
- Call static `Handler.Handle()` directly
