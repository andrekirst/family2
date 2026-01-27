using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.RequestPasswordReset;

/// <summary>
/// Command to request a password reset.
/// Sends reset link or code to the user's email.
/// UNAUTHENTICATED: Public endpoint (user forgot password).
/// </summary>
public sealed record RequestPasswordResetCommand(
    Email Email,
    bool UseMobileCode = false
) : ICommand<FamilyHub.SharedKernel.Domain.Result<RequestPasswordResetResult>>;

/// <summary>
/// Result of password reset request.
/// </summary>
public sealed record RequestPasswordResetResult
{
    /// <summary>
    /// Always returns success even if email doesn't exist (security best practice).
    /// </summary>
    public bool Success { get; init; } = true;

    /// <summary>
    /// Generic message for user (doesn't reveal if email exists).
    /// </summary>
    public string Message { get; init; } = "If an account with that email exists, you will receive password reset instructions.";
}
