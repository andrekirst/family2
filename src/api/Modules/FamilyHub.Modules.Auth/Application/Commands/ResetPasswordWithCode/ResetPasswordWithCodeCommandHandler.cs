using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.ResetPasswordWithCode;

/// <summary>
/// Handler for ResetPasswordWithCodeCommand.
/// </summary>
/// <param name="authService">Service handling password reset logic.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class ResetPasswordWithCodeCommandHandler(
    IAuthService authService,
    ILogger<ResetPasswordWithCodeCommandHandler> logger)
    : ICommandHandler<ResetPasswordWithCodeCommand, FamilyHub.SharedKernel.Domain.Result<ResetPasswordWithCodeResult>>
{
    /// <inheritdoc />
    public async Task<FamilyHub.SharedKernel.Domain.Result<ResetPasswordWithCodeResult>> Handle(
        ResetPasswordWithCodeCommand request,
        CancellationToken cancellationToken)
    {
        LogPasswordResetWithCode(request.Email.Value);

        // Validate password confirmation
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            LogPasswordResetWithCodeFailed(request.Email.Value, "New passwords do not match");
            return Result.Failure<ResetPasswordWithCodeResult>("New passwords do not match.");
        }

        var serviceRequest = new ResetPasswordWithCodeRequest
        {
            Email = request.Email.Value,
            Code = request.Code,
            NewPassword = request.NewPassword
        };

        var authResult = await authService.ResetPasswordWithCodeAsync(serviceRequest, cancellationToken);

        if (authResult.IsFailure)
        {
            LogPasswordResetWithCodeFailed(request.Email.Value, authResult.ErrorMessage ?? "Password reset failed");
            return Result.Failure<ResetPasswordWithCodeResult>(authResult.ErrorMessage ?? "Password reset failed.");
        }

        LogPasswordResetWithCodeSuccess(request.Email.Value);

        return Result.Success(new ResetPasswordWithCodeResult { Success = true });
    }

    [LoggerMessage(LogLevel.Information, "Password reset with code attempt for email {Email}")]
    partial void LogPasswordResetWithCode(string email);

    [LoggerMessage(LogLevel.Warning, "Password reset with code failed for email {Email}: {Reason}")]
    partial void LogPasswordResetWithCodeFailed(string email, string reason);

    [LoggerMessage(LogLevel.Information, "Password reset with code successful for email {Email}")]
    partial void LogPasswordResetWithCodeSuccess(string email);
}
