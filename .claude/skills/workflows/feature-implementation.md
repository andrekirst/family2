---
name: feature-implementation
description: Complete feature implementation workflow from spec to tests
category: workflows
module-aware: true
inputs:
  - featureName: Feature name (e.g., family-invites)
  - module: DDD module name
  - issueNumber: GitHub issue number
---

# Feature Implementation Workflow

End-to-end workflow for implementing a complete feature in Family Hub.

## Phase 1: Context Loading

1. Load module profile: `agent-os/profiles/modules/{module}.yaml`
2. Load relevant standards from `agent-os/standards/`
3. Check related documentation from profile

## Phase 2: Domain Layer

### 2.1 Value Objects

If needed, create Vogen value objects:

```
Invoke skill: backend/value-object
- module: {module}
- valueObjectName: {Name}
- baseType: string|Guid|int
```

### 2.2 Entities/Aggregates

Create or update entity in `Domain/Entities/`:

```csharp
public class {Entity} : AggregateRoot
{
    public {EntityId} Id { get; private set; }
    // Properties with Vogen types

    public static {Entity} Create({VogenType} param)
    {
        var entity = new {Entity} { ... };
        entity.RaiseDomainEvent(new {Entity}CreatedEvent(...));
        return entity;
    }
}
```

### 2.3 Domain Events

Create event in `Domain/Events/`:

```csharp
public sealed record {Entity}CreatedEvent(
    {EntityId} Id,
    // Event properties
    DateTime CreatedAt
);
```

**Note:** Domain events are plain records (no MediatR `INotification`). They are raised via `RaiseDomainEvent()` on aggregates and cleared with `ClearDomainEvents()`.

### 2.4 Repository Interface

Add to `Domain/Repositories/`:

```csharp
public interface I{Entity}Repository
{
    Task<{Entity}?> GetByIdAsync({EntityId} id, CancellationToken ct);
    Task AddAsync({Entity} entity, CancellationToken ct);
}
```

## Phase 3: Application Layer (Subfolder-per-Command)

### 3.1 Commands

Each command gets its own subfolder under `Application/Commands/{Name}/`:

```
Commands/{Name}/
├── {Name}Command.cs        # Record with command data
├── {Name}CommandHandler.cs  # Wolverine static handler
├── {Name}CommandValidator.cs # FluentValidation (if needed)
├── {Name}Result.cs          # Result type (if needed)
└── MutationType.cs          # Hot Chocolate mutation type
```

**Wolverine handler pattern** (static class, auto-discovered):

```csharp
public static class {Name}CommandHandler
{
    public static async Task<{Result}> Handle(
        {Name}Command command,
        I{Entity}Repository repository,
        // Other dependencies injected as parameters
        CancellationToken ct)
    {
        // Implementation
    }
}
```

### 3.2 Queries

Each query gets its own subfolder under `Application/Queries/{Name}/`:

```
Queries/{Name}/
├── {Name}Query.cs           # Record with query parameters
├── {Name}QueryHandler.cs    # Wolverine static handler
└── QueryType.cs             # Hot Chocolate query type
```

### 3.3 Validators (if needed)

```csharp
public class {Name}CommandValidator
    : AbstractValidator<{Name}Command>
{
    public {Name}CommandValidator()
    {
        // Validation rules
    }
}
```

### 3.4 Permissions Check

If the feature involves authorized actions:

- Add permission methods to the relevant Role VO (e.g., `FamilyRole.CanDoAction()`)
- Add permission string to `GetPermissions()` using format `{module}:{action}`
- Use `FamilyAuthorizationService` (or equivalent) for backend enforcement

## Phase 4: Persistence Layer

### 4.1 EF Core Configuration

Create `Data/{Entity}Configuration.cs`:

```csharp
public class {Entity}Configuration : IEntityTypeConfiguration<{Entity}>
{
    public void Configure(EntityTypeBuilder<{Entity}> builder)
    {
        builder.ToTable("{entities}");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(new {EntityId}.EfCoreValueConverter());
    }
}
```

### 4.2 Repository Implementation

Create `Infrastructure/Repositories/{Entity}Repository.cs`

### 4.3 Migration

```
Invoke skill: database/ef-migration
- migrationName: Add{Entity}Table
- module: {module}
```

## Phase 5: Presentation Layer (GraphQL)

### 5.1 Mutations

Add to `GraphQL/{Module}Mutations.cs`:

```csharp
[MutationType]
public static class {Module}Mutations
{
    // Wolverine dispatches to handler automatically
}
```

Or use `MutationType.cs` inside each command subfolder for auto-registration.

### 5.2 Queries

Add to `GraphQL/{Module}Queries.cs` or use `QueryType.cs` inside each query subfolder.

## Phase 6: Frontend

### 6.1 Components

```
Invoke skill: frontend/angular-component
- componentName: {entity}-form
- hasGraphQL: true
```

Use `inject()` for all dependency injection (not constructor injection).

### 6.2 GraphQL Operations

Create in `src/app/features/{module}/graphql/`:

```typescript
const CREATE_{ENTITY} = gql`
  mutation Create{Entity}($input: Create{Entity}Input!) {
    create{Entity}(input: $input) { id }
  }
`;
```

### 6.3 Permission-Gated UI

If the feature has authorized actions:

- Inject the relevant permission service (e.g., `FamilyPermissionService`)
- Wrap restricted UI elements in `@if (permissions.canDoAction())`
- Pattern: **HIDE** unauthorized actions (never disable+tooltip)

### 6.4 Routes

Add to module routing.

## Phase 7: Testing

### 7.1 Unit Tests

For each handler — use **fake repository pattern** (inner classes implementing interfaces):

```
Invoke skill: testing/unit-test
- className: {Name}CommandHandler
- module: {module}
```

### 7.2 E2E Tests

```
Invoke skill: testing/playwright-test
- feature: {feature-name}
- module: {module}
```

## Phase 8: Verification

### 8.1 Build Check

```bash
dotnet build src/FamilyHub.Api/FamilyHub.Api.csproj
cd src/frontend/family-hub-web && ng build
```

### 8.2 Test Check

```bash
dotnet test tests/FamilyHub.UnitTests/FamilyHub.UnitTests.csproj --verbosity normal
```

### 8.3 Manual Verification

1. Start infrastructure: `docker-compose up -d`
2. Start backend: `dotnet run --project src/FamilyHub.Api/FamilyHub.Api.csproj`
3. Start frontend: `cd src/frontend/family-hub-web && ng serve`
4. Test feature manually

## Phase 9: Commit

```bash
git add [specific files]
git commit -m "feat({module}): implement {feature-name} (#{issueNumber})

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

## Checklist

- [ ] Value objects created with validation
- [ ] Entity follows aggregate pattern
- [ ] Domain events raised (not MediatR INotification)
- [ ] Wolverine command/query handlers (static classes)
- [ ] Subfolder-per-command layout followed
- [ ] EF Core configuration in `Data/` folder
- [ ] Repository in `Infrastructure/Repositories/`
- [ ] GraphQL types and operations added
- [ ] Permission methods added (if authorized action)
- [ ] Frontend permission service updated (if authorized action)
- [ ] UI hides unauthorized actions (never disable)
- [ ] Frontend uses `inject()` for DI
- [ ] Unit tests with fake repositories pass
- [ ] Build succeeds (backend + frontend)
- [ ] Manual verification complete
