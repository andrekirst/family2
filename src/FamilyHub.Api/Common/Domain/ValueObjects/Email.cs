using Vogen;

namespace FamilyHub.Api.Common.Domain.ValueObjects;

/// <summary>
/// Email address value object with validation.
/// Ensures email addresses are never invalid at domain level.
/// </summary>
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

        // Basic format validation
        var parts = value.Split('@');
        if (parts.Length != 2)
            return Validation.Invalid("Invalid email format");

        if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            return Validation.Invalid("Invalid email format - empty local or domain part");

        return Validation.Ok;
    }
}
