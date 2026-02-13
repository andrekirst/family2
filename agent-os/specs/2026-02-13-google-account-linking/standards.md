# Standards for Google Account Linking

The following standards apply to this work.

---

## backend/user-context

Accessing the current authenticated user from JWT claims.

### ClaimNames Constants

```csharp
// Common/Infrastructure/ClaimNames.cs
public static class ClaimNames
{
    public const string Sub = "sub";
}
```

### IUserService

```csharp
public interface IUserService
{
    Task<User> GetCurrentUser(
        ClaimsPrincipal claimsPrincipal,
        IUserRepository userRepository,
        CancellationToken cancellationToken);
}
```

### Usage in GraphQL MutationType

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
}
```

### Rules

- `ClaimNames.Sub` is the single constant for JWT subject claim
- `IUserService` registered as Scoped
- Always use `[Authorize]` on mutations/queries requiring authentication
- `ClaimsPrincipal` is automatically provided by Hot Chocolate in resolvers

---

## backend/permission-system

Role-based permissions using Value Object methods with defense-in-depth enforcement.

### Permission String Format

```
{module}:{action}
```

### Rules

- Permission strings: `{module}:{action}` in kebab-case
- VO methods on the role: `Can{Action}() => Value is "Owner" or "Admin"`
- Backend: Authorization service enforces in handlers
- Frontend: Permission service with computed signals hides UI
- Always HIDE unauthorized actions (never disable+tooltip)

**Note:** For Google Account Linking, no new permission strings are needed — users can only manage their own linked account (enforced by user scoping, not role-based permissions).

---

## backend/secure-token-pattern

Generate secure tokens where plaintext is only in transit, hash is persisted.

### Token Flow

1. Handler generates plaintext + hash
2. Aggregate stores hash only
3. Domain event carries plaintext
4. Verification hashes incoming token, looks up by hash

### Rules

- Always use `RandomNumberGenerator` (never `Random` or `Guid`)
- Always hash with SHA256 before storing
- Plaintext only exists in the domain event (transient)

**Note:** Google OAuth tokens use a different pattern (AES-256-GCM encryption, not SHA256 hashing) because we need to decrypt them for API calls. The secure-token pattern informs the crypto approach but isn't directly applied.

---

## backend/graphql-input-command

Separate Input DTOs (primitives) from Commands (Vogen types). See ADR-003.

### File Organization

```
Commands/{Name}/
  Command.cs
  Handler.cs
  Validator.cs
  Result.cs
  MutationType.cs
```

### Rules

- Input DTOs in `Models/` with primitives
- Commands in `Commands/{Name}/` with Vogen types
- One MutationType per command (not centralized)
- Dispatch via `ICommandBus.SendAsync()`

---

## backend/vogen-value-objects

Always use Vogen 8.0+ for domain value objects.

### Definition Pattern

```csharp
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value) { ... }
    private static string NormalizeInput(string input) => input?.Trim().ToLowerInvariant() ?? string.Empty;
}
```

### Rules

- Always include `conversions: Conversions.EfCoreValueConverter`
- Implement `Validate()` for business rules
- Implement `NormalizeInput()` for string normalization
- Location: `Domain/ValueObjects/{Name}.cs`

---

## backend/domain-events

Events extend `DomainEvent` base record. Handlers are static classes with `Handle()` method.

### Event Definition

```csharp
public sealed record GoogleAccountLinkedEvent(
    GoogleAccountLinkId LinkId,
    UserId UserId,
    GoogleAccountId GoogleAccountId,
    GoogleScopes GrantedScopes
) : DomainEvent;
```

### Rules

- Events are sealed records extending `DomainEvent`
- Location: `Features/{Module}/Domain/Events/{Name}Event.cs`
- Handlers: `Features/{Module}/Application/EventHandlers/{Name}Handler.cs`
- Use past tense: Linked, Unlinked, Refreshed, Failed

---

## database/ef-core-migrations

EF Core migrations with Data/ folder for configurations.

### Schema Separation

```csharp
builder.ToTable("google_account_links", "google_integration");
```

### Rules

- Schema name = module name (lowercase, hyphenated for multi-word)
- EF Core config files in `Data/` folder
- Enable RLS on user-scoped tables

---

## frontend/angular-components

All components are standalone. Use atomic design hierarchy.

### Rules

- Always use `standalone: true`
- Import dependencies in `imports` array
- Use Angular Signals for state (`signal()`, `computed()`)
- Follow atomic design for organization
- Use `inject()` for DI

---

## frontend/apollo-graphql

Use Apollo Client for GraphQL with typed operations.

### Rules

- Use `inject(Apollo)` for dependency injection
- Handle errors with `catchError`
- Use typed operations (gql tagged templates)
- Operations file per feature: `graphql/{feature}.operations.ts`
