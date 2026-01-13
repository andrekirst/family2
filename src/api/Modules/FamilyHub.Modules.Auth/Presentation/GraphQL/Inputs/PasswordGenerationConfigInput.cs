namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for password generation configuration.
/// Allows users to customize password strength for managed accounts.
/// </summary>
public sealed record PasswordGenerationConfigInput
{
    /// <summary>
    /// Password length (12-32 characters).
    /// Configurable via slider in UI.
    /// </summary>
    public required int Length { get; init; }

    /// <summary>
    /// Include uppercase letters (A-Z).
    /// </summary>
    public required bool IncludeUppercase { get; init; }

    /// <summary>
    /// Include lowercase letters (a-z).
    /// </summary>
    public required bool IncludeLowercase { get; init; }

    /// <summary>
    /// Include digits (0-9).
    /// </summary>
    public required bool IncludeDigits { get; init; }

    /// <summary>
    /// Include symbols (!@#$%^&amp;*).
    /// </summary>
    public required bool IncludeSymbols { get; init; }
}
