# User Context

Accessing the current authenticated user from JWT claims.

## ClaimNames Constants

```csharp
// Common/Infrastructure/ClaimNames.cs
public static class ClaimNames
{
    public const string Sub = "sub";
}
```

## IUserService

```csharp
// Common/Services/IUserService.cs
public interface IUserService
{
    Task<User> GetCurrentUser(
        ClaimsPrincipal claimsPrincipal,
        IUserRepository userRepository,
        CancellationToken cancellationToken);
}

public class UserService : IUserService
{
    public async Task<User> GetCurrentUser(
        ClaimsPrincipal claimsPrincipal,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
                                   ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        return await userRepository.GetByExternalIdAsync(externalUserId, cancellationToken)
               ?? throw new UnauthorizedAccessException("User not found");
    }
}
```

## Registration

```csharp
// Common/Services/ServiceRegistrations.cs
services.AddScoped<IUserService, UserService>();
```

## Usage in GraphQL MutationType

```csharp
[Authorize]
public async Task<InvitationDto> SendInvitation(
    SendInvitationRequest input,
    ClaimsPrincipal claimsPrincipal,
    [Service] ICommandBus commandBus,
    [Service] IUserRepository userRepository,
    [Service] IUserService userService,
    CancellationToken cancellationToken)
{
    var user = await userService.GetCurrentUser(
        claimsPrincipal, userRepository, cancellationToken);
    // user.Id, user.FamilyId, etc. now available
}
```

## Rules

- `ClaimNames.Sub` is the single constant for JWT subject claim
- `IUserService` registered as Scoped
- Always use `[Authorize]` on mutations/queries requiring authentication
- `ClaimsPrincipal` is automatically provided by Hot Chocolate in resolvers
