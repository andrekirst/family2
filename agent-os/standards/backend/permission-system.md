# Permission System

Role-based permissions using Value Object methods with defense-in-depth enforcement (backend + frontend).

## Permission String Format

```
{module}:{action}
```

Examples: `family:invite`, `family:revoke-invitation`, `family:edit`, `family:delete`

## FamilyRole Value Object

Permission methods live on the Vogen VO:

```csharp
// Domain/ValueObjects/FamilyRole.cs
public bool CanInvite() => Value is "Owner" or "Admin";
public bool CanRevokeInvitation() => Value is "Owner" or "Admin";
public bool CanRemoveMembers() => Value is "Owner" or "Admin";
public bool CanEditFamily() => Value is "Owner" or "Admin";
public bool CanDeleteFamily() => Value is "Owner";
public bool CanManageRoles() => Value is "Owner";

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
```

## Role Hierarchy

| Permission | Owner | Admin | Member |
|-----------|-------|-------|--------|
| family:invite | yes | yes | no |
| family:revoke-invitation | yes | yes | no |
| family:remove-members | yes | yes | no |
| family:edit | yes | yes | no |
| family:delete | yes | no | no |
| family:manage-roles | yes | no | no |

## Backend Enforcement (FamilyAuthorizationService)

```csharp
public class FamilyAuthorizationService
{
    private readonly IFamilyMemberRepository _memberRepository;

    public async Task<bool> CanInviteAsync(UserId userId, FamilyId familyId, CancellationToken ct)
    {
        var member = await _memberRepository.GetByUserAndFamilyAsync(userId, familyId, ct);
        return member?.Role.CanInvite() ?? false;
    }
}
```

Used in handlers to throw `DomainException` when unauthorized.

## Permission Flow (Backend to Frontend)

```
FamilyRole.GetPermissions() -> UserDto.Permissions -> GraphQL [String!]! -> FamilyPermissionService
```

`GetCurrentUserQueryHandler` populates `UserDto.Permissions` from the FamilyMember's role.

## Frontend Enforcement (FamilyPermissionService)

```typescript
@Injectable({ providedIn: 'root' })
export class FamilyPermissionService {
  private userService = inject(UserService);
  private permissions = computed(() => this.userService.currentUser()?.permissions ?? []);

  canInvite = computed(() => this.permissions().includes('family:invite'));
  canRevokeInvitation = computed(() => this.permissions().includes('family:revoke-invitation'));
  canRemoveMembers = computed(() => this.permissions().includes('family:remove-members'));
  canEditFamily = computed(() => this.permissions().includes('family:edit'));
  canDeleteFamily = computed(() => this.permissions().includes('family:delete'));
  canManageRoles = computed(() => this.permissions().includes('family:manage-roles'));
}
```

## UI Pattern: HIDE (not disable)

Always **hide** unauthorized UI elements. Never disable with tooltip.

```html
@if (permissions.canInvite()) {
  <button (click)="openInviteDialog()">Invite Member</button>
}
```

## Known Gaps

- `RegisterUser` mutation does not return permissions (only `GetCurrentUser` does)
- No permission caching -- refetch `currentUser` after role-changing actions
- Cross-module permissions not yet designed

## Rules

- Permission strings: `{module}:{action}` in kebab-case
- VO methods on the role: `Can{Action}() => Value is "Owner" or "Admin"`
- Backend: `FamilyAuthorizationService` enforces in handlers
- Frontend: `FamilyPermissionService` with computed signals hides UI
- Always HIDE unauthorized actions (never disable+tooltip)
- Location: VO in `Domain/ValueObjects/`, service in `Application/Services/`, frontend in `core/permissions/`
