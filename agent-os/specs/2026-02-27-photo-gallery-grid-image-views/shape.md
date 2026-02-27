# Photo Gallery with Grid and Image Views — Shaping Notes

**Feature**: Photo gallery with grid (thumbnails) and image (full-size viewer) modes
**Created**: 2026-02-27
**GitHub Issue**: #202

---

## Scope

**As a** family member, **I want** a photo gallery with grid and image views, **so that** I can browse and view family photos easily.

### In Scope

- New standalone Photos module (backend + frontend)
- Grid view: responsive thumbnail tiles, paginated (30 per page)
- Image view: full-size viewer with prev/next navigation, keyboard support
- Photo upload (two-step: REST binary + GraphQL metadata)
- Caption editing (inline in image view)
- Soft delete with permission checks
- Family-scoped access (all family members can view)

### Out of Scope (MVP)

- Albums/collections (flat gallery only)
- Photo editing/cropping
- Batch operations
- External storage integration
- Comments/reactions on photos

## Decisions

- **Standalone module**: Created as a new Photos module rather than extending the existing FileManagement module, following bounded context principles
- **Grid + Image only**: No list view — grid is the natural browsing mode for photos, image view for detailed viewing
- **Two-step upload**: REST for binary (avoids base64 bloat in GraphQL), GraphQL for metadata registration
- **Cursor-based navigation**: Image viewer next/prev uses `CreatedAt` ordering with `PhotoId` tiebreaker
- **Soft delete**: Photos are soft-deleted (`IsDeleted` flag) rather than hard-deleted
- **Family-scoped**: All photos belong to a family; no per-photo sharing in MVP
- **Pagination**: Offset-based (`skip/take`) for grid view simplicity; cursor-based for next/prev in viewer

## Context

- **Visuals**: None provided. Agreed on ASCII mockups:
  - Grid: Responsive tiles (2-6 columns based on breakpoint)
  - Image: Centered large photo with left/right arrows and close button
- **References**: Calendar module studied as the primary reference implementation
- **Product alignment**: Photos are Phase 1 priority (RICE scores 38-42 for photo-related features in backlog)
- **Existing infrastructure**: FileManagement module exists with albums, thumbnails, storage — but user chose separate module

## Standards Applied

- **architecture/ddd-modules** — New Photos bounded context with IModule pattern
- **backend/graphql-input-command** — Input DTOs (primitives) → Commands (Vogen) → Results
- **backend/vogen-value-objects** — PhotoId, PhotoCaption with EfCoreValueConverter
- **frontend/angular-components** — Standalone components with signals
- **frontend/apollo-graphql** — Apollo Client with typed operations
- **database/ef-core-migrations** — EF Core config in `photos` schema
- **testing/unit-testing** — xUnit + FluentAssertions + fake repos
- **backend/permission-system** — Family membership-based access, HIDE pattern for UI
