using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.ResendVerificationEmail;

/// <summary>
/// Handler for ResendVerificationEmailCommand.
/// </summary>
/// <param name="authService">Service handling email verification logic.</param>
/// <param name="userContext">Current user context.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class ResendVerificationEmailCommandHandler(
    IAuthService authService,
    IUserContext userContext,
    ILogger<ResendVerificationEmailCommandHandler> logger)
    : ICommandHandler<ResendVerificationEmailCommand, FamilyHub.SharedKernel.Domain.Result<ResendVerificationEmailResult>>
{
    /// <inheritdoc />
    public async Task<FamilyHub.SharedKernel.Domain.Result<ResendVerificationEmailResult>> Handle(
        ResendVerificationEmailCommand request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        LogResendVerificationEmail(userId.Value);

        var authResult = await authService.ResendVerificationEmailAsync(userId, cancellationToken);

        if (authResult.IsFailure)
        {
            LogResendVerificationEmailFailed(userId.Value, authResult.ErrorMessage ?? "Failed to resend verification email");
            return Result.Failure<ResendVerificationEmailResult>(authResult.ErrorMessage ?? "Failed to resend verification email.");
        }

        LogResendVerificationEmailSuccess(userId.Value);

        return Result.Success(new ResendVerificationEmailResult { Success = true });
    }

    [LoggerMessage(LogLevel.Information, "Resend verification email for user {UserId}")]
    partial void LogResendVerificationEmail(Guid userId);

    [LoggerMessage(LogLevel.Warning, "Resend verification email failed for user {UserId}: {Reason}")]
    partial void LogResendVerificationEmailFailed(Guid userId, string reason);

    [LoggerMessage(LogLevel.Information, "Verification email resent for user {UserId}")]
    partial void LogResendVerificationEmailSuccess(Guid userId);
}
