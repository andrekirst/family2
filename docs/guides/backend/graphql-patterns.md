# GraphQL Patterns (Hot Chocolate)

**Purpose:** Guide for implementing GraphQL mutations and queries with Hot Chocolate in Family Hub.

**Core pattern:** ADR-003 Input-to-Command pattern. GraphQL types receive primitive inputs, convert to Vogen-typed commands, and dispatch through the command/query bus.

**Reference:** [ADR-003: GraphQL Input-Command Pattern](../../architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md)

---

## Architecture Overview

```
GraphQL Request (primitives: strings, ints)
       |
       v
MutationType / QueryType (Hot Chocolate)
  1. [Authorize] checks JWT authentication
  2. Resolves current user from ClaimsPrincipal
  3. Converts primitives to Vogen value objects
  4. Creates Command/Query record
  5. Dispatches via ICommandBus / IQueryBus
       |
       v
Command/Query Handler (Wolverine, static class)
  Executes business logic, returns result
       |
       v
DTO / Payload returned to GraphQL client
```

---

## MutationType (Per-Command)

Each command has its own `MutationType.cs` file inside the command subfolder. It extends the root mutation type using Hot Chocolate's `[ExtendObjectType]`:

```csharp
// src/FamilyHub.Api/Features/Family/Application/Commands/SendInvitation/MutationType.cs

[ExtendObjectType(typeof(AuthMutations))]
public class MutationType
{
    [Authorize]
    public async Task<InvitationDto> SendInvitation(
        SendInvitationRequest input,             // Primitives from client
        ClaimsPrincipal claimsPrincipal,          // JWT identity
        [Service] ICommandBus commandBus,         // Wolverine command bus
        [Service] IUserRepository userRepository,
        [Service] IFamilyInvitationRepository invitationRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        // 1. Resolve current user
        var user = await userService.GetCurrentUser(
            claimsPrincipal, userRepository, cancellationToken);

        if (user.FamilyId is null)
        {
            throw new InvalidOperationException(
                "You must be part of a family to send invitations");
        }

        // 2. Map primitives to Vogen value objects and create command
        var command = new SendInvitationCommand(
            user.FamilyId.Value,
            user.Id,
            Email.From(input.Email.Trim()),       // Primitive -> Vogen
            FamilyRole.From(input.Role));          // Primitive -> Vogen

        // 3. Dispatch via command bus
        var result = await commandBus.SendAsync(command, cancellationToken);

        // 4. Return DTO
        var invitation = await invitationRepository.GetByIdAsync(
            result.InvitationId, cancellationToken);
        return InvitationMapper.ToDto(invitation!);
    }
}
```

### Key Conventions

- Class name is always `MutationType`.
- Decorated with `[ExtendObjectType(typeof(AuthMutations))]` to extend the root mutation type.
- Uses `[Authorize]` for protected operations.
- Uses `[Service]` attribute on DI parameters (this is a Hot Chocolate concept, not Wolverine).
- Returns a DTO, not the domain entity.

---

## QueryType (Per-Query)

Each query has its own `QueryType.cs` file inside the query subfolder:

```csharp
// src/FamilyHub.Api/Features/Family/Application/Queries/GetMyFamily/QueryType.cs

[ExtendObjectType(typeof(AuthQueries))]
public class QueryType
{
    [Authorize]
    public async Task<FamilyDto?> GetMyFamily(
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(externalUserIdString))
        {
            return null;
        }

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var query = new GetMyFamilyQuery(externalUserId);

        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
```

### Key Conventions

- Class name is always `QueryType`.
- Decorated with `[ExtendObjectType(typeof(AuthQueries))]` to extend the root query type.
- Extracts identity from `ClaimsPrincipal` using `ClaimNames.Sub`.
- Dispatches via `IQueryBus.QueryAsync()`.

---

## Root Mutation and Query Types

The root types serve as extension points. They are minimal:

```csharp
// src/FamilyHub.Api/Features/Auth/GraphQL/AuthMutations.cs
// Root mutation type that all modules extend

// src/FamilyHub.Api/Features/Auth/GraphQL/AuthQueries.cs
// Root query type that all modules extend
```

Module-specific GraphQL classes also extend these root types for inline queries/mutations that are not part of the subfolder-per-command pattern:

