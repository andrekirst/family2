---
name: graphql-mutation
description: Create Hot Chocolate GraphQL mutation with Wolverine dispatch
category: backend
module-aware: true
inputs:
  - mutationName: PascalCase mutation name (e.g., SendInvitation)
  - module: DDD module name
---

# GraphQL Mutation Skill

Create a Hot Chocolate mutation that maps GraphQL input to a Wolverine command.

## Request DTO (primitives)

Location: `Features/{Module}/Models/{MutationName}Request.cs`

```csharp
public sealed record {MutationName}Request
{
    public required string Field1 { get; init; }
}
```

## MutationType (per-command)

Location: `Features/{Module}/Application/Commands/{MutationName}/MutationType.cs`

```csharp
[ExtendObjectType(typeof(AuthMutations))]
public class MutationType
{
    [Authorize]
    public async Task<{Result}Dto> {MutationName}(
        {MutationName}Request input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserService userService,
        [Service] IUserRepository userRepository,
        CancellationToken ct)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, ct);
        var command = new {MutationName}Command(
            VogenType.From(input.Field1));
        var result = await commandBus.SendAsync(command, ct);
        return mapper.ToDto(result);
    }
}
```

## Validation

- [ ] Input uses primitives only (no Vogen)
- [ ] MutationType extends AuthMutations
- [ ] Uses [Authorize] for protected operations
- [ ] Maps primitives to Vogen via .From()
- [ ] Dispatches via ICommandBus.SendAsync()
