using FamilyHub.Modules.Auth.Application.Abstractions;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL type extensions for AuthType.
/// Adds resolver fields for authentication operations.
/// </summary>
[ExtendObjectType(typeof(AuthType))]
public sealed class AuthTypeExtensions
{
    /// <summary>
    /// Validates password strength against configured policy.
    /// Returns real-time validation feedback for password input.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <param name="passwordService">The password validation service.</param>
    /// <returns>Password validation result with strength score and suggestions.</returns>
    [GraphQLDescription("Validates password strength against configured policy. Returns real-time validation feedback.")]
    public PasswordValidationPayload ValidatePassword(
        string password,
        [Service] IPasswordService passwordService)
    {
        var result = passwordService.ValidateStrength(password);

        return new PasswordValidationPayload
        {
            IsValid = result.IsValid,
            Score = result.Score,
            Strength = result.Strength,
            Errors = result.Errors.ToList(),
            Suggestions = result.Suggestions.ToList()
        };
    }
}

/// <summary>
/// GraphQL payload for password validation result.
/// </summary>
public sealed record PasswordValidationPayload
{
    /// <summary>
    /// Gets whether the password meets all requirements.
    /// </summary>
    [GraphQLDescription("Whether the password meets all requirements.")]
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the strength score from 0 (very weak) to 4 (very strong).
    /// </summary>
    [GraphQLDescription("Strength score from 0 (very weak) to 4 (very strong).")]
    public int Score { get; init; }

    /// <summary>
    /// Gets the human-readable strength description.
    /// </summary>
    [GraphQLDescription("Human-readable strength description (e.g., 'Weak', 'Strong').")]
    public string Strength { get; init; } = string.Empty;

    /// <summary>
    /// Gets the list of validation errors if any.
    /// </summary>
    [GraphQLDescription("List of validation errors, if any.")]
    public List<string> Errors { get; init; } = [];

    /// <summary>
    /// Gets suggestions for improving the password.
    /// </summary>
    [GraphQLDescription("Suggestions for improving the password strength.")]
    public List<string> Suggestions { get; init; } = [];
}