```csharp
// src/FamilyHub.Api/Features/Family/GraphQL/FamilyQueries.cs

[ExtendObjectType(typeof(AuthQueries))]
public class FamilyQueries
{
    [Authorize]
    public async Task<List<InvitationDto>> GetPendingInvitations(
        ClaimsPrincipal claimsPrincipal,
        [Service] IUserRepository userRepository,
        [Service] IFamilyInvitationRepository invitationRepository,
        [Service] IUserService userService,
        CancellationToken ct)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, ct);
        // ... direct repository query, no query bus
    }
}
```

Some simpler queries are defined directly in `FamilyQueries.cs` rather than going through the full query bus pattern. This is acceptable for straightforward read operations that do not need the handler abstraction.

---

## Input Records (Request DTOs)

Input records use **primitives only** (strings, ints, bools). They are what the GraphQL client sends:

```csharp
// src/FamilyHub.Api/Features/Family/Models/SendInvitationRequest.cs

public sealed record SendInvitationRequest
{
    public required string Email { get; init; }
    public required string Role { get; init; }
}
```

```csharp
// src/FamilyHub.Api/Features/Family/Models/CreateFamilyRequest.cs

public sealed record CreateFamilyRequest
{
    public required string Name { get; init; }
}
```

### Why Separate Input and Command

| Concern | Input Record (Request) | Command Record |
|---------|----------------------|----------------|
| Types | Primitives (string, int) | Vogen value objects |
| Validation | JSON deserialization only | Domain-level validation |
| Location | `Models/` folder | `Commands/{Name}/` folder |
| Used by | GraphQL layer | Application/Domain layer |

This separation means:

- GraphQL serialization works with simple types.
- Vogen validation fires at the boundary (in `MutationType`) via `.From()`.
- Commands carry fully validated domain types into the handler.

---

## DTO Mappers

Static mapper classes convert domain entities to DTOs for the GraphQL response:

```csharp
// src/FamilyHub.Api/Features/Family/Application/Mappers/InvitationMapper.cs

public static class InvitationMapper
{
    public static InvitationDto ToDto(FamilyInvitation invitation) => new(...)
    {
        // Map domain entity fields to DTO fields
    };
}
```

Mapper conventions:

- Static class with static `ToDto()` methods.
- Located in `Application/Mappers/`.
- Never expose domain entities directly through GraphQL.

---

## Public vs. Protected Queries

Most queries require authentication with `[Authorize]`. Some queries are intentionally public:

```csharp
// No [Authorize] attribute -- intentionally public
public async Task<InvitationDto?> GetInvitationByToken(
    string token,
    [Service] IFamilyInvitationRepository invitationRepository,
    CancellationToken cancellationToken)
{
    var tokenHash = SendInvitationCommandHandler.ComputeSha256Hash(token);
    var invitation = await invitationRepository.GetByTokenHashAsync(
        InvitationToken.From(tokenHash), cancellationToken);
    return invitation is null ? null : InvitationMapper.ToDto(invitation);
}
```

This query is public because it powers the invitation acceptance page, which unauthenticated users may visit.

---

## Checklist: Adding a New GraphQL Mutation

1. Create the request DTO in `Models/` with primitive types.
2. Create the command subfolder: `Commands/{Name}/`.
3. Create `MutationType.cs` in the subfolder:
   - Extend `AuthMutations` with `[ExtendObjectType]`.
   - Add `[Authorize]` if the operation requires authentication.
   - Resolve the current user from `ClaimsPrincipal`.
   - Map input primitives to Vogen value objects with `.From()`.
   - Dispatch via `commandBus.SendAsync()`.
   - Return a DTO.

## Checklist: Adding a New GraphQL Query

1. Create the query subfolder: `Queries/{Name}/`.
2. Create `QueryType.cs` in the subfolder:
   - Extend `AuthQueries` with `[ExtendObjectType]`.
   - Add `[Authorize]` if the operation requires authentication.
   - Extract identity from `ClaimsPrincipal`.
   - Dispatch via `queryBus.QueryAsync()`.
   - Return a DTO.

---

## Related Guides

- [Handler Patterns](handler-patterns.md) -- the handlers dispatched by MutationType/QueryType
- [Vogen Value Objects](vogen-value-objects.md) -- the `.From()` conversions in the mapping layer
- [Authorization Patterns](authorization-patterns.md) -- `[Authorize]` and permission enforcement
- [ADR-003](../../architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md) -- architectural decision record

---

**Last Updated:** 2026-02-09
