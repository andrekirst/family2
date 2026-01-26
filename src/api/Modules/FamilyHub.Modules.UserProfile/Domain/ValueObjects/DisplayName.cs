namespace FamilyHub.Modules.UserProfile.Domain.ValueObjects;

/// <summary>
/// Represents a user's display name within the application.
/// Strongly-typed value object enforcing domain validation rules.
/// Required field, 1-100 characters, automatically trimmed.
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct DisplayName
{
    /// <summary>
    /// Minimum allowed length for a display name.
    /// </summary>
    private const int MinLength = 1;

    /// <summary>
    /// Maximum allowed length for a display name.
    /// </summary>
    private const int MaxLength = 100;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Display name cannot be empty.");
        }

        if (value.Length < MinLength)
        {
            return Validation.Invalid($"Display name must be at least {MinLength} character.");
        }

        if (value.Length > MaxLength)
        {
            return Validation.Invalid($"Display name cannot exceed {MaxLength} characters.");
        }

        return Validation.Ok;
    }

    /// <summary>
    /// Normalizes input by trimming whitespace.
    /// </summary>
    private static string NormalizeInput(string input) =>
        input.Trim();
}
