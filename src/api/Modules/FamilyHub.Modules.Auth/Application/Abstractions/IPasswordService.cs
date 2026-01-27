using FamilyHub.Modules.Auth.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Abstractions;

/// <summary>
/// Service for password hashing, verification, and strength validation.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashes a password using Argon2id.
    /// </summary>
    /// <param name="password">The plain text password to hash.</param>
    /// <returns>The hashed password as a PasswordHash value object.</returns>
    PasswordHash HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="hashedPassword">The stored password hash.</param>
    /// <param name="providedPassword">The password provided by the user.</param>
    /// <returns>True if the password matches, false otherwise.</returns>
    bool VerifyPassword(PasswordHash hashedPassword, string providedPassword);

    /// <summary>
    /// Validates password strength against configured policy.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>A result containing validation status and any errors.</returns>
    PasswordStrengthResult ValidateStrength(string password);
}

/// <summary>
/// Result of password strength validation.
/// </summary>
public sealed record PasswordStrengthResult
{
    /// <summary>
    /// Whether the password meets all requirements.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Strength score from 0 (very weak) to 4 (very strong).
    /// </summary>
    public int Score { get; init; }

    /// <summary>
    /// Human-readable strength description.
    /// </summary>
    public string Strength { get; init; } = string.Empty;

    /// <summary>
    /// List of validation errors if any.
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>
    /// Suggestions for improving the password.
    /// </summary>
    public IReadOnlyList<string> Suggestions { get; init; } = [];

    /// <summary>
    /// Creates a valid password strength result.
    /// </summary>
    /// <param name="score">Strength score from 0-4.</param>
    /// <param name="strength">Human-readable strength description.</param>
    /// <returns>A valid password strength result.</returns>
    public static PasswordStrengthResult Valid(int score, string strength) =>
        new()
        {
            IsValid = true,
            Score = score,
            Strength = strength,
            Errors = [],
            Suggestions = []
        };

    /// <summary>
    /// Creates an invalid password strength result.
    /// </summary>
    /// <param name="errors">List of validation errors.</param>
    /// <param name="suggestions">Optional suggestions for improvement.</param>
    /// <returns>An invalid password strength result.</returns>
    public static PasswordStrengthResult Invalid(IReadOnlyList<string> errors, IReadOnlyList<string>? suggestions = null) =>
        new()
        {
            IsValid = false,
            Score = 0,
            Strength = "Too Weak",
            Errors = errors,
            Suggestions = suggestions ?? []
        };
}
