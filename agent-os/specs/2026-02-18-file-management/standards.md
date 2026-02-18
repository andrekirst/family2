# Standards for File Management Module

The following standards apply to this work and guide our implementation decisions.

---

## 1. backend/graphql-input-command

**Source**: `agent-os/standards/backend/graphql-input-command.md`

**Application**: All file management mutations (CreateFolder, UploadFile, MoveFile, RenameFile, DeleteFile, CreateAlbum, CreateTag, etc.) use Input DTOs (primitives) mapped to Commands (Vogen value objects) in mutation resolvers. REST endpoints for binary upload/download are separate.

---

## 2. backend/domain-events

**Source**: `agent-os/standards/backend/domain-events.md`

**Application**: File management publishes domain events for cross-module integration:

- `FileUploadedEvent` — triggers event chains (e.g., auto-tag, notify family)
- `FileDeletedEvent` — cleanup references in albums, tags, shares
- `FileMovedEvent` — update materialized paths, inbox processing log
- `FileRenamedEvent` — update search index
- `FolderCreatedEvent` — inbox rule evaluation
- `FolderDeletedEvent` — cascade cleanup
- `StorageQuotaExceededEvent` — notify family admins

All events are sealed records extending `DomainEvent`, published via `AppDbContext.SaveChangesAsync()`.

---

## 3. backend/vogen-value-objects

**Source**: `agent-os/standards/backend/vogen-value-objects.md`

**Application**: All domain identifiers and constrained types use Vogen:

- `FileId` — GUID identifier for stored files
- `FolderId` — GUID identifier for folders
- `FileName` — validated string (max 255 chars, no path separators)
- `MimeType` — validated MIME string
- `FileSize` — long with non-negative validation
- `StorageKey` — opaque string key for storage backend
- `TagId`, `TagName`, `TagColor` — tag value objects
- `AlbumId`, `AlbumName` — album value objects
- `ShareToken` — 32-byte cryptographic token
- `InvitationToken` pattern reused for share links

---

## 4. database/ef-core-migrations

**Source**: `agent-os/standards/database/ef-core-migrations.md`

**Application**: File management uses the `file_management` PostgreSQL schema. All entity configurations registered in shared `AppDbContext`. Migration creates tables:

- `stored_files` — file metadata, storage key, checksums
- `folders` — hierarchy with materialized path
- `file_tags` / `tags` — tagging system
- `albums` / `album_items` — album groupings
- `file_versions` — version history
- `file_permissions` — access control overrides
- `share_links` / `share_link_access_log` — external sharing
- `inbox_rules` — auto-organization rules
- `external_connections` — external storage provider connections
- `secure_notes` — encrypted note storage
- `zip_jobs` — background zip generation tracking

Follows existing pattern: single `AppDbContext`, schema separation via `builder.HasDefaultSchema("file_management")`.

---

## 5. database/rls-policies

**Source**: `agent-os/standards/database/rls-policies.md`

**Application**: All `file_management` schema tables have RLS policies using `app.current_family_id` session variable. Exceptions:

- `share_link_access_log` — public access logging (no RLS, but rate-limited)
- Share link validation queries bypass RLS via service account

Policies:

```sql
CREATE POLICY family_isolation ON file_management.stored_files
    USING (family_id = current_setting('app.current_family_id', true)::uuid);
```

Applied to: `stored_files`, `folders`, `file_tags`, `tags`, `albums`, `album_items`, `file_versions`, `file_permissions`, `share_links`, `inbox_rules`, `external_connections`, `secure_notes`, `zip_jobs`.

---

## 6. architecture/ddd-modules

**Source**: `agent-os/standards/architecture/ddd-modules.md`

**Application**: `FileManagementModule : IModule` follows the established pattern:

- Self-contained DI registrations
- Registered via `builder.Services.RegisterModule<FileManagementModule>(configuration)` in Program.cs
- Two primary aggregates: `StoredFile`, `Folder`
- Secondary aggregates: `Album`, `SecureNote`
- Cross-module references by ID only (FamilyId, UserId)

---

## 7. architecture/event-chains

**Source**: `agent-os/standards/architecture/event-chains.md`

**Application**: File management events participate in the Event Chain Engine:

**As Triggers:**

- `FileUploadedEvent` → chain actions (auto-tag, move to folder, notify)
- `StorageQuotaExceededEvent` → chain actions (notify admin, suggest cleanup)

**As Actions:**

- "Upload File" — chain step that creates a file in a specified folder
- "Create Folder" — chain step that creates a folder
- "Tag File" — chain step that adds tags to a file

Registered via `IChainRegistry` plugin pattern.

---

## 8. frontend/angular-components

**Source**: `agent-os/standards/frontend/angular-components.md`

**Application**: File management frontend components follow Angular 19 patterns:

- Signals-based state management
- Lazy-loaded route at `/files`
- Feature provider: `provideFileManagementFeature()`
- Components: file-browser, folder-tree, upload-dialog, media-viewer, album-grid, search-bar, tag-manager, secure-notes, share-dialog, batch-toolbar
- Angular CDK drag-and-drop for file/folder organization

---

## 9. frontend/apollo-graphql

**Source**: `agent-os/standards/frontend/apollo-graphql.md`

**Application**: Apollo Angular client with typed operations for all file management queries and mutations. File upload/download via REST (not Apollo). Cache invalidation on file operations.

---

## 10. testing/unit-testing

**Source**: `agent-os/standards/testing/unit-testing.md`

**Application**: `FamilyHub.FileManagement.Tests` project with:

- Domain aggregate tests (StoredFile, Folder, Album, SecureNote)
- Command handler tests (all 14 feature areas)
- Value object validation tests (FileName, MimeType, FileSize, etc.)
- Authorization service tests
- xUnit + FluentAssertions + fake repositories (shared from `FamilyHub.TestCommon`)

---

## How These Standards Apply to File Management

### GraphQL Input→Command (Standard #1)

File management mutations use Input DTOs with primitives, mapped to Commands with Vogen types in resolvers. Binary operations use REST endpoints.

### Domain Events (Standard #2)

7+ domain events published for cross-module integration and event chain triggers. Events flow through existing `AppDbContext.SaveChangesAsync()` pipeline.

### Vogen Value Objects (Standard #3)

10+ Vogen value objects for domain identifiers and constrained types. All with `EfCoreValueConverter` conversions and validation rules.

### EF Core Migrations (Standard #4)

12+ tables in `file_management` schema. Single migration for initial schema, incremental migrations for feature additions.

### RLS Policies (Standard #5)

Family isolation on all tables except public share access log. Consistent with existing RLS patterns.

### DDD Modules (Standard #6)

Standard `IModule` implementation with self-contained registrations. Minimal conflict surface with shared files.

### Event Chains (Standard #7)

File events as triggers for automation. File operations as available chain actions for user-defined workflows.

### Angular Components (Standard #8)

Signals-based components with lazy-loaded routing. Feature provider pattern for DI configuration.

### Apollo GraphQL (Standard #9)

Typed operations for metadata CRUD. REST for binary upload/download/streaming.

### Unit Testing (Standard #10)

Per-module test project with comprehensive coverage of domain logic, handlers, and authorization.
