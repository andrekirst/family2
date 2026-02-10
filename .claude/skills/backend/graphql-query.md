---
name: graphql-query
description: Create Hot Chocolate GraphQL query with Wolverine dispatch
category: backend
module-aware: true
inputs:
  - queryName: PascalCase query name (e.g., GetMyFamily)
  - module: DDD module name
---

# GraphQL Query Skill

Create a Hot Chocolate query that dispatches to a Wolverine query handler.

## Query Record

Location: `Features/{Module}/Application/Queries/{QueryName}/{QueryName}Query.cs`

```csharp
public sealed record {QueryName}Query(
    ExternalUserId ExternalUserId
) : IQuery<{Result}Dto?>;
```

## Query Handler (Static)

Location: `Features/{Module}/Application/Queries/{QueryName}/{QueryName}QueryHandler.cs`

```csharp
public static class {QueryName}QueryHandler
{
    public static async Task<{Result}Dto?> Handle(
        {QueryName}Query query,
        IUserRepository userRepository,
        I{Entity}Repository entityRepository,
        CancellationToken ct)
    {
        // Query logic
    }
}
```

## QueryType (per-query)

Location: `Features/{Module}/Application/Queries/{QueryName}/QueryType.cs`

```csharp
[ExtendObjectType(typeof(AuthQueries))]
public class QueryType
{
    [Authorize]
    public async Task<{Result}Dto?> {QueryName}(
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var externalUserId = ExternalUserId.From(
            claimsPrincipal.FindFirst(ClaimNames.Sub)!.Value);
        return await queryBus.QueryAsync(
            new {QueryName}Query(externalUserId), ct);
    }
}
```

## Validation

- [ ] Query implements IQuery<TResult>
- [ ] Handler is static class with static Handle()
- [ ] QueryType extends AuthQueries
- [ ] Dispatches via IQueryBus.QueryAsync()
