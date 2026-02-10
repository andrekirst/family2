# Vogen Value Objects

**Purpose:** Guide for defining and using Vogen value objects in Family Hub.

**Rule:** Never use primitive types for domain concepts. Always wrap domain values in Vogen structs.

**Package:** `Vogen 8.0+`

---

## Basic Pattern

Every Vogen value object follows this structure:

```csharp
using Vogen;

[ValueObject<TUnderlying>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct MyValueObject
{
    private static Validation Validate(TUnderlying value)
    {
        // Return Validation.Ok or Validation.Invalid("message")
    }
}
```

The `conversions: Conversions.EfCoreValueConverter` parameter generates an EF Core value converter so the type works seamlessly with Entity Framework.

---

## ID Value Objects (Guid-based)

For entity identifiers, wrap `Guid` and add a `New()` factory:

```csharp
// src/FamilyHub.Api/Features/Family/Domain/ValueObjects/FamilyId.cs

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyId
{
    public static FamilyId New() => From(Guid.NewGuid());

    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
            return Validation.Invalid("Family ID cannot be empty");

        return Validation.Ok;
    }
}
```

### Usage

```csharp
// Generate a new unique ID
var familyId = FamilyId.New();

// Create from an existing Guid (with validation)
var familyId = FamilyId.From(someGuid);

// Access the underlying value
Guid raw = familyId.Value;
```

### Other ID types in the codebase

| Type | Underlying | Location |
|------|-----------|----------|
| `FamilyId` | `Guid` | `Features/Family/Domain/ValueObjects/` |
| `FamilyMemberId` | `Guid` | `Features/Family/Domain/ValueObjects/` |
| `InvitationId` | `Guid` | `Features/Family/Domain/ValueObjects/` |
| `UserId` | `Guid` | `Features/Auth/Domain/ValueObjects/` |

---

## String Value Objects

For domain strings with validation and optional normalization:

### Email

```csharp
// src/FamilyHub.Api/Common/Domain/ValueObjects/Email.cs

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Email is required");

        if (!value.Contains('@'))
            return Validation.Invalid("Invalid email format - missing @");

        if (value.Length > 320)
            return Validation.Invalid("Email too long (max 320 characters)");

        var parts = value.Split('@');
        if (parts.Length != 2)
            return Validation.Invalid("Invalid email format");

        if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            return Validation.Invalid("Invalid email format - empty local or domain part");

        return Validation.Ok;
    }
}
```

### InvitationToken (Hash)

```csharp
// src/FamilyHub.Api/Features/Family/Domain/ValueObjects/InvitationToken.cs

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct InvitationToken
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Invitation token is required");

        if (value.Length != 64)
            return Validation.Invalid(
                "Invitation token hash must be exactly 64 characters (SHA256 hex)");

        return Validation.Ok;
    }
}
```

---

## Enumeration-Style Value Objects

For constrained string values with named constants and behavior methods:

### FamilyRole

```csharp
// src/FamilyHub.Api/Features/Family/Domain/ValueObjects/FamilyRole.cs

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyRole
{
    // Named constants
    public static FamilyRole Owner => From("Owner");
    public static FamilyRole Admin => From("Admin");
    public static FamilyRole Member => From("Member");

    private static readonly string[] ValidRoles = ["Owner", "Admin", "Member"];

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Family role is required");

        if (!ValidRoles.Contains(value))
            return Validation.Invalid(
                $"Invalid family role: '{value}'. Valid: {string.Join(", ", ValidRoles)}");

        return Validation.Ok;
    }

    // Permission methods (behavior on the value object)
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
}
```

This pattern combines validation (only valid roles allowed) with domain behavior (permission methods), keeping the logic co-located with the type.

### InvitationStatus

```csharp
// Similar enumeration-style pattern
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct InvitationStatus
{
    public static InvitationStatus Pending => From("Pending");
    public static InvitationStatus Accepted => From("Accepted");
    public static InvitationStatus Declined => From("Declined");
    public static InvitationStatus Revoked => From("Revoked");

    public bool IsPending() => Value == "Pending";
}
```

---

## NormalizeInput

For value objects that need input normalization (trimming, case conversion), add a `NormalizeInput` method:

```csharp
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static string NormalizeInput(string input)
        => input?.Trim().ToLowerInvariant() ?? string.Empty;

    private static Validation Validate(string value)
    {
        // Validation runs AFTER normalization
    }
}
```

When `NormalizeInput` is defined, Vogen calls it before `Validate`. This ensures the stored value is always in canonical form.

---

## Creation Methods

### `.From()` -- Validated Creation

Throws `ValueObjectValidationException` if validation fails:

```csharp
var email = Email.From("user@example.com");     // Valid -- succeeds
var email = Email.From("invalid");              // Throws ValueObjectValidationException
```

### `.TryFrom()` -- Safe Creation

Returns a result without throwing:

```csharp
if (Email.TryFrom("user@example.com", out var email))
{
    // Use email
}
```

### `.New()` -- GUID Generation (ID types only)

Generates a new unique identifier:

```csharp
var familyId = FamilyId.New();   // Wraps Guid.NewGuid()
var userId = UserId.New();
```

### `.Value` -- Access Underlying Value

```csharp
Email email = Email.From("user@example.com");
string raw = email.Value;    // "user@example.com"

FamilyId id = FamilyId.New();
Guid raw = id.Value;          // The underlying Guid
```

---

## Where Value Objects Are Used

| Context | Primitives or Vogen? |
|---------|---------------------|
| Command records | **Vogen** -- always |
| Query records | **Vogen** -- always |
| Domain entities | **Vogen** -- always |
| GraphQL input records (Request DTOs) | **Primitives** -- for JSON serialization |
| GraphQL output (DTOs) | **Primitives** -- for JSON serialization |
| EF Core configurations | **Vogen** with converters |
| Tests | **Vogen** via `.From()` and `.New()` |

The boundary between primitives and Vogen types is the GraphQL layer (MutationType/QueryType), where `.From()` is called to convert.

---

## EF Core Configuration

See [ef-core-patterns.md](ef-core-patterns.md) for full details on configuring Vogen types with EF Core value converters.

---

## Related Guides

- [EF Core Patterns](ef-core-patterns.md) -- value converter configuration
- [Handler Patterns](handler-patterns.md) -- Vogen types in commands/queries
- [GraphQL Patterns](graphql-patterns.md) -- primitive-to-Vogen boundary
- [Authorization Patterns](authorization-patterns.md) -- permission methods on FamilyRole
- [Testing Patterns](testing-patterns.md) -- creating test data with `.From()` and `.New()`

---

**Last Updated:** 2026-02-09
