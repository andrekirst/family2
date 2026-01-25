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
        entity.AddDomainEvent(new {Entity}CreatedEvent(...));
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
) : INotification;
```

### 2.4 Repository Interface

Add to `Domain/Repositories/`:

```csharp
public interface I{Entity}Repository
{
    Task<{Entity}?> GetByIdAsync({EntityId} id, CancellationToken ct);
    Task AddAsync({Entity} entity, CancellationToken ct);
}
```

## Phase 3: Application Layer

### 3.1 Commands

```
Invoke skill: backend/graphql-mutation
- mutationName: Create{Entity}
- module: {module}
- fields: [list of fields]
```

### 3.2 Queries

```
Invoke skill: backend/graphql-query
- queryName: Get{Entity}
- module: {module}
```

### 3.3 Validators (if needed)

```csharp
public class Create{Entity}CommandValidator
    : AbstractValidator<Create{Entity}Command>
{
    public Create{Entity}CommandValidator()
    {
        // Validation rules
    }
}
```

## Phase 4: Persistence Layer

### 4.1 EF Core Configuration

Create `Persistence/Configurations/{Entity}Configuration.cs`:

```csharp
public class {Entity}Configuration : IEntityTypeConfiguration<{Entity}>
{
    public void Configure(EntityTypeBuilder<{Entity}> builder)
    {
        builder.ToTable("{entities}", "{module}");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(new {EntityId}.EfCoreValueConverter());
    }
}
```

### 4.2 Repository Implementation

Create `Persistence/Repositories/{Entity}Repository.cs`

### 4.3 Migration

```
Invoke skill: database/ef-migration
- migrationName: Add{Entity}Table
- module: {module}
```

## Phase 5: Presentation Layer

### 5.1 GraphQL Types

Create `Presentation/GraphQL/Types/{Entity}Type.cs`

### 5.2 Mutations/Queries

Already created by mutation/query skills.

## Phase 6: Frontend

### 6.1 Components

```
Invoke skill: frontend/angular-component
- componentName: {entity}-form
- atomicLevel: organisms
- hasGraphQL: true
```

### 6.2 GraphQL Operations

Create in `src/app/{module}/graphql/`:

```typescript
const CREATE_{ENTITY} = gql`
  mutation Create{Entity}($input: Create{Entity}Input!) {
    create{Entity}(input: $input) { id }
  }
`;
```

### 6.3 Routes

Add to module routing.

## Phase 7: Testing

### 7.1 Unit Tests

For each handler:

```
Invoke skill: testing/unit-test
- className: Create{Entity}CommandHandler
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
dotnet build
npm run build
```

### 8.2 Test Check

```bash
dotnet test
npm test
npx playwright test
```

### 8.3 Manual Verification

1. Start infrastructure: `docker-compose up -d`
2. Start backend: `dotnet run`
3. Start frontend: `npm start`
4. Test feature manually

## Phase 9: Commit

```bash
git add .
git commit -m "feat({module}): implement {feature-name} (#issueNumber)

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

## Checklist

- [ ] Value objects created with validation
- [ ] Entity follows aggregate pattern
- [ ] Domain events emitted
- [ ] Command/Query handlers complete
- [ ] EF Core configuration correct
- [ ] Migration applied
- [ ] GraphQL types and operations added
- [ ] Frontend component created
- [ ] Unit tests pass
- [ ] E2E tests pass
- [ ] Manual verification complete
