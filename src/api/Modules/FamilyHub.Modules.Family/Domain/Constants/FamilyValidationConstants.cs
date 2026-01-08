namespace FamilyHub.Modules.Family.Domain.Constants;

/// <summary>
/// Domain validation constants for the Family module.
/// Centralized to ensure consistency across domain entities, input DTOs, and tests.
/// </summary>
public static class FamilyValidationConstants
{
    /// <summary>
    /// Maximum length for family names (e.g., "Smith Family").
    /// Must match MaxLength in CreateFamilyInput GraphQL input type.
    /// </summary>
    public const int FamilyNameMaxLength = 100;

    /// <summary>
    /// Error message for family name max length validation.
    /// Used for validation error messages across the domain.
    /// </summary>
    public static readonly string FamilyNameMaxLengthMessage =
        $"Family name cannot exceed {FamilyNameMaxLength} characters.";
}
