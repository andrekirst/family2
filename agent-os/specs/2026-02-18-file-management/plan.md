# File Management Module — Implementation Plan

**Created**: 2026-02-18
**Spec Folder**: `agent-os/specs/2026-02-18-file-management/`

---

## Context

The "Document & Info Vault" (Feature Backlog Phase 2, Domain 7) is a privacy-first family file storage and document management system inspired by Synology Photos/Drive. It provides secure storage, auto-organization, media streaming, external integrations, and zero-knowledge encrypted notes — capabilities no competitor offers in a family-focused platform.

This plan covers the creation of GitHub issues (1 epic + 14 features), a milestone, labels, and spec documentation for the File Management module.

---

## Issue Summary Table

| # | Title | Priority | Effort | Depends On |
|---|-------|----------|--------|------------|
| 1 | Core file storage infrastructure | P0 | XL | None |
| 2 | FileManagement module & domain model | P0 | XL | #1 |
| 3 | Folder organization & hierarchy | P0 | L | #2 |
| 4 | Virtual inbox & auto-organization rules | P1 | XL | #2, #3 |
| 5 | File tagging & metadata (GPS) | P1 | L | #2 |
| 6 | Full-text search & discoverability | P1 | L | #2, #5 |
| 7 | Albums & favorites | P1 | M | #2 |
| 8 | Document versioning | P2 | L | #1, #2 |
| 9 | File & folder sharing (internal permissions) | P0 | L | #2, #3 |
| 10 | External sharing (link-based access) | P0 | L | #2, #9 |
| 11 | Media viewer & streaming | P1 | XL | #1, #2 |
| 12 | External storage integrations | P2 | XL | #2, #3 |
| 13 | Secure notes (encrypted) | P0 | XL | #2 |
| 14 | Remote zip & batch operations | P2 | L | #2, #3, #5, #7 |

---

## Dependency Graph

```
#1 Core Storage Infrastructure
 └──▶ #2 Module & Domain Model
      ├──▶ #3 Folder Organization
      │    ├──▶ #4 Virtual Inbox (needs #2, #3)
      │    ├──▶ #9 Sharing & Permissions (needs #2, #3)
      │    │    └──▶ #10 External Sharing (needs #2, #9)
      │    ├──▶ #12 External Storage (needs #2, #3)
      │    └──▶ #14 Batch Ops (needs #2, #3, #5, #7)
      ├──▶ #5 File Tagging & Metadata
      │    └──▶ #6 Full-Text Search (needs #2, #5)
      ├──▶ #7 Albums & Favorites
      ├──▶ #8 Document Versioning (needs #1, #2)
      ├──▶ #11 Media Viewer (needs #1, #2)
      └──▶ #13 Secure Notes
```

---

## Issue Definitions

### Issue 1: Core File Storage Infrastructure

- **User Story:** As a system architect, I want a robust file storage abstraction supporting local PostgreSQL and future object storage backends, so that all file operations have a reliable foundation.
- **Key ACs:** Generalized `IFileStorageService`, PostgreSQL large object storage (up to 100MB), chunked uploads, MIME detection, SHA-256 checksums, storage quotas per family, `IStorageProvider` abstraction, `file_management` schema
- **Events:** `FileUploadedEvent`, `FileDeletedEvent`, `StorageQuotaExceededEvent`
- **Tech notes:** Generalize `Common/Infrastructure/Avatar/IFileStorageService.cs`, REST upload/download endpoints alongside GraphQL, RLS on `file_management.*`

### Issue 2: FileManagement Module & Domain Model

- **User Story:** As a family member, I want a dedicated file management system so I can upload, store, and organize family documents and media.
- **Key ACs:** `FileManagementModule : IModule` in Program.cs, `StoredFile` + `Folder` aggregates, Vogen VOs (`FileId`, `FolderId`, `FileName`, `MimeType`, `FileSize`, `StorageKey`), repositories, EF Core configs, GraphQL mutations/queries, `FamilyHub.FileManagement.Tests` project, DbSets in AppDbContext
- **Events:** `FileUploadedEvent`, `FileDeletedEvent`, `FileMovedEvent`, `FileRenamedEvent`, `FolderCreatedEvent`, `FolderDeletedEvent`

### Issue 3: Folder Organization & Hierarchy

- **User Story:** As a family member, I want to organize files into folders/subfolders with intuitive navigation so I can maintain structured storage.
- **Key ACs:** Unlimited nesting, default root folders, breadcrumb nav, grid/list view toggle, drag-and-drop, folder size calculation, recursive delete confirmation, sorting, paginated queries
- **Tech notes:** Materialized path pattern for efficient subtree queries, Angular CDK drag-and-drop

### Issue 4: Virtual Inbox & Auto-Organization Rules

- **User Story:** As a family member, I want to upload files to an inbox that auto-organizes them by rules so I can bulk-upload without manual sorting.
- **Key ACs:** Dedicated Inbox folder, rule conditions (extension, MIME, filename regex, size, date), rule actions (move, tag), zip extraction with per-file rule processing, rule management UI, rule testing/preview, processing log
- **Tech notes:** `inbox_rules` table with conditions/actions JSON, `System.IO.Compression.ZipFile`, background processing

### Issue 5: File Tagging & Metadata (GPS)

