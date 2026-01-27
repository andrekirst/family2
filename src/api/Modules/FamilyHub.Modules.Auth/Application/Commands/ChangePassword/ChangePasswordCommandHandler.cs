using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.ChangePassword;

/// <summary>
/// Handler for ChangePasswordCommand.
/// </summary>
/// <param name="authService">Service handling password change logic.</param>
/// <param name="userContext">Current user context.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class ChangePasswordCommandHandler(
    IAuthService authService,
    IUserContext userContext,
    ILogger<ChangePasswordCommandHandler> logger)
    : ICommandHandler<ChangePasswordCommand, FamilyHub.SharedKernel.Domain.Result<ChangePasswordResult>>
{
    /// <inheritdoc />
    public async Task<FamilyHub.SharedKernel.Domain.Result<ChangePasswordResult>> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        LogChangePasswordAttempt(userId.Value);

        // Validate password confirmation
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            LogChangePasswordFailed(userId.Value, "New passwords do not match");
            return Result.Failure<ChangePasswordResult>("New passwords do not match.");
        }

        var serviceRequest = new ChangePasswordRequest
        {
            UserId = userId,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        var authResult = await authService.ChangePasswordAsync(serviceRequest, cancellationToken);

        if (authResult.IsFailure)
        {
            LogChangePasswordFailed(userId.Value, authResult.ErrorMessage ?? "Password change failed");
            return Result.Failure<ChangePasswordResult>(authResult.ErrorMessage ?? "Password change failed.");
        }

        LogChangePasswordSuccess(userId.Value);

        return Result.Success(new ChangePasswordResult { Success = true });
    }

    [LoggerMessage(LogLevel.Information, "Password change attempt for user {UserId}")]
    partial void LogChangePasswordAttempt(Guid userId);

    [LoggerMessage(LogLevel.Warning, "Password change failed for user {UserId}: {Reason}")]
    partial void LogChangePasswordFailed(Guid userId, string reason);

    [LoggerMessage(LogLevel.Information, "Password changed for user {UserId}")]
    partial void LogChangePasswordSuccess(Guid userId);
}
