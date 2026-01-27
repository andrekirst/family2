using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;

namespace FamilyHub.Modules.Auth.Application.Commands.VerifyEmail;

/// <summary>
/// Command to verify a user's email address using a token.
/// UNAUTHENTICATED: Uses verification token from email link.
/// </summary>
public sealed record VerifyEmailCommand(
    string Token
) : ICommand<FamilyHub.SharedKernel.Domain.Result<VerifyEmailResult>>;

/// <summary>
/// Result of email verification operation.
/// </summary>
public sealed record VerifyEmailResult
{
    /// <summary>
    /// Indicates the email was verified successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Message to display to the user.
    /// </summary>
    public string Message { get; init; } = "Email verified successfully.";
}
