# Authorization Patterns

**Purpose:** Guide for implementing permission checks and role-based authorization in Family Hub.

**Strategy:** Defense-in-depth. The backend always enforces permissions. The frontend hides UI elements based on permissions but never relies on the frontend alone for security.

---

## Architecture Overview

```
Frontend (Angular)
  Hides buttons/actions based on permissions
  NEVER the sole enforcement layer
       |
       | GraphQL
       v
GraphQL MutationType / QueryType
  [Authorize] attribute = authentication gate
  Extracts user identity from ClaimsPrincipal
       |
       | ICommandBus / IQueryBus
       v
Command Handler
  FamilyAuthorizationService = permission
  enforcement (CanInvite, CanDelete, etc.)
```

---

## FamilyRole Value Object

The `FamilyRole` value object defines the three roles and encapsulates all permission logic:

```csharp
// src/FamilyHub.Api/Features/Family/Domain/ValueObjects/FamilyRole.cs

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyRole
{
    public static FamilyRole Owner => From("Owner");
    public static FamilyRole Admin => From("Admin");
    public static FamilyRole Member => From("Member");

    private static readonly string[] ValidRoles = ["Owner", "Admin", "Member"];

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Family role is required");

        if (!ValidRoles.Contains(value))
            return Validation.Invalid($"Invalid family role: '{value}'");

        return Validation.Ok;
    }

    // Permission methods
    public bool CanInvite() => Value is "Owner" or "Admin";
    public bool CanRevokeInvitation() => Value is "Owner" or "Admin";
    public bool CanRemoveMembers() => Value is "Owner" or "Admin";
    public bool CanEditFamily() => Value is "Owner" or "Admin";
    public bool CanDeleteFamily() => Value is "Owner";
    public bool CanManageRoles() => Value is "Owner";

    // Permission string export for frontend
    public List<string> GetPermissions()
    {
        var permissions = new List<string>();
        if (CanInvite()) permissions.Add("family:invite");
        if (CanRevokeInvitation()) permissions.Add("family:revoke-invitation");
        if (CanRemoveMembers()) permissions.Add("family:remove-members");
        if (CanEditFamily()) permissions.Add("family:edit");
        if (CanDeleteFamily()) permissions.Add("family:delete");
        if (CanManageRoles()) permissions.Add("family:manage-roles");
        return permissions;
    }
}
```

### Permission Matrix

| Permission               | Owner | Admin | Member |
|--------------------------|:-----:|:-----:|:------:|
| `family:invite`          | Yes   | Yes   | No     |
| `family:revoke-invitation` | Yes | Yes   | No     |
| `family:remove-members`  | Yes   | Yes   | No     |
| `family:edit`            | Yes   | Yes   | No     |
| `family:delete`          | Yes   | No    | No     |
| `family:manage-roles`    | Yes   | No    | No     |

### Permission String Format

Permissions follow the `{module}:{action}` convention:

- `family:invite` -- can send invitations
- `family:revoke-invitation` -- can revoke pending invitations
- `family:remove-members` -- can remove family members
- `family:edit` -- can edit family settings
- `family:delete` -- can delete the family
- `family:manage-roles` -- can change member roles

---

## FamilyAuthorizationService

Backend enforcement happens through `FamilyAuthorizationService`, which looks up the user's family membership and checks role-based permissions:

```csharp
// src/FamilyHub.Api/Features/Family/Application/Services/FamilyAuthorizationService.cs

public class FamilyAuthorizationService(IFamilyMemberRepository familyMemberRepository)
{
    public async Task<bool> CanInviteAsync(UserId userId, FamilyId familyId, CancellationToken ct = default)
    {
        var member = await familyMemberRepository.GetByUserAndFamilyAsync(userId, familyId, ct);
        return member is not null && member.IsActive && member.Role.CanInvite();
    }
}
```

The service checks three things:

1. The user is a member of the family.
2. The member is active.
3. The member's role has the required permission.

