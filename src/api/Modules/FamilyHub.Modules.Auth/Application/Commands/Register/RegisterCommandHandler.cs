using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.Register;

/// <summary>
/// Handler for RegisterCommand.
/// Delegates to IAuthService for registration logic including:
/// - Password validation and hashing
/// - User creation with personal family
/// - Email verification token generation
/// - Welcome and verification email sending
/// </summary>
/// <param name="authService">Service handling registration logic.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class RegisterCommandHandler(
    IAuthService authService,
    ILogger<RegisterCommandHandler> logger)
    : ICommandHandler<RegisterCommand, FamilyHub.SharedKernel.Domain.Result<RegisterResult>>
{
    /// <inheritdoc />
    public async Task<FamilyHub.SharedKernel.Domain.Result<RegisterResult>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        LogRegisterAttempt(request.Email.Value);

        // Validate password confirmation
        if (request.Password != request.ConfirmPassword)
        {
            LogRegisterFailed(request.Email.Value, "Passwords do not match");
            return Result.Failure<RegisterResult>("Passwords do not match.");
        }

        var serviceRequest = new RegisterRequest
        {
            Email = request.Email.Value,
            Password = request.Password
        };

        var authResult = await authService.RegisterAsync(serviceRequest, cancellationToken);

        if (authResult.IsFailure)
        {
            LogRegisterFailed(request.Email.Value, authResult.ErrorMessage ?? "Registration failed");
            return Result.Failure<RegisterResult>(authResult.ErrorMessage ?? "Registration failed.");
        }

        LogRegisterSuccess(authResult.UserId!.Value.Value);

        return Result.Success(new RegisterResult
        {
            UserId = authResult.UserId!.Value,
            Email = authResult.Email ?? request.Email,
            EmailVerificationRequired = authResult.User is null || !authResult.User.EmailVerified,
            AccessToken = authResult.AccessToken,
            RefreshToken = authResult.RefreshToken
        });
    }

    [LoggerMessage(LogLevel.Information, "Registration attempt for email {Email}")]
    partial void LogRegisterAttempt(string email);

    [LoggerMessage(LogLevel.Warning, "Registration failed for email {Email}: {Reason}")]
    partial void LogRegisterFailed(string email, string reason);

    [LoggerMessage(LogLevel.Information, "User {UserId} registered successfully")]
    partial void LogRegisterSuccess(Guid userId);
}
