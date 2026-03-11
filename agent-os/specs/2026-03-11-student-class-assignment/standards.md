# Standards for Student Class Assignment

The following standards apply to this work.

---

## ddd-modules

DDD module structure with bounded contexts (feature-folder layout).

### Module Layout

```
Features/{ModuleName}/
├── Domain/
│   ├── Entities/          # Aggregates
│   ├── ValueObjects/      # Vogen types
│   ├── Events/            # Domain events
│   └── Repositories/      # Repository interfaces
├── Application/
│   ├── Commands/          # Write operations
│   ├── Queries/           # Read operations
│   ├── Mappers/           # DTO mappers
│   └── Services/          # Application services
├── Data/                  # EF Core configurations
├── Infrastructure/
│   └── Repositories/      # Repository implementations
├── Models/                # DTOs and request models
└── {ModuleName}Module.cs  # IModule registration
```

### Rules

- One PostgreSQL schema per module (schema: `school`)
- No direct module dependencies — reference IDs only
- Event-driven cross-module communication
- Cross-cutting concerns in `Common/`

---

## graphql-input-command

ADR-003 pattern separating Input DTOs from Commands with subfolder-per-command layout.

### File Organization

```
Commands/{Name}/
  {Name}Command.cs
  {Name}CommandHandler.cs
  {Name}CommandValidator.cs
  {Name}BusinessValidator.cs  (optional, for complex business rules)
  {Name}Result.cs
  MutationType.cs
```

### Rules

- Input DTOs in `Models/` with primitives
- Commands in `Commands/{Name}/` with Vogen types
- One MutationType per command (not centralized)
- Dispatch via `ICommandBus.SendAsync()`

---

## vogen-value-objects

Vogen value objects with EfCoreValueConverter for type safety.

### Pattern

```csharp
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct SchoolId
{
    public static SchoolId New() => From(Guid.NewGuid());
}
```

### Rules

- Always include `conversions: Conversions.EfCoreValueConverter`
- Implement `Validate()` for business rules
- Location: `Domain/ValueObjects/{Name}.cs`

---

## permission-system

Role-based permissions with VO methods, defense-in-depth enforcement.

### Permission String Format

```
{module}:{action}
```

New permissions for this feature: `school:manage-schools`

### Rules

- VO methods on FamilyRole: `CanManageSchools() => Value is "Owner" or "Admin"`
- Backend: Check permission in handlers/validators
- Frontend: `FamilyPermissionService` with computed signals hides UI
- Always HIDE unauthorized actions (never disable+tooltip)

---

## angular-components

Standalone components with inject() DI and computed signals.

### Rules

- Always use `standalone: true`
- Use Angular Signals for state
- Use `inject()` for dependency injection
- `ChangeDetectionStrategy.OnPush`

---

## apollo-graphql

Apollo Client with typed GraphQL operations.

### Rules

- Use `inject(Apollo)` for dependency injection
- Handle errors with catchError
- Use typed operations (gql tagged templates)
- Operations in `graphql/{feature}.operations.ts`

---

## unit-testing

xUnit + FluentAssertions with fake repository pattern.

### Rules

- FluentAssertions for all assertions
- Fake repositories in `FamilyHub.TestCommon/Fakes/` for cross-cutting
- Arrange-Act-Assert pattern
- Call static `Handler.Handle()` directly with fakes
- Per-module test projects: `tests/FamilyHub.School.Tests/`

---

## ef-core-migrations

DbUp SQL migrations with schema-per-module.

### Rules

- Migration files in `Database/Migrations/{schema}/`
- Schema name = module name (lowercase): `school`
- All columns nullable for address (owned type on existing table)
- FK constraints for referential integrity
- Unique constraints where needed
