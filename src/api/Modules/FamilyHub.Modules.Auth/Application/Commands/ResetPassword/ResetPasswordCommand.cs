using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;

namespace FamilyHub.Modules.Auth.Application.Commands.ResetPassword;

/// <summary>
/// Command to reset password using a token from email link.
/// UNAUTHENTICATED: Uses reset token instead of JWT.
/// </summary>
public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string ConfirmNewPassword
) : ICommand<FamilyHub.SharedKernel.Domain.Result<ResetPasswordResult>>;

/// <summary>
/// Result of password reset operation.
/// </summary>
public sealed record ResetPasswordResult
{
    /// <summary>
    /// Indicates the password was reset successfully.
    /// </summary>
    public bool Success { get; init; }
}
