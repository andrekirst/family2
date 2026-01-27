using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.ResetPassword;

/// <summary>
/// Handler for ResetPasswordCommand.
/// </summary>
/// <param name="authService">Service handling password reset logic.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class ResetPasswordCommandHandler(
    IAuthService authService,
    ILogger<ResetPasswordCommandHandler> logger)
    : ICommandHandler<ResetPasswordCommand, FamilyHub.SharedKernel.Domain.Result<ResetPasswordResult>>
{
    /// <inheritdoc />
    public async Task<FamilyHub.SharedKernel.Domain.Result<ResetPasswordResult>> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        LogPasswordReset();

        // Validate password confirmation
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            LogPasswordResetFailed("New passwords do not match");
            return Result.Failure<ResetPasswordResult>("New passwords do not match.");
        }

        var serviceRequest = new ResetPasswordWithTokenRequest
        {
            Token = request.Token,
            NewPassword = request.NewPassword
        };

        var authResult = await authService.ResetPasswordWithTokenAsync(serviceRequest, cancellationToken);

        if (authResult.IsFailure)
        {
            LogPasswordResetFailed(authResult.ErrorMessage ?? "Password reset failed");
            return Result.Failure<ResetPasswordResult>(authResult.ErrorMessage ?? "Password reset failed.");
        }

        LogPasswordResetSuccess();

        return Result.Success(new ResetPasswordResult { Success = true });
    }

    [LoggerMessage(LogLevel.Information, "Password reset attempt with token")]
    partial void LogPasswordReset();

    [LoggerMessage(LogLevel.Warning, "Password reset failed: {Reason}")]
    partial void LogPasswordResetFailed(string reason);

    [LoggerMessage(LogLevel.Information, "Password reset successful")]
    partial void LogPasswordResetSuccess();
}
