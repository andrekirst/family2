# GraphQL Input->Command Pattern

Separate Input DTOs (primitives) from Wolverine Commands (Vogen). See ADR-003.

## Input (primitives)

```csharp
public sealed record SendInvitationRequest
{
    public required string Email { get; init; }
    public required string Role { get; init; }
}
```

## Command (Vogen types)

```csharp
public sealed record SendInvitationCommand(
    FamilyId FamilyId,
    UserId InvitedBy,
    Email InviteeEmail,
    FamilyRole Role
) : ICommand<SendInvitationResult>;
```

## MutationType (per-command)

Each command has its own `MutationType.cs` in its subfolder. Maps primitives to Vogen, dispatches via `ICommandBus`.

```csharp
[ExtendObjectType(typeof(AuthMutations))]
public class MutationType
{
    [Authorize]
    public async Task<InvitationDto> SendInvitation(
        SendInvitationRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserService userService,
        ...)
    {
        var user = await userService.GetCurrentUser(...);
        var command = new SendInvitationCommand(
            user.FamilyId!.Value, user.Id,
            Email.From(input.Email.Trim()),
            FamilyRole.From(input.Role));
        var result = await commandBus.SendAsync(command, ct);
        return mapper.ToDto(result);
    }
}
```

## File Organization

```
Commands/{Name}/
  {Name}Command.cs
  {Name}CommandHandler.cs  (static class)
  {Name}CommandValidator.cs
  {Name}Result.cs
  MutationType.cs
```

## Rules

- Input DTOs in `Models/` with primitives
- Commands in `Commands/{Name}/` with Vogen types
- One MutationType per command (not centralized)
- Handlers are static classes (Wolverine)
- Dispatch via `ICommandBus.SendAsync()` (not `IMediator.Send()`)
