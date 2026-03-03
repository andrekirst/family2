# References for Message Attachments

## Similar Implementations

### Family-Wide Messaging Channel

- **Spec**: `agent-os/specs/2026-02-27-family-messaging-channel/`
- **Implementation**: PR #203 / #209
- **Relevance**: This is the module being extended. MessageAttachment will be added to the existing `Message` aggregate.
- **Key patterns to reuse:**
  - `Message` aggregate with `Create()` factory method and domain events
  - `MessageConfiguration` EF Core setup with Vogen converters
  - `MessagingModule` DI registration
  - `SendMessage` command/handler/mutation structure
  - Frontend `MessageInputComponent` and `MessageItemComponent` layout
  - Apollo subscription pattern for real-time delivery

### File Management Module

- **Spec**: `agent-os/specs/2026-02-18-file-management/`
- **Status**: Not yet implemented (14-issue epic, Phase 2)
- **Relevance**: Provides the `StoredFile` aggregate and `IFileStorageService` that attachments will reference.
- **Key patterns to reuse:**
  - `StoredFile` aggregate with metadata (filename, mime type, size, storage key)
  - `IFileStorageService` + `IStorageProvider` abstraction for binary storage
  - REST endpoints for file upload/download (binary operations separate from GraphQL)
  - `FileId` as cross-module reference ID

### Browse & Drag-Drop Upload

- **Spec**: `agent-os/specs/2026-02-26-browse-drag-drop-upload/`
- **Relevance**: Drag-and-drop UX pattern for file upload in the frontend.
- **Key patterns to reuse:**
  - `isDragging` signal with drag counter pattern (prevent flickering)
  - Semi-transparent overlay during drag
  - Auto-open upload dialog with dropped files pre-loaded
  - Native HTML5 Drag and Drop API (no external library)

### Communication Module Profile

- **Profile**: `agent-os/profiles/modules/communication.yaml`
- **Relevance**: Defines the broader Communication module that Messaging belongs to.
- **Key patterns to reuse:**
  - `MessageSentEvent` domain event pattern
  - Event chain integration triggers (cross-domain notifications)

## Existing Code to Reference

### Message Aggregate

- **Location**: `src/FamilyHub.Api/Features/Messaging/Domain/Entities/Message.cs`
- **Pattern**: AggregateRoot with factory method, domain events, Vogen VOs

### SendMessage Command

- **Location**: `src/FamilyHub.Api/Features/Messaging/Commands/SendMessage/`
- **Pattern**: Input -> Command -> Handler -> Result with MutationType

### MessageInput Component (Frontend)

- **Location**: `src/frontend/family-hub-web/src/app/features/messages/components/message-input/`
- **Pattern**: Textarea + send button, Enter to send, Shift+Enter for newline

### MessageItem Component (Frontend)

- **Location**: `src/frontend/family-hub-web/src/app/features/messages/components/message-item/`
- **Pattern**: Slack-style layout (avatar left, sender + timestamp, content)

### Family Invitation Flow

- **Location**: `src/FamilyHub.Api/Features/Family/Commands/SendInvitation/`
- **Relevance**: Example of cross-module ID reference pattern (UserId from Auth module referenced in Family module without FK constraint)
