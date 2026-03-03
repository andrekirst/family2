# Message Attachments with File Management Integration

**Created**: 2026-03-03
**GitHub Issue**: #205
**Spec**: `agent-os/specs/2026-03-03-message-attachments/`

## Context

Family members need to share files and images within chat messages. The messaging module (#203/#209) currently supports text-only messages. The File Management module is spec'd (`agent-os/specs/2026-02-18-file-management/`) but not yet built.

This feature is a **cross-domain integration** between Messaging and File Management:

- **File Management** owns `StoredFile` aggregate + `IFileStorageService` (single source of truth for all files)
- **Messaging** gains a `MessageAttachment` concept referencing `FileId` from File Management
- Upload goes through File Management's REST endpoint, then `MessageAttachment` links the resulting `FileId` to a `Message`

**Hard dependency**: File Management module core (StoredFile aggregate, IFileStorageService, REST upload endpoint) must be implemented first.

**Soft dependency**: Event Chain Engine (#117) for auto-organization of chat attachments into folders.

## Shaping Decisions

| Question | Decision |
|---|---|
| Where to save files/images? | File Management module (`StoredFile` + `IFileStorageService`). Single source of truth. |
| What happens on file/image deletion? | **Configurable** family setting: keep file in storage or delete with message. |
| Future chats/channels/groups? | Design attachment model channel-agnostic from the start. `MessageAttachment` references `FileId` regardless of message container. |
| How to add files/images? | All three: **Attachment button** (paperclip icon) + **Drag & Drop** + **Clipboard paste** (Ctrl+V). |
| UI pattern? | **Slack-style file cards**: Below message text with thumbnail, filename, size, mime icon. Click to preview/download. |

## Files to Modify

### Backend (Messaging Module)

| File | Change |
|---|---|
| `src/FamilyHub.Api/Features/Messaging/Domain/Entities/Message.cs` | Add `Attachments` collection |
| `src/FamilyHub.Api/Features/Messaging/Domain/ValueObjects/MessageAttachment.cs` | New — attachment VO with FileId, cached metadata |
| `src/FamilyHub.Api/Features/Messaging/Domain/Events/MessageAttachmentAddedEvent.cs` | New domain event |
| `src/FamilyHub.Api/Features/Messaging/Data/MessageConfiguration.cs` | Configure attachment owned entity |
| `src/FamilyHub.Api/Features/Messaging/MessagingModule.cs` | Register new services |
| `src/FamilyHub.Api/Features/Messaging/Commands/SendMessage/` | Enhance to accept optional file IDs |
| EF Core migration | New `message_attachments` table in `messaging` schema |

### Frontend (Messaging Feature)

| File | Change |
|---|---|
| `MessageInputComponent` | Add attachment button, drag-drop zone, clipboard paste |
| `MessageItemComponent` | Add Slack-style file card rendering |
| `MessagingService` | Enhanced `sendMessage` with file upload flow |
| New `FileCardComponent` | Reusable file card display (thumbnail + metadata) |
| New `AttachmentUploadService` | Orchestrates upload -> FileId -> attach to message |

## Implementation Tasks

### Task 1: Save Spec, Commit, and Update GitHub Issue

1. Write spec files to `agent-os/specs/2026-03-03-message-attachments/`
2. Update GitHub issue #205 with agent-os formatted title, body, and labels
3. Git commit and push

### Task 2: Backend Domain - MessageAttachment Value Object

- Create `MessageAttachment` (owned entity) with:
  - `FileId` (from File Management module — cross-module ID reference)
  - `FileName` (cached at attachment time for display without querying File Management)
  - `MimeType` (cached for thumbnail/icon rendering)
  - `FileSize` (cached for display)
  - `AttachedAt` (timestamp)
- Extend `Message` aggregate with `IReadOnlyList<MessageAttachment> Attachments`
- Enhance `Message.Create()` factory to accept optional attachments
- New domain event: `MessageAttachmentAddedEvent`

### Task 3: Database Migration - Attachment Storage

- New table `messaging.message_attachments`:
  - `id` (UUID PK)
  - `message_id` (FK -> messages)
  - `file_id` (UUID — references file_management.stored_files, no FK constraint across modules)
  - `file_name`, `mime_type`, `file_size` (cached metadata)
  - `attached_at` (timestamp)
- Index on `message_id` for efficient message loading
- RLS policy inheriting from messages table (family isolation)

### Task 4: GraphQL Mutations - Send Message with Attachments

- Enhance `SendMessage` mutation input to accept optional `fileIds: [UUID!]`
- New query field: `attachments` on `MessageType` (loaded with message)
- File card metadata resolved from cached attachment fields (no cross-module query needed)
- Update subscription payload to include attachments

### Task 5: Frontend - Attachment Upload UX

- **Attachment button**: Paperclip icon in `MessageInputComponent` toolbar
- **Drag & Drop**: Drop zone overlay on chat area (reuse pattern from `2026-02-26-browse-drag-drop-upload` spec)
- **Clipboard paste**: `Ctrl+V` handler for image paste via `paste` event + `clipboardData.files`
- Upload flow: File -> REST upload endpoint (File Management) -> receive `FileId` -> attach to message
- Show upload progress indicator per file
- Support multiple attachments per message

### Task 6: Frontend - Slack-Style File Cards

- New `FileCardComponent`: thumbnail, filename, size badge, mime icon, click handler
- Integrate into `MessageItemComponent` below message text
- Image attachments: inline preview (max-width constrained)
- Non-image files: icon + metadata card
- Click behavior: images -> lightbox/viewer, files -> download

### Task 7: Configurable Deletion Behavior

- New family setting: `chatAttachmentDeletionPolicy` (`keep` | `delete_with_message`)
- When message deleted:
  - `keep`: Remove `MessageAttachment` record, `StoredFile` persists in file browser
  - `delete_with_message`: Remove both, publish `FileDeletedEvent`
- UI: Family settings page toggle for this preference

### Task 8: Testing

- Unit tests: `MessageAttachment` creation, `Message` with attachments, deletion policy logic
- Integration tests: Upload -> attach -> query -> display flow
- Frontend: Component tests for file card rendering, drag-drop behavior

## Verification

1. **Send message with attachment**: Upload file via REST, send message with `fileIds`, verify Slack-style card appears
2. **Drag & drop**: Drop image on chat, verify upload + attachment in one flow
3. **Clipboard paste**: Paste screenshot, verify inline image attachment
4. **Multiple attachments**: Send message with 3+ files, verify all cards render
5. **Deletion (keep)**: Delete message, verify file still accessible in file browser
6. **Deletion (delete)**: Toggle setting, delete message, verify file removed
7. **Real-time**: Verify attachments appear via subscription for other family members
8. **Channel-readiness**: Verify attachment model doesn't assume family-wide context
