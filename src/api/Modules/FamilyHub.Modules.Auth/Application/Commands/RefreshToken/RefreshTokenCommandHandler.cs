using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FamilyHub.Modules.Auth.Application.Commands.RefreshToken;

/// <summary>
/// Handler for RefreshTokenCommand.
/// Delegates to IAuthService for token refresh logic including:
/// - Refresh token validation
/// - Token rotation (revoke old, issue new)
/// - Audit logging
/// </summary>
/// <param name="authService">Service handling token refresh logic.</param>
/// <param name="jwtSettings">JWT configuration for expiration time.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class RefreshTokenCommandHandler(
    IAuthService authService,
    IOptions<JwtSettings> jwtSettings,
    ILogger<RefreshTokenCommandHandler> logger)
    : ICommandHandler<RefreshTokenCommand, FamilyHub.SharedKernel.Domain.Result<RefreshTokenResult>>
{
    /// <inheritdoc />
    public async Task<FamilyHub.SharedKernel.Domain.Result<RefreshTokenResult>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        LogRefreshAttempt();

        var authResult = await authService.RefreshTokenAsync(
            request.RefreshToken,
            cancellationToken);

        if (authResult.IsFailure)
        {
            LogRefreshFailed(authResult.ErrorMessage ?? "Token refresh failed");
            return Result.Failure<RefreshTokenResult>(authResult.ErrorMessage ?? "Token refresh failed.");
        }

        LogRefreshSuccess(authResult.UserId!.Value.Value);

        return Result.Success(new RefreshTokenResult
        {
            AccessToken = authResult.AccessToken!,
            RefreshToken = authResult.RefreshToken!,
            ExpiresIn = jwtSettings.Value.AccessTokenExpirationMinutes * 60,
            UserId = authResult.UserId!.Value
        });
    }

    [LoggerMessage(LogLevel.Debug, "Token refresh attempt")]
    partial void LogRefreshAttempt();

    [LoggerMessage(LogLevel.Warning, "Token refresh failed: {Reason}")]
    partial void LogRefreshFailed(string reason);

    [LoggerMessage(LogLevel.Debug, "Token refreshed for user {UserId}")]
    partial void LogRefreshSuccess(Guid userId);
}
