# File Management Module — Shaping Notes

**Feature**: Privacy-first family file storage, document vault, and media management
**Created**: 2026-02-18

---

## Scope

The File Management module provides a comprehensive file storage and organization system for families, inspired by Synology Photos/Drive. It encompasses file upload/download, folder hierarchy, tagging, search, media streaming, external integrations, encrypted notes, and batch operations.

### What's In Scope (V1)

1. **Core Storage**: Abstracted `IFileStorageService` with PostgreSQL large object backend, chunked uploads, MIME detection, SHA-256 checksums
2. **Domain Model**: `StoredFile` + `Folder` aggregates with Vogen value objects, EF Core configurations, `file_management` schema
3. **Folder Hierarchy**: Unlimited nesting with materialized path pattern, breadcrumb navigation, grid/list views, drag-and-drop
4. **Virtual Inbox**: Dedicated inbox folder with configurable auto-organization rules (extension, MIME, filename, size, date conditions)
5. **Tagging & Metadata**: Family-scoped tags with colors, EXIF/GPS extraction, map thumbnails, auto-tag suggestions
6. **Full-Text Search**: PostgreSQL `tsvector`/`tsquery` with GIN indexes, faceted filters, GPS proximity search
7. **Albums & Favorites**: Virtual groupings (many-to-many), per-user favorites, cover images
8. **Document Versioning**: Auto-version on re-upload, version history, restore, configurable retention
9. **Internal Permissions**: View/Edit/Manage levels, folder inheritance, `FileManagementAuthorizationService`
10. **External Sharing**: Link-based access with expiration, password, download limits, access logging
11. **Media Viewer**: Image lightbox, video streaming (range requests), audio player, PDF viewer, thumbnails
12. **External Storage**: OneDrive, Google Drive, Dropbox, Paperless-ngx integrations via `IExternalStorageProvider`
13. **Secure Notes**: Zero-knowledge AES-256-GCM encryption, client-side only, master password, categories
14. **Batch Operations**: Multi-select, batch move/delete/tag/album/zip, server-side zip generation

### What's Out of Scope (Future)

1. OCR text extraction from images/PDFs — future enhancement for search
2. AI-powered auto-tagging (object detection, face recognition) — V2 feature
3. Real-time collaborative editing (Google Docs-like) — separate feature
4. S3/MinIO object storage backend — V1 uses PostgreSQL only
5. Mobile-optimized photo capture workflow — Phase 6 mobile app
6. File deduplication (content-addressable storage) — optimization phase
7. Trash/recycle bin with retention — V1 uses immediate delete with confirmation

---

## Decisions

### 1. Storage Backend

**Question**: Where are files stored?

**Answer**: **PostgreSQL Large Objects (V1)**

- Files stored as PostgreSQL large objects (up to 100MB per file)
- `IStorageProvider` abstraction allows future backends (S3, MinIO, local filesystem)
- Storage metadata in `stored_files` table, binary data via `lo_*` functions

**Rationale**: PostgreSQL is already the primary store. Large objects provide transactional consistency with metadata. The `IStorageProvider` interface enables future migration without domain changes.

### 2. Folder Hierarchy Pattern

**Question**: How are nested folders efficiently queried?

**Answer**: **Materialized Path**

- Each folder stores its full path as a string (e.g., `/root/documents/taxes/2026/`)
- Subtree queries use `LIKE '/root/documents/%'` with GIN trigram index
- Move operations update paths of all descendants

**Rationale**: Materialized path is simpler than nested sets or closure tables. Subtree queries are fast with GIN indexes. Move operations are rare relative to reads.

### 3. Search Infrastructure

**Question**: How is full-text search implemented?

**Answer**: **PostgreSQL Native FTS**

- `tsvector` column on `stored_files` for name, tags, path
- GIN index for fast text search
- `pg_trgm` extension for fuzzy matching
- `earth_distance`/`cube` extensions for GPS proximity queries

**Rationale**: PostgreSQL FTS is mature and requires no additional infrastructure. Covers 95% of family file search needs. Elasticsearch would be over-engineering for V1.

### 4. Encryption Strategy for Secure Notes

**Question**: How are sensitive notes encrypted?

**Answer**: **Client-Side Zero-Knowledge Encryption**

- AES-256-GCM encryption in browser via Web Crypto API
- Master password never leaves the client
- Key derivation: PBKDF2 (600k iterations) or Argon2id
- Server stores only ciphertext — cannot decrypt
- Sentinel value for password verification (encrypt known string, verify on unlock)

**Rationale**: Zero-knowledge architecture means a database breach reveals nothing. Even Family Hub operators cannot read user secrets. This is the standard for password managers.

### 5. Media Streaming

**Question**: How are videos streamed?

**Answer**: **REST Endpoints with HTTP Range Requests**

- Dedicated REST endpoints for file download/streaming (not GraphQL)
- `Accept-Ranges: bytes` header for video seeking
- Thumbnail generation via `SixLabors.ImageSharp` (images) and FFmpeg (video keyframes)
- Thumbnails cached alongside originals

