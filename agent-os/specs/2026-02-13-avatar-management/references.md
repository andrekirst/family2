# References

## Domain Model

- `src/FamilyHub.Api/Features/Auth/Domain/Entities/User.cs` — User aggregate, will add AvatarId property
- `src/FamilyHub.Api/Features/Family/Domain/Entities/FamilyMember.cs` — FamilyMember entity, will add AvatarId override
- `src/FamilyHub.Api/Features/Family/Domain/Entities/Family.cs` — Family aggregate root
- `src/FamilyHub.Api/Common/Domain/AggregateRoot.cs` — Base class for aggregates
- `src/FamilyHub.Api/Common/Domain/DomainEvent.cs` — Base record for domain events
- `src/FamilyHub.Api/Common/Domain/DomainException.cs` — Domain invariant violations

## Value Objects

- `src/FamilyHub.Api/Features/Family/Domain/ValueObjects/FamilyId.cs` — Vogen Guid VO pattern (reference for AvatarId, FileId)
- `src/FamilyHub.Api/Features/Family/Domain/ValueObjects/FamilyName.cs` — Vogen string VO pattern
- `src/FamilyHub.Api/Common/Domain/ValueObjects/Email.cs` — Existing common VO

## Command/Handler Pattern

- `src/FamilyHub.Api/Features/Family/Application/Commands/CreateFamily/` — Command folder structure pattern (Command.cs, Handler.cs, MutationType.cs, Validator.cs, Result.cs)
- `src/FamilyHub.Api/Features/Family/Application/Commands/SendInvitation/` — File upload-adjacent pattern (complex handler with multiple service interactions)

## GraphQL Pattern

- `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/FamilyMutation.cs` — Mutation namespace type
- `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/FamilyQuery.cs` — Query namespace type

## DTOs

- `src/FamilyHub.Api/Features/Auth/Application/DTOs/UserDto.cs` — Will add AvatarId
- `src/FamilyHub.Api/Features/Family/Application/DTOs/FamilyMemberDto.cs` — Will add AvatarId

## Repository Pattern

- `src/FamilyHub.Api/Features/Family/Domain/Repositories/IFamilyRepository.cs` — Domain repository interface
- `src/FamilyHub.Api/Features/Family/Infrastructure/Repositories/FamilyRepository.cs` — EF Core implementation

## EF Configuration Pattern

- `src/FamilyHub.Api/Features/Family/Data/FamilyConfiguration.cs` — EF entity configuration
- `src/FamilyHub.Api/Features/Auth/Data/UserConfiguration.cs` — User EF configuration (will add AvatarId mapping)

## Database

- `src/FamilyHub.Api/Common/Database/AppDbContext.cs` — Single DbContext, will add Avatar/AvatarVariant/FileMetadata DbSets

## Module Registration

- `src/FamilyHub.Api/Program.cs` — Module registration (may need MapControllers for REST endpoint)
- `src/FamilyHub.Api/Features/Family/FamilyModule.cs` — Family module DI registrations

## Infrastructure

- `src/FamilyHub.Api/Common/Infrastructure/` — Existing infrastructure patterns (behaviors, GraphQL, security)

## Frontend

- `src/frontend/family-hub-web/src/app/features/family/components/members-list/` — Will add avatar display
- `src/frontend/family-hub-web/src/app/features/family/components/family-settings/` — Will add avatar upload
- `src/frontend/family-hub-web/src/app/core/` — Avatar components will live here (cross-cutting)
- `src/frontend/family-hub-web/src/app/shared/components/top-bar/` — Will add current user avatar
- `src/frontend/family-hub-web/src/app/features/family/graphql/family.operations.ts` — GraphQL operations pattern

## Testing

- `tests/FamilyHub.TestCommon/Fakes/` — Shared fake repositories (pattern for FakeFileStorageService, FakeAvatarRepository)
- `tests/FamilyHub.Family.Tests/` — Family module test project
- `tests/FamilyHub.Auth.Tests/` — Auth module test project

## NuGet Packages (to add)

- `SixLabors.ImageSharp` — Image processing (resize, crop, format conversion)

## NPM Packages (to add)

- `ngx-image-cropper` — Frontend crop tool for avatar upload