Usage in a handler:

```csharp
public static class SendInvitationCommandHandler
{
    public static async Task<SendInvitationResult> Handle(
        SendInvitationCommand command,
        FamilyAuthorizationService authService,  // Injected by Wolverine
        IFamilyInvitationRepository invitationRepository,
        IFamilyMemberRepository memberRepository,
        CancellationToken ct)
    {
        if (!await authService.CanInviteAsync(command.InvitedBy, command.FamilyId, ct))
        {
            throw new DomainException("You do not have permission to send invitations for this family");
        }

        // ... proceed with business logic
    }
}
```

---

## IUserService and ClaimNames

The `IUserService` abstraction resolves the current authenticated user from an OAuth `ClaimsPrincipal`:

```csharp
// src/FamilyHub.Api/Common/Services/IUserService.cs

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

`ClaimNames` centralizes claim type constants:

```csharp
// src/FamilyHub.Api/Common/Infrastructure/ClaimNames.cs

public static class ClaimNames
{
    public const string Sub = "sub";
}
```

---

## Permission Population

Permissions are populated in `GetCurrentUserQueryHandler` and returned to the frontend as part of the `UserDto`:

```csharp
// src/FamilyHub.Api/Features/Auth/Application/Handlers/GetCurrentUserQueryHandler.cs

public static class GetCurrentUserQueryHandler
{
    public static async Task<UserDto?> Handle(
        GetCurrentUserQuery query,
        IUserRepository userRepository,
        IFamilyMemberRepository familyMemberRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetByExternalIdAsync(query.ExternalUserId, ct);
        if (user is null) return null;

        var dto = UserMapper.ToDto(user);

        // Populate permissions based on family membership role
        if (user.FamilyId is not null)
        {
            var member = await familyMemberRepository.GetByUserAndFamilyAsync(
                user.Id, user.FamilyId.Value, ct);
            if (member is not null)
            {
                dto.Permissions = member.Role.GetPermissions();
            }
        }

        return dto;
    }
}
```

The frontend uses these permission strings to hide/show UI elements, but the backend always re-validates on every operation.

---

## Authentication vs. Authorization Layers

| Layer | Mechanism | What It Does |
|-------|-----------|--------------|
| GraphQL endpoint | `[Authorize]` attribute | Ensures the user is authenticated (has a valid JWT) |
| MutationType/QueryType | `IUserService.GetCurrentUser()` | Resolves the domain `User` entity from OAuth claims |
| Command handler | `FamilyAuthorizationService` | Checks role-based permissions against the family membership |

---

## Known Limitations and Watch-Outs

### RegisterUser Does Not Return Permissions

When a user first registers, they have no family membership and therefore no permissions. The `RegisterUser` mutation returns a `UserDto` with an empty permissions list. Permissions only become available after the user creates or joins a family.

### Permission Caching

Currently, permissions are fetched from the database on every `GetCurrentUser` query. There is no caching layer. If performance becomes a concern, consider adding a short-lived cache keyed by `UserId + FamilyId`.

### Cross-Module Permission Composition

Right now only the Family module defines permissions. As more modules are added (Calendar, Tasks, etc.), each module will define its own permission methods and `GetPermissions()` output. The `GetCurrentUserQueryHandler` will need to aggregate permissions across modules.

### Role Immutability in Invitations

When an invitation is sent, the role is fixed at send time. If the inviter's role changes before the invitation is accepted, the originally assigned role in the invitation is still applied.

---

## Related Guides

- [Handler Patterns](handler-patterns.md) -- how authorization fits into handlers
- [GraphQL Patterns](graphql-patterns.md) -- `[Authorize]` attribute and `ClaimsPrincipal`
- [Vogen Value Objects](vogen-value-objects.md) -- `FamilyRole` definition
- [Testing Patterns](testing-patterns.md) -- testing authorization with fake repositories

---

**Last Updated:** 2026-02-09
