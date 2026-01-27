using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;

namespace FamilyHub.Modules.Auth.Application.Commands.ChangePassword;

/// <summary>
/// Command to change the authenticated user's password.
/// Requires current password verification.
/// </summary>
public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
) : ICommand<FamilyHub.SharedKernel.Domain.Result<ChangePasswordResult>>,
    IRequireAuthentication;

/// <summary>
/// Result of password change operation.
/// </summary>
public sealed record ChangePasswordResult
{
    /// <summary>
    /// Indicates the password was changed successfully.
    /// </summary>
    public bool Success { get; init; }
}
