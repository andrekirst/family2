namespace FamilyHub.Modules.UserProfile.Domain.ValueObjects;

/// <summary>
/// Represents a user's preferred pronouns.
/// Strongly-typed value object enforcing domain validation rules.
/// Optional field, maximum 50 characters, automatically trimmed.
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct Pronouns
{
    /// <summary>
    /// Maximum allowed length for pronouns.
    /// </summary>
    private const int MaxLength = 50;

    private static Validation Validate(string value)
    {
        // After normalization, empty string is valid (optional field)
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Ok;
        }

        if (value.Length > MaxLength)
        {
            return Validation.Invalid($"Pronouns cannot exceed {MaxLength} characters.");
        }

        return Validation.Ok;
    }

    /// <summary>
    /// Normalizes input by trimming whitespace.
    /// </summary>
    private static string NormalizeInput(string input) =>
        input?.Trim() ?? string.Empty;
}
