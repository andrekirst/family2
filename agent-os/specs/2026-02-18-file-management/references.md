# References for File Management Module

This document captures the code references studied during the exploration phase to inform the File Management module implementation.

---

## Core Infrastructure References

### 1. IModule Pattern

**Location**: `src/FamilyHub.Api/Common/Modules/IModule.cs`

**Relevance**: `FileManagementModule` implements `IModule` with self-contained DI registrations. Registered via `builder.Services.RegisterModule<FileManagementModule>(configuration)` in Program.cs.

**Reference Implementation**: `src/FamilyHub.Api/Features/Family/FamilyModule.cs`

### 2. Existing IFileStorageService

**Location**: `src/FamilyHub.Api/Common/Infrastructure/Avatar/IFileStorageService.cs`

**Relevance**: The existing avatar storage service will be generalized into a module-agnostic `IFileStorageService` with the `IStorageProvider` abstraction. Current implementation handles image upload/download — new implementation extends to all file types with chunked uploads, MIME detection, and SHA-256 checksums.

### 3. FamilyAuthorizationService

**Location**: `src/FamilyHub.Api/Features/Family/Application/Services/FamilyAuthorizationService.cs`

**Relevance**: `FileManagementAuthorizationService` follows this exact pattern for file/folder permission checks. View/Edit/Manage permission levels map to the Family module's role-based permission system.

### 4. AppDbContext

**Location**: `src/FamilyHub.Api/Common/Database/AppDbContext.cs`

**Relevance**: File management entities (DbSets) are added to the shared `AppDbContext`. Entity configurations use `file_management` schema. Domain events published via existing `SaveChangesAsync()` pipeline.

---

## Domain Model References

### 5. AggregateRoot Base Class

**Location**: `src/FamilyHub.Api/Common/Domain/AggregateRoot.cs`

**Relevance**: `StoredFile`, `Folder`, `Album`, and `SecureNote` aggregates inherit from `AggregateRoot<T>`. Domain events raised via `RaiseDomainEvent()`.

### 6. DomainEvent Base Record

**Location**: `src/FamilyHub.Api/Common/Domain/DomainEvent.cs`

**Relevance**: File management events (`FileUploadedEvent`, `FileDeletedEvent`, etc.) extend `DomainEvent` base record.

### 7. Family Aggregate (Pattern Reference)

**Location**: `src/FamilyHub.Api/Features/Family/Domain/Family.cs`

**Relevance**: Shows aggregate design patterns: static factory methods, domain event raising, value object usage, entity collections.

---

## Value Object References

### 8. FamilyId / FamilyName (Vogen Pattern)

**Location**: `src/FamilyHub.Api/Features/Family/Domain/ValueObjects/`

**Relevance**: File management value objects (`FileId`, `FolderId`, `FileName`, `MimeType`, `FileSize`, `StorageKey`) follow the same Vogen pattern with `EfCoreValueConverter` and validation.

### 9. InvitationToken

**Location**: `src/FamilyHub.Api/Features/Family/Domain/ValueObjects/InvitationToken.cs`

**Relevance**: `ShareToken` for external sharing follows the same cryptographic token pattern — 32-byte random token, URL-safe base64 encoding.

---

## CQRS References

### 10. Command/Handler Pattern

**Location**: `src/FamilyHub.Api/Features/Family/Commands/CreateFamily/`

**Relevance**: File management commands follow the subfolder pattern: `Commands/{Name}/Command.cs, Handler.cs, MutationType.cs, Validator.cs`. Each command uses Vogen types, handler returns result record.

### 11. Query Pattern

**Location**: `src/FamilyHub.Api/Features/Family/Queries/`

**Relevance**: File management queries (GetFiles, GetFolderContents, SearchFiles) follow the same query handler pattern.

---

## GraphQL References

### 12. Mutation Type Extensions

**Location**: `src/FamilyHub.Api/Features/Family/Commands/CreateFamily/MutationType.cs`

**Relevance**: File management mutations registered as `[ExtendObjectType("Mutation")]` with Input→Command mapping. REST endpoints added separately for binary operations.

### 13. Query Type Extensions

**Location**: `src/FamilyHub.Api/Features/Family/Queries/`

**Relevance**: File management queries registered as `[ExtendObjectType("Query")]` with pagination support.

