# Vogen Value Objects

Always use Vogen 8.0+ for domain value objects. Never use primitives in commands/domain.

## Definition Pattern

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

## Creation

```csharp
UserId userId = UserId.New();           // New GUID
Email email = Email.From("user@ex.com"); // With validation (throws if invalid)
Email.TryFrom("invalid", out var result); // Safe creation
```

## EF Core Configuration

```csharp
builder.Property(u => u.Email)
    .HasConversion(new Email.EfCoreValueConverter())
    .HasMaxLength(320)
    .IsRequired();
```

## Rules

- Always include `conversions: Conversions.EfCoreValueConverter`
- Implement `Validate()` for business rules
- Implement `NormalizeInput()` for string normalization
- Location: `Domain/ValueObjects/{Name}.cs`
