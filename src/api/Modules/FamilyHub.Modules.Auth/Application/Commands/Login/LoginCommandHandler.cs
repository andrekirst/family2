using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FamilyHub.Modules.Auth.Application.Commands.Login;

/// <summary>
/// Handler for LoginCommand.
/// Delegates to IAuthService for authentication logic including:
/// - Password verification
/// - Lockout checking and increment
/// - Audit logging
/// - Token generation
/// </summary>
/// <param name="authService">Service handling authentication logic.</param>
/// <param name="jwtSettings">JWT configuration for expiration time.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class LoginCommandHandler(
    IAuthService authService,
    IOptions<JwtSettings> jwtSettings,
    ILogger<LoginCommandHandler> logger)
    : ICommandHandler<LoginCommand, FamilyHub.SharedKernel.Domain.Result<LoginResult>>
{
    /// <inheritdoc />
    public async Task<FamilyHub.SharedKernel.Domain.Result<LoginResult>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        LogLoginAttempt(request.Email.Value);

        var serviceRequest = new LoginRequest
        {
            Email = request.Email.Value,
            Password = request.Password
        };

        var authResult = await authService.LoginAsync(serviceRequest, cancellationToken);

        if (authResult.IsFailure)
        {
            LogLoginFailed(request.Email.Value, authResult.ErrorMessage ?? "Login failed");
            return Result.Failure<LoginResult>(authResult.ErrorMessage ?? "Login failed.");
        }

        LogLoginSuccess(authResult.UserId!.Value.Value);

        return Result.Success(new LoginResult
        {
            UserId = authResult.UserId!.Value,
            Email = authResult.Email ?? request.Email,
            AccessToken = authResult.AccessToken!,
            RefreshToken = authResult.RefreshToken!,
            ExpiresIn = jwtSettings.Value.AccessTokenExpirationMinutes * 60,
            FamilyId = authResult.User?.FamilyId,
            EmailVerified = authResult.User?.EmailVerified ?? false
        });
    }

    [LoggerMessage(LogLevel.Information, "Login attempt for email {Email}")]
    partial void LogLoginAttempt(string email);

    [LoggerMessage(LogLevel.Warning, "Login failed for email {Email}: {Reason}")]
    partial void LogLoginFailed(string email, string reason);

    [LoggerMessage(LogLevel.Information, "User {UserId} logged in successfully")]
    partial void LogLoginSuccess(Guid userId);
}