---

## Repository References

### 14. IFamilyRepository + FamilyRepository

**Location**: `src/FamilyHub.Api/Features/Family/Domain/Repositories/IFamilyRepository.cs`

**Relevance**: File management repositories (`IStoredFileRepository`, `IFolderRepository`, `IAlbumRepository`, etc.) follow interface + implementation pattern with primary constructor taking `AppDbContext`.

### 15. FakeFamilyRepository (Test Reference)

**Location**: `tests/FamilyHub.TestCommon/Fakes/FakeFamilyRepository.cs`

**Relevance**: Shared fake repositories in `FamilyHub.TestCommon` extended with `FakeStoredFileRepository`, `FakeFolderRepository`, etc.

---

## EF Core Configuration References

### 16. FamilyConfiguration

**Location**: `src/FamilyHub.Api/Features/Family/Data/FamilyConfiguration.cs`

**Relevance**: File management entity configurations follow this pattern — manual `.HasConversion()` for Vogen types, schema prefix, index definitions.

---

## RLS References

### 17. Existing RLS Policies

**Location**: `src/FamilyHub.Api/Migrations/` (AddRlsPolicies migration)

**Relevance**: File management RLS migration adds `family_isolation` policies to all `file_management` schema tables using `app.current_family_id` session variable.

---

## Frontend References

### 18. Feature Provider Pattern

**Location**: `src/frontend/family-hub-web/src/app/app.config.ts`

**Relevance**: `provideFileManagementFeature()` spread into the app config, following `provideCalendarFeature()` and `provideFamilyFeature()` patterns.

### 19. Lazy-Loaded Routes

**Location**: `src/frontend/family-hub-web/src/app/app.routes.ts`

**Relevance**: File management route added as `loadChildren` entry with group-level `authGuard`.

### 20. FamilyPermissionService

**Location**: `src/frontend/family-hub-web/src/app/core/permissions/`

**Relevance**: `FilePermissionService` follows the same computed signals pattern for file-level permission checks in the UI.

---

## Documentation References

### Architecture Documents

- `docs/architecture/domain-model-microservices-map.md` — Domain 7 (Document & Info Vault) definition
- `docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md` — Modular monolith strategy
- `docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md` — Input→Command separation

### Product Strategy

- `docs/product-strategy/FEATURE_BACKLOG.md` — Phase 2, Domain 7 feature list
- `docs/product-strategy/implementation-roadmap.md` — Phase 2 timeline

### Standards

- `agent-os/standards/backend/graphql-input-command.md`
- `agent-os/standards/backend/domain-events.md`
- `agent-os/standards/backend/vogen-value-objects.md`
- `agent-os/standards/database/ef-core-migrations.md`
- `agent-os/standards/database/rls-policies.md`
- `agent-os/standards/architecture/ddd-modules.md`
- `agent-os/standards/architecture/event-chains.md`
- `agent-os/standards/frontend/angular-components.md`
- `agent-os/standards/frontend/apollo-graphql.md`
- `agent-os/standards/testing/unit-testing.md`

---

## Key Architectural Insights

### 1. Single AppDbContext

The codebase uses a single `AppDbContext`, not per-module contexts. File management entities are added as DbSets to this shared context with `file_management` schema prefix.

### 2. Mediator Pipeline

martinothamar/Mediator with pipeline behaviors: DomainEventPublishing(100) → Logging(200) → Validation(300) → Transaction(400). File management handlers participate in this pipeline automatically.

### 3. REST + GraphQL Hybrid

File management is the first module requiring REST endpoints alongside GraphQL. Binary upload/download/streaming use ASP.NET Core minimal APIs or controllers. Metadata CRUD uses Hot Chocolate GraphQL.

### 4. IModule Conflict Surface

Adding the File Management module touches exactly 5 shared files:

1. `Program.cs` — `RegisterModule<FileManagementModule>()`
2. `AppDbContext.cs` — new DbSet properties
3. `FamilyHub.slnx` — new project entry
4. `app.routes.ts` — `loadChildren` entry
5. `app.config.ts` — provider spread

### 5. Generalize Before Specialize

The existing `IFileStorageService` (avatar-specific) should be generalized before building the file management module. This prevents duplicate storage infrastructure and ensures all file operations share the same abstraction.