**Rationale**: GraphQL is unsuitable for binary streaming. REST with range requests is the HTTP standard for media delivery. Thumbnail pre-generation avoids on-demand processing latency.

### 6. External Storage Integration

**Question**: How do external providers integrate?

**Answer**: **`IExternalStorageProvider` Interface**

- Common interface: List, Download, Upload, Delete, GetMetadata
- OAuth 2.0 for cloud providers (OneDrive, Google Drive, Dropbox)
- API token for self-hosted (Paperless-ngx)
- Encrypted token storage (AES-256-GCM) in `external_connections` table
- Background sync with conflict resolution

**Rationale**: Provider abstraction makes adding new providers trivial. OAuth is standard for cloud APIs. Encrypted token storage protects credentials at rest.

### 7. Permission Model

**Question**: How are file permissions managed?

**Answer**: **Role-Based with Overrides**

- Default: All files visible to family members
- Override: Restrict specific files/folders to specific members or roles
- Three levels: View (read), Edit (modify/rename), Manage (delete/share/permissions)
- Folder permissions inherit to children (overridable)
- RLS policies enforce at database level

**Rationale**: Matches the existing `FamilyAuthorizationService` pattern. Default-open with override-to-restrict is user-friendly. RLS provides defense-in-depth.

### 8. Module Structure

**Question**: How does the module integrate with the existing codebase?

**Answer**: **Standard IModule Pattern**

- `FileManagementModule : IModule` registered in Program.cs
- `file_management` PostgreSQL schema
- DbSets added to shared `AppDbContext`
- Lazy-loaded Angular route at `/files`
- Feature provider: `provideFileManagementFeature()`

**Rationale**: Follows established pattern from Auth/Family modules. Minimal conflict surface (5 shared files to edit).

---

## Technical Constraints

1. **PostgreSQL only**: No additional infrastructure for V1 (no S3, no Elasticsearch, no Redis)
2. **100MB file size limit**: PostgreSQL large objects can handle this; larger files are out of scope
3. **REST for binary**: Upload/download/streaming via REST endpoints, metadata via GraphQL
4. **Client-side encryption**: Secure notes encrypted before reaching server — no server-side key management
5. **EF Core migrations**: Follow existing migration pattern in `src/FamilyHub.Api/Migrations/`
6. **Hot Chocolate GraphQL**: Follow Input→Command pattern (ADR-003) for all mutations
7. **RLS on all tables**: Family-scoped data isolation via PostgreSQL row-level security
8. **Mediator pipeline**: Use martinothamar/Mediator with existing pipeline behaviors

---

## Risks & Mitigations

### Risk: PostgreSQL large object performance at scale

**Mitigation**: 100MB file limit keeps individual objects manageable. Storage quota per family prevents unbounded growth. `IStorageProvider` abstraction enables migration to S3/MinIO when needed.

### Risk: Materialized path performance for deep hierarchies

**Mitigation**: GIN trigram index on path column. Depth limit of 20 levels. Batch path updates for move operations.

### Risk: External provider API rate limits

**Mitigation**: Client-side rate limiting per provider. Background sync with exponential backoff. Token refresh before expiration.

### Risk: Zero-knowledge encryption key loss

**Mitigation**: Clear UX warning about master password being unrecoverable. Optional recovery key generation (encrypted with a second password). No server-side recovery possible by design.

### Risk: Large zip generation overwhelming server

**Mitigation**: Maximum zip size limit (configurable, default 1GB). Background job processing via Mediator. `zip_jobs` table with TTL auto-cleanup. Concurrent zip limit per family.

---

## Visual Reference

- Synology Photos: Album management, timeline view, map view, sharing
- Synology Drive: Folder hierarchy, file versioning, external sharing links
- User-provided mockups: See `visuals/` directory

---

## Success Indicators

### Functional

- [ ] Files can be uploaded, downloaded, moved, renamed, deleted
- [ ] Folder hierarchy supports unlimited nesting with breadcrumb navigation
- [ ] Virtual inbox auto-organizes files by configurable rules
- [ ] Tags and GPS metadata are extracted and searchable
- [ ] Full-text search returns results within 300ms
- [ ] Albums group files without moving them
- [ ] File versions are tracked and restorable
- [ ] Internal permissions restrict access by member/role
- [ ] External share links work without authentication
- [ ] Media viewer streams images, video, audio, and PDF inline
- [ ] External storage providers browseable alongside local files
- [ ] Secure notes encrypted client-side with zero-knowledge architecture
- [ ] Batch operations (move/delete/tag/zip) work on multi-selected files

### Quality

- [ ] >90% unit test coverage on domain logic
- [ ] Integration tests for storage, search, and permissions
- [ ] RLS enforced on all `file_management` schema tables
- [ ] <2s upload time for 10MB files
- [ ] Thumbnail generation <500ms per image

### User Experience

- [ ] Drag-and-drop file organization
- [ ] Grid/list view toggle with persistent preference
- [ ] Responsive design (desktop and tablet)
- [ ] Clear permission indicators (lock icons)
- [ ] Auto-lock on secure notes after timeout
