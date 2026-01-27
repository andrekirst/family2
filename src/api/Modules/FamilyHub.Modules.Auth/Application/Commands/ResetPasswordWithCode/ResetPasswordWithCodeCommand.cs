using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.ResetPasswordWithCode;

/// <summary>
/// Command to reset password using a 6-digit code (mobile flow).
/// UNAUTHENTICATED: Uses email + code instead of JWT.
/// </summary>
public sealed record ResetPasswordWithCodeCommand(
    Email Email,
    string Code,
    string NewPassword,
    string ConfirmNewPassword
) : ICommand<FamilyHub.SharedKernel.Domain.Result<ResetPasswordWithCodeResult>>;

/// <summary>
/// Result of password reset with code operation.
/// </summary>
public sealed record ResetPasswordWithCodeResult
{
    /// <summary>
    /// Indicates the password was reset successfully.
    /// </summary>
    public bool Success { get; init; }
}
