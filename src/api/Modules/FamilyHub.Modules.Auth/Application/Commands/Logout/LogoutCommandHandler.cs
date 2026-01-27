using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.Logout;

/// <summary>
/// Handler for LogoutCommand.
/// Delegates to IAuthService for logout logic including:
/// - Single session logout (specific refresh token)
/// - All devices logout (revoke all refresh tokens)
/// - Audit logging
/// </summary>
/// <param name="authService">Service handling logout logic.</param>
/// <param name="userContext">Current user context.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class LogoutCommandHandler(
    IAuthService authService,
    IUserContext userContext,
    ILogger<LogoutCommandHandler> logger)
    : ICommandHandler<LogoutCommand, FamilyHub.SharedKernel.Domain.Result<LogoutResult>>
{
    /// <inheritdoc />
    public async Task<FamilyHub.SharedKernel.Domain.Result<LogoutResult>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        LogLogoutAttempt(userId.Value, request.LogoutAllDevices);

        int revokedCount;

        if (request.LogoutAllDevices)
        {
            revokedCount = await authService.LogoutAllDevicesAsync(userId, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            var success = await authService.LogoutAsync(request.RefreshToken, cancellationToken);
            revokedCount = success ? 1 : 0;
        }
        else
        {
            // No refresh token specified and not logging out all devices
            // Just acknowledge the logout (client will discard tokens)
            revokedCount = 0;
        }

        LogLogoutSuccess(userId.Value, revokedCount);

        return Result.Success(new LogoutResult
        {
            Success = true,
            RevokedSessionCount = revokedCount
        });
    }

    [LoggerMessage(LogLevel.Information, "Logout attempt for user {UserId}, all devices: {AllDevices}")]
    partial void LogLogoutAttempt(Guid userId, bool allDevices);

    [LoggerMessage(LogLevel.Warning, "Logout failed for user {UserId}: {Reason}")]
    partial void LogLogoutFailed(Guid userId, string reason);

    [LoggerMessage(LogLevel.Information, "User {UserId} logged out, {RevokedCount} sessions revoked")]
    partial void LogLogoutSuccess(Guid userId, int revokedCount);
}
