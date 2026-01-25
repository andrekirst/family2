---
name: vogen-value-object
description: Create Vogen value object with validation and EF Core support
category: backend
module-aware: true
inputs:
  - objectName: PascalCase name (e.g., Email, FamilyName)
  - baseType: string | Guid | int | decimal
  - module: DDD module name
  - validationRules: Validation requirements
---

# Vogen Value Object Skill

Create a Vogen value object with validation and EF Core integration.

## Context

Load module profile: `agent-os/profiles/modules/{module}.yaml`

## Steps

### 1. Create Value Object File

**Location:** `Modules/FamilyHub.Modules.{Module}/Domain/ValueObjects/{ObjectName}.cs`

```csharp
using Vogen;

namespace FamilyHub.Modules.{Module}.Domain.ValueObjects;

[ValueObject<{baseType}>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct {ObjectName}
{
    private static Validation Validate({baseType} value)
    {
        // Add validation rules here
        if (value == default)
            return Validation.Invalid("{ObjectName} cannot be empty.");

        return Validation.Ok;
    }

    // Optional: Normalize input (for strings)
    private static string NormalizeInput(string input)
        => input?.Trim() ?? string.Empty;
}
```

### 2. Common Validation Patterns

**String value object:**

```csharp
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Email cannot be empty.");

        if (value.Length > 320)
            return Validation.Invalid("Email cannot exceed 320 characters.");

        if (!value.Contains('@'))
            return Validation.Invalid("Invalid email format.");

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
        => input?.Trim().ToLowerInvariant() ?? string.Empty;
}
```

**GUID value object:**

```csharp
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyId
{
    public static FamilyId New() => From(Guid.NewGuid());
}
```

**Decimal value object:**

```csharp
[ValueObject<decimal>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Money
{
    private static Validation Validate(decimal value)
    {
        if (value < 0)
            return Validation.Invalid("Money cannot be negative.");

        return Validation.Ok;
    }
}
```

### 3. Usage in Domain

```csharp
// Create with validation (throws if invalid)
var email = Email.From("user@example.com");

// Safe creation
if (Email.TryFrom("user@example.com", out var result))
{
    // Use result.Value
}

// New GUID
var id = FamilyId.New();
```

### 4. EF Core Configuration

```csharp
// In entity configuration
builder.Property(e => e.Email)
    .HasConversion(new Email.EfCoreValueConverter())
    .HasMaxLength(320)
    .IsRequired();
```

## Validation

- [ ] Value object created in Domain/ValueObjects/
- [ ] Validation method returns proper error messages
- [ ] EfCoreValueConverter conversion enabled
- [ ] Used in entity with HasConversion
