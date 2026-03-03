# Message Attachments — Shaping Notes

**Feature**: Add file and image attachments to chat messages
**Created**: 2026-03-03
**GitHub Issue**: #205

---

## Scope

Family members can attach files and images to messages in the family chat. Attachments are stored via the File Management module (shared `StoredFile` aggregate) and referenced by the Messaging module via `FileId`. The UI follows Slack-style file cards below message text.

### In Scope

- Attachment button (paperclip icon) in message input area
- Drag & Drop file upload onto chat area
- Clipboard paste (Ctrl+V) for images
- Slack-style file card display (thumbnail, filename, size, mime icon)
- Multiple attachments per message
- Configurable deletion behavior (family setting)
- Channel-agnostic design (works with future channels/groups)

### Out of Scope

- File previews/editing within chat (defer to File Management viewer)
- Audio/video recording directly in chat
- Message threading or reactions
- File size limits (handled by File Management module)
- File type restrictions (handled by File Management module)

---

## Decisions

### Q: Where are files stored?

**A**: File Management module owns all file storage via `StoredFile` aggregate and `IFileStorageService`. Messages reference files via `FileId` (cross-module ID reference, no FK constraint). This ensures a single source of truth — files uploaded in chat are also browsable in the file manager.

### Q: What happens when a message with attachments is deleted?

**A**: Configurable via a family setting (`chatAttachmentDeletionPolicy`):

- `keep` — Only the `MessageAttachment` record is removed. The `StoredFile` persists and remains accessible in the file browser.
- `delete_with_message` — Both the attachment record and the `StoredFile` are deleted. A `FileDeletedEvent` is published for cleanup.

### Q: How does this work with future channels/groups?

**A**: The `MessageAttachment` model is channel-agnostic. It references `FileId` regardless of whether the message belongs to the family-wide channel, a group chat, or a direct message. The `Message` aggregate already has a context (currently `familyId`), and future channel support would add `channelId` to `Message`, not to `MessageAttachment`.

### Q: How do users add files?

**A**: Three methods, matching modern chat app expectations:

1. **Attachment button** — Paperclip icon next to send button, opens file picker
2. **Drag & Drop** — Drop zone overlay on chat area (reuses pattern from browse-drag-drop-upload spec)
3. **Clipboard paste** — Ctrl+V handler for pasting screenshots/images

### Q: What's the upload flow?

**A**: File -> REST upload endpoint (File Management) -> receive `FileId` -> include `fileIds` in `SendMessage` mutation -> `MessageAttachment` records created. This keeps binary upload separate from the GraphQL mutation (REST for binary, GraphQL for metadata).

### Q: What does the UI look like?

**A**: Slack-style file cards:

- Below message text, not inline
- Shows: thumbnail (for images), filename, file size, mime type icon
- Click: images open in lightbox/viewer, other files trigger download
- Multiple attachments stack vertically below the message

---

## Context

- **Visuals:** Slack-style file cards (reference: Slack desktop app file sharing UX)
- **References:**
  - Messaging module: `agent-os/specs/2026-02-27-family-messaging-channel/`
  - File Management: `agent-os/specs/2026-02-18-file-management/`
  - Drag & Drop upload: `agent-os/specs/2026-02-26-browse-drag-drop-upload/`
  - Communication profile: `agent-os/profiles/modules/communication.yaml`
- **Product alignment:** Cross-domain integration is core to Family Hub's event chain automation differentiator. File attachments in chat demonstrate the value of shared file storage across modules.

## Standards Applied

- **architecture/ddd-modules** — Cross-module integration via ID references (no FK constraints)
- **backend/domain-events** — `MessageAttachmentAddedEvent` for event chain triggers
- **backend/graphql-input-command** — Enhanced SendMessage input with optional fileIds
- **backend/vogen-value-objects** — FileId, MimeType value objects
- **database/ef-core-migrations** — New message_attachments table in messaging schema
- **frontend/angular-components** — Standalone FileCardComponent with signals
- **frontend/apollo-graphql** — Updated GraphQL operations with attachment fields
- **testing/unit-testing** — Fake repository pattern for attachment testing
