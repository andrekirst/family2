# Photo Gallery with Grid and Image Views

**Created**: 2026-02-27
**GitHub Issue**: #202
**Spec**: `agent-os/specs/2026-02-27-photo-gallery-grid-image-views/`

## Context

Family members need a way to browse and view family photos. This feature adds a new standalone Photos module with two viewing modes: a responsive thumbnail grid (default browsing) and a large single-photo viewer with prev/next navigation.

## Files to Modify

### New Files (~42)

**Backend Domain** (`src/FamilyHub.Api/Features/Photos/Domain/`):

- `Entities/Photo.cs` — Aggregate root with soft delete, factory method, domain events
- `ValueObjects/PhotoId.cs` — Vogen `[ValueObject<Guid>]`
- `ValueObjects/PhotoCaption.cs` — Vogen `[ValueObject<string>]`, max 500 chars
- `Events/PhotoUploadedEvent.cs`, `PhotoCaptionUpdatedEvent.cs`, `PhotoDeletedEvent.cs`
- `Repositories/IPhotoRepository.cs` — Paginated queries, next/prev navigation

**Backend Application** (`src/FamilyHub.Api/Features/Photos/Application/`):

- Commands: `UploadPhoto`, `UpdatePhotoCaption`, `DeletePhoto` (each with Command, Result)
- Queries: `GetPhotos` (paginated), `GetPhoto`, `GetAdjacentPhotos`
- Handlers for all commands and queries
- `Validators/UploadPhotoCommandValidator.cs`
- `Mappers/PhotoMapper.cs`

**Backend Data/Infrastructure**:

- `Data/PhotoConfiguration.cs` — EF Core config, `photos` schema
- `Infrastructure/Repositories/PhotoRepository.cs`

**Backend GraphQL**:

- `Common/Infrastructure/GraphQL/NamespaceTypes/FamilyPhotosMutation.cs`
- `Features/Photos/GraphQL/PhotoQueries.cs`, `PhotoMutations.cs`

**Backend Models** (`src/FamilyHub.Api/Features/Photos/Models/`):

- `PhotoDto.cs`, `PhotosPageDto.cs`, `AdjacentPhotosDto.cs`
- `UploadPhotoRequest.cs`, `UpdatePhotoCaptionRequest.cs`

**Backend Module**:

- `Features/Photos/PhotosModule.cs`

**Frontend** (`src/frontend/family-hub-web/src/app/features/photos/`):

- `graphql/photos.operations.ts` — Typed GraphQL operations
- `services/photos.service.ts` — Apollo wrapper
- `models/photos.models.ts` — TypeScript interfaces and constants
- `components/photo-grid/photo-grid.component.ts` — Responsive thumbnail grid
- `components/photo-viewer/photo-viewer.component.ts` — Full-size viewer with nav
- `components/photos-page/photos-page.component.ts` — Main page orchestrator
- `photos.routes.ts`, `photos.providers.ts`

**Tests**:

- `tests/FamilyHub.Photos.Tests/FamilyHub.Photos.Tests.csproj`
- `tests/FamilyHub.TestCommon/Fakes/FakePhotoRepository.cs`
- Domain tests: `PhotoAggregateTests.cs`, `PhotoCaptionTests.cs`
- Handler tests: `UploadPhotoCommandHandlerTests.cs`, `DeletePhotoCommandHandlerTests.cs`, `GetPhotosQueryHandlerTests.cs`

### Modified Files (~8)

| File | Change |
|------|--------|
| `src/FamilyHub.Api/Program.cs` | Register `PhotosModule` |
| `src/FamilyHub.Api/Common/Database/AppDbContext.cs` | Add `DbSet<Photo>` |
| `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/FamilyMutation.cs` | Add `Photos()` method |
| `src/FamilyHub.Common/Domain/DomainErrorCodes.cs` | Add photo error codes |
| `src/FamilyHub.Api/FamilyHub.slnx` | Add test project |
| `src/frontend/family-hub-web/src/app/app.routes.ts` | Add `/photos` route |
| `src/frontend/family-hub-web/src/app/app.config.ts` | Spread `providePhotosFeature()` |

## Implementation Tasks

### Task 1: Save Spec, Commit, and Create GitHub Issue

### Task 2: Backend Domain Layer

Create Photo aggregate root, Vogen value objects (PhotoId, PhotoCaption), domain events, and repository interface. Follow CalendarEvent pattern.

### Task 3: Backend Application Layer

Create CQRS commands (Upload, UpdateCaption, Delete), queries (GetPhotos paginated, GetPhoto, GetAdjacentPhotos), handlers, validators, and mappers. Use martinothamar/Mediator with `ValueTask<T>` returns.

### Task 4: Backend Data + Infrastructure

Create EF Core configuration in `photos` schema with indexes on `(FamilyId, CreatedAt)`. Implement `PhotoRepository` with soft-delete filtering. Generate migration.

### Task 5: Backend GraphQL Layer

Create `FamilyPhotosMutation` namespace type. Add queries extending `FamilyQuery` and mutations extending `FamilyPhotosMutation`. Two-step upload: REST binary + GraphQL metadata.

### Task 6: Frontend Service Layer

Create Apollo-wrapped `PhotosService`, typed GraphQL operations, and TypeScript models/constants.

### Task 7: Frontend Photo Grid Component

Responsive thumbnail grid with `grid-cols-2` to `grid-cols-6` breakpoints. Skeleton loading, empty state, "Load More" pagination.

### Task 8: Frontend Image Viewer Component

Full-size photo display with prev/next arrows, keyboard navigation (ArrowLeft/Right, Escape), inline caption editing, photo info panel.

### Task 9: Frontend Main Page + Routes

Orchestrator component with signals-based state for grid/image view switching, pagination, upload, and navigation. Lazy-loaded route at `/photos`.

### Task 10: Tests (~20-25)

Domain aggregate tests, value object tests, command handler tests, query handler tests. FakePhotoRepository in TestCommon.

### Task 11: Module Registration

Register PhotosModule in Program.cs, add DbSet, add route + providers, add slnx entry.

## Task Dependencies

```
Task 2 (domain) → Task 3 (application) → Task 4 (data) → Task 5 (GraphQL) → Task 11 (registration)
                                                                                ↓
                                          Task 6 (frontend service) → Task 7 (grid) → Task 9 (page)
                                                                    → Task 8 (viewer) ↗
Task 10 (tests) — parallel after Tasks 2-3
```
