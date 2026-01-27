using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.Register;

/// <summary>
/// Command to register a new user with email and password.
/// Creates user account, sends verification email, and auto-creates personal family.
/// UNAUTHENTICATED: No authorization requirements (public registration).
/// </summary>
public sealed record RegisterCommand(
    Email Email,
    string Password,
    string ConfirmPassword
) : ICommand<FamilyHub.SharedKernel.Domain.Result<RegisterResult>>;

/// <summary>
/// Result of successful user registration.
/// </summary>
public sealed record RegisterResult
{
    /// <summary>
    /// The ID of the newly created user.
    /// </summary>
    public required UserId UserId { get; init; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public required Email Email { get; init; }

    /// <summary>
    /// Indicates whether email verification is required.
    /// </summary>
    public bool EmailVerificationRequired { get; init; } = true;

    /// <summary>
    /// JWT access token (not issued until email is verified in strict mode).
    /// Null if email verification is required before login.
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// Refresh token for obtaining new access tokens.
    /// Null if email verification is required before login.
    /// </summary>
    public string? RefreshToken { get; init; }
}
