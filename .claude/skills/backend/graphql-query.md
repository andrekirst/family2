---
name: graphql-query
description: Create GraphQL query with MediatR query handler
category: backend
module-aware: true
inputs:
  - queryName: PascalCase query name (e.g., GetFamily)
  - module: DDD module name
  - returnType: Return type (single entity or collection)
---

# GraphQL Query Skill

Create a GraphQL query with MediatR query handler following CQRS pattern.

## Context

Load module profile: `agent-os/profiles/modules/{module}.yaml`

## Steps

### 1. Create Query

**Location:** `Modules/FamilyHub.Modules.{Module}/Application/Queries/{QueryName}/{QueryName}Query.cs`

```csharp
using MediatR;
using FamilyHub.Modules.{Module}.Domain.ValueObjects;

namespace FamilyHub.Modules.{Module}.Application.Queries.{QueryName};

public sealed record {QueryName}Query(
    {EntityId} Id
) : IRequest<{QueryName}Result?>;
```

### 2. Create Query Result

**Location:** `Modules/FamilyHub.Modules.{Module}/Application/Queries/{QueryName}/{QueryName}Result.cs`

```csharp
namespace FamilyHub.Modules.{Module}.Application.Queries.{QueryName};

public sealed record {QueryName}Result(
    {EntityId} Id,
    {PropertyType} Property1,
    DateTime CreatedAt
);
```

### 3. Create Query Handler

**Location:** `Modules/FamilyHub.Modules.{Module}/Application/Queries/{QueryName}/{QueryName}QueryHandler.cs`

```csharp
using MediatR;
using FamilyHub.Modules.{Module}.Domain.Repositories;

namespace FamilyHub.Modules.{Module}.Application.Queries.{QueryName};

public sealed class {QueryName}QueryHandler
    : IRequestHandler<{QueryName}Query, {QueryName}Result?>
{
    private readonly I{Entity}Repository _repository;

    public {QueryName}QueryHandler(I{Entity}Repository repository)
    {
        _repository = repository;
    }

    public async Task<{QueryName}Result?> Handle(
        {QueryName}Query query,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(query.Id, cancellationToken);

        if (entity is null)
            return null;

        return new {QueryName}Result(
            entity.Id,
            entity.Property1,
            entity.CreatedAt
        );
    }
}
```

### 4. Create GraphQL Query Method

**Location:** `Modules/FamilyHub.Modules.{Module}/Presentation/GraphQL/Queries/{Entity}Queries.cs`

```csharp
using HotChocolate;
using HotChocolate.Authorization;
using MediatR;

namespace FamilyHub.Modules.{Module}.Presentation.GraphQL.Queries;

[ExtendObjectType("Query")]
public class {Entity}Queries
{
    [Authorize]
    public async Task<{QueryName}Result?> {QueryName}(
        Guid id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new {QueryName}Query({EntityId}.From(id));
        return await mediator.Send(query, cancellationToken);
    }
}
```

## Collection Query Example

**Query:**

```csharp
public sealed record GetFamiliesQuery(
    int Skip = 0,
    int Take = 10
) : IRequest<IReadOnlyList<GetFamiliesResult>>;
```

**Handler:**

```csharp
public async Task<IReadOnlyList<GetFamiliesResult>> Handle(
    GetFamiliesQuery query,
    CancellationToken cancellationToken)
{
    var families = await _repository.GetAllAsync(
        query.Skip,
        query.Take,
        cancellationToken);

    return families.Select(f => new GetFamiliesResult(
        f.Id,
        f.Name,
        f.CreatedAt
    )).ToList();
}
```

**GraphQL:**

```csharp
[Authorize]
[UseFiltering]
[UseSorting]
public async Task<IReadOnlyList<GetFamiliesResult>> GetFamilies(
    int skip = 0,
    int take = 10,
    [Service] IMediator mediator,
    CancellationToken cancellationToken)
{
    var query = new GetFamiliesQuery(skip, take);
    return await mediator.Send(query, cancellationToken);
}
```

## Validation

- [ ] Query created in Application/Queries/{QueryName}/
- [ ] Query handler returns null for not found (not exception)
- [ ] GraphQL query method created in Presentation/GraphQL/Queries/
- [ ] Authorization attribute added
- [ ] Primitive→Vogen mapping in GraphQL layer