- **User Story:** As a family member, I want to tag files and auto-extract GPS location from photos so I can find files by category or location.
- **Key ACs:** Family-scoped tags (name, color), multi-tag per file, EXIF extraction (GPS, camera, capture date), map thumbnail for geotagged images, auto-tag suggestions, tag cloud/filter in sidebar
- **Tech notes:** `MetadataExtractor` NuGet, OpenStreetMap Nominatim for reverse geocoding

### Issue 6: Full-Text Search & Discoverability

- **User Story:** As a family member, I want to find files instantly by searching names, tags, and metadata without browsing folders.
- **Key ACs:** Instant search (300ms debounce), full-text across names/tags/paths, faceted filters, GPS proximity search, recent + saved searches, highlighted results
- **Tech notes:** PostgreSQL `tsvector`/`tsquery` with GIN index, `pg_trgm` for fuzzy matching, `earth_distance`/`cube` for GPS proximity

### Issue 7: Albums & Favorites

- **User Story:** As a family member, I want to group files into albums and mark favorites for quick access without moving them.
- **Key ACs:** Album CRUD with cover image, many-to-many file-album, favorites toggle, dedicated Favorites view, album grid/detail views
- **Tech notes:** `albums` + `album_items` join table, favorites are per-user (not family-scoped)

### Issue 8: Document Versioning

- **User Story:** As a family member, I want version history for files so I never lose document changes.
- **Key ACs:** Auto-version on re-upload, explicit "Upload New Version", version history panel, download/restore any version, text diff for plain text, configurable retention policy
- **Tech notes:** `file_versions` table with `is_current` flag, restore creates new version

### Issue 9: File & Folder Sharing (Internal Permissions)

- **User Story:** As a family member, I want to control which members can view/edit/manage specific files so sensitive documents stay private.
- **Key ACs:** Default family-visible, override to restrict by member/role, View/Edit/Manage levels, folder permission inheritance, lock icon on restricted items, `FileManagementAuthorizationService`
- **Tech notes:** Follow `FamilyAuthorizationService` pattern, `file_permissions` table, RLS integration

### Issue 10: External Sharing (Link-Based Access)

- **User Story:** As a family member, I want to share files with people outside my family via secure links without requiring accounts.
- **Key ACs:** Generate share links with expiration/password/download options, `/share/{token}` public route, folder shares with navigation, access logging, management view, rate limiting
- **Tech notes:** Cryptographic 32-byte token, `share_links` + `share_link_access_log` tables, no RLS on share access

### Issue 11: Media Viewer & Streaming

- **User Story:** As a family member, I want to view images, stream videos, and play audio in the browser without downloading.
- **Key ACs:** Image lightbox (zoom, pan, rotate, gallery nav), slideshow, thumbnail generation, HTML5 video streaming with range requests, audio player with playlist, PDF inline viewer, responsive
- **Tech notes:** `SixLabors.ImageSharp` for thumbnails, FFmpeg for video keyframes, REST streaming with `Accept-Ranges: bytes`

### Issue 12: External Storage Integrations

- **User Story:** As a family member, I want to connect OneDrive/Google Drive/Dropbox/Paperless-ngx so I can manage all files from one interface.
- **Key ACs:** `IExternalStorageProvider` interface, 4 provider implementations, connection management UI, browse external files alongside local, download/upload between providers, selective sync, token refresh
- **Tech notes:** OAuth 2.0 for cloud providers, API token for Paperless-ngx, encrypted token storage (AES-256-GCM)

### Issue 13: Secure Notes (Encrypted)

- **User Story:** As a family member, I want encrypted notes for passwords/PINs with zero-knowledge architecture so my data is safe even if the database is compromised.
- **Key ACs:** Client-side AES-256-GCM encryption, master password (never sent to server), categories (Passwords/Financial/Medical/Personal), auto-lock timeout, copy-to-clipboard with auto-clear, no server-side decryption
- **Tech notes:** Web Crypto API, PBKDF2 (600k iterations) or Argon2id key derivation, sentinel value for password verification

### Issue 14: Remote Zip & Batch Operations

- **User Story:** As a family member, I want to select multiple files for batch operations and download as zip so I can efficiently manage large collections.
- **Key ACs:** Multi-select (checkbox, Shift+click, Ctrl+click), batch toolbar (Move/Delete/Tag/Album/Zip), server-side zip with progress, zip ready notification, max size limit, batch confirmations
- **Tech notes:** `System.IO.Compression.ZipArchive`, background job via Mediator, `zip_jobs` table with TTL auto-cleanup

---

## Execution Order

1. Save spec documentation to `agent-os/specs/2026-02-18-file-management/`
2. Create labels (`service-file-management`, `domain-files`) and milestone (`File Management`)
3. Create epic issue (captures epic number)
4. Create issues 1-14 in order (each references epic and dependencies by real number)
5. Edit epic body to fill in real sub-issue numbers

---

## Verification

- [ ] Milestone "File Management" exists with all 15 issues assigned
- [ ] Epic references all 14 feature issues in Sub-Issues
- [ ] Each feature issue has correct labels
- [ ] Dependencies are correct
- [ ] Spec folder saved at `agent-os/specs/2026-02-18-file-management/`
