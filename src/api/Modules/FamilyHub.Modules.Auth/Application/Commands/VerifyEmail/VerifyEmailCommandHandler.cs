using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.VerifyEmail;

/// <summary>
/// Handler for VerifyEmailCommand.
/// </summary>
/// <param name="authService">Service handling email verification logic.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class VerifyEmailCommandHandler(
    IAuthService authService,
    ILogger<VerifyEmailCommandHandler> logger)
    : ICommandHandler<VerifyEmailCommand, FamilyHub.SharedKernel.Domain.Result<VerifyEmailResult>>
{
    /// <inheritdoc />
    public async Task<FamilyHub.SharedKernel.Domain.Result<VerifyEmailResult>> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        LogVerifyEmailAttempt();

        var authResult = await authService.VerifyEmailAsync(request.Token, cancellationToken);

        if (authResult.IsFailure)
        {
            LogVerifyEmailFailed(authResult.ErrorMessage ?? "Email verification failed");
            return Result.Failure<VerifyEmailResult>(authResult.ErrorMessage ?? "Email verification failed.");
        }

        LogVerifyEmailSuccess();

        return Result.Success(new VerifyEmailResult { Success = true });
    }

    [LoggerMessage(LogLevel.Information, "Email verification attempt")]
    partial void LogVerifyEmailAttempt();

    [LoggerMessage(LogLevel.Warning, "Email verification failed: {Reason}")]
    partial void LogVerifyEmailFailed(string reason);

    [LoggerMessage(LogLevel.Information, "Email verification successful")]
    partial void LogVerifyEmailSuccess();
}
