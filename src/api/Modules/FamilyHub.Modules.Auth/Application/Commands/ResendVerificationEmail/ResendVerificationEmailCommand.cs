using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;

namespace FamilyHub.Modules.Auth.Application.Commands.ResendVerificationEmail;

/// <summary>
/// Command to resend the email verification link.
/// Requires authentication (user must be logged in).
/// </summary>
public sealed record ResendVerificationEmailCommand
    : ICommand<FamilyHub.SharedKernel.Domain.Result<ResendVerificationEmailResult>>,
      IRequireAuthentication;

/// <summary>
/// Result of resend verification email operation.
/// </summary>
public sealed record ResendVerificationEmailResult
{
    /// <summary>
    /// Indicates the email was sent successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Message to display to the user.
    /// </summary>
    public string Message { get; init; } = "Verification email sent.";
}
