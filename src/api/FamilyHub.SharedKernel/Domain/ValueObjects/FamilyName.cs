using Vogen;

namespace FamilyHub.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a family group name (e.g., "Smith Family").
/// Strongly-typed value object enforcing domain validation rules.
/// Maximum length: 100 characters.
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyName
{
    /// <summary>
    /// Maximum allowed length for a family name.
    /// </summary>
    private const int MaxLength = 100;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Family name cannot be empty.");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Family name cannot exceed {MaxLength} characters.");

        return Validation.Ok;
    }

    /// <summary>
    /// Normalizes input by trimming whitespace.
    /// </summary>
    private static string NormalizeInput(string input) =>
        input.Trim();
}
