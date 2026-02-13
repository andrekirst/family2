# Applicable Standards

## Backend

- `backend/graphql-input-command.md` — Input -> Command pattern for GraphQL mutations (UploadAvatar, RemoveAvatar, SetFamilyAvatar)
- `backend/vogen-value-objects.md` — Vogen VOs with EfCoreValueConverter (AvatarId, FileId)
- `backend/domain-events.md` — DomainEvent records, raised in aggregates, published after save (UserAvatarChangedEvent, UserAvatarRemovedEvent)
- `backend/permission-system.md` — FamilyRole permissions for avatar management authorization
- `backend/user-context.md` — User context for identifying current user in avatar operations

## Frontend

- `frontend/angular-components.md` — Standalone components, Angular Signals (avatar display, avatar upload)
- `frontend/apollo-graphql.md` — Apollo service, gql tagged templates (upload/remove mutations, query updates)

## Database

- `database/ef-core-migrations.md` — EF Core migrations for Avatar, AvatarVariant, FileMetadata tables

## Testing

- `testing/unit-testing.md` — xUnit + FluentAssertions, fake repositories (FakeFileStorageService, FakeAvatarRepository)
