# Avatar Management - Implementation Plan

**Created**: 2026-02-13
**Issue**: #140
**Branch**: `feature/avatar-management`
**Spec Folder**: `agent-os/specs/2026-02-13-avatar-management/`

---

## Overview

**User Story**: As a family member, I want to manage my avatar so that I have a visual identity displayed across the entire application (nav bar, members list, invitations, all user contexts).

**Current State**:

- Users displayed as text-only (name + email) everywhere
- No avatar, no file storage infrastructure, no image processing in the codebase
- No REST endpoints (only GraphQL)

**What We're Building**:

1. File storage abstraction (`IFileStorageService`) with PostgreSQL large objects backend
2. Image processing service (resize, crop, format optimization via SixLabors.ImageSharp)
3. Avatar aggregate with 4 size variants (Tiny 24px, Small 48px, Medium 128px, Large 512px)
4. Global avatar on `User` (Auth) + optional per-family override on `FamilyMember` (Family)
5. REST endpoint for avatar serving with browser caching (ETag, Cache-Control)
6. Frontend avatar display component with initials fallback
7. Frontend avatar upload component with crop tool
8. Integration across all views (nav bar, members list, family settings, invitations)

---

## 11 Implementation Tasks

1. Save spec documentation
2. Add NuGet packages & infrastructure types (IFileStorageService, IAvatarProcessingService, AvatarId/FileId VOs)
3. Create Avatar domain model + database schema (Avatar aggregate, AvatarVariant, FileMetadata, EF configs, migration)
4. Update User aggregate (Auth) + FamilyMember (Family) with AvatarId, domain events, DTOs
5. Avatar management commands (UploadAvatar, RemoveAvatar, SetFamilyAvatar in Family module)
6. REST avatar endpoint (GET /api/avatars/{avatarId}/{size} with caching)
7. Update existing GraphQL queries (GetCurrentUser, GetFamilyMembers include AvatarId)
8. Frontend avatar core components (display, upload with crop, service, GraphQL operations)
9. Frontend integration across views (members list, family settings, top bar, invitation accept)
10. Backend tests (shared fakes, handler tests, domain logic tests)
11. Frontend tests (avatar service spec, component spec)

---

## Architecture

```
                  Common Layer
  IFileStorageService  <-  PostgresFileStorageService
  IAvatarProcessingService <- AvatarProcessingService
  Avatar aggregate (AvatarId, variants, metadata)
  AvatarController (REST: GET /api/avatars/{id}/{size})
            |                      |
   Auth Module              Family Module
   User.AvatarId?           FamilyMember.AvatarId?
   (global avatar)          (per-family override)
   Updated via              UploadAvatar cmd
   domain event             RemoveAvatar cmd
                            SetFamilyAvatar cmd
```

**Avatar serving**: REST endpoint (`GET /api/avatars/{avatarId}/{size}`) for browser caching (ETag, Cache-Control) and native `<img src>` support.

**Initials fallback**: Client-side generation (first letter of first + last name), no server call needed.

---

## Key Design Decisions

- **Avatar types**: Image upload + auto-generated initials fallback (client-side)
- **Storage**: `IFileStorageService` abstraction + PostgreSQL large objects (Phase 1)
- **Image processing**: Full crop tool, 4 size variants, format optimization (ImageSharp)
- **Size variants**: Tiny (24x24), Small (48x48), Medium (128x128), Large (512x512)
- **Module ownership**: Family module owns avatar management commands
- **Avatar scope**: Global avatar on User (Auth) + optional per-family override on FamilyMember (Family)
- **Serving**: REST endpoint (not GraphQL) for browser caching and native img support
- **Security**: MIME type validation, max 5MB, max 4096x4096, content validation via ImageSharp

## File Upload Validation

| Check | Constraint |
|-------|-----------|
| MIME type | `image/jpeg`, `image/png`, `image/webp` only |
| File size | Max 5 MB |
| Dimensions | Max 4096x4096 |
| Content validation | ImageSharp parses to verify actual image (not just extension) |
