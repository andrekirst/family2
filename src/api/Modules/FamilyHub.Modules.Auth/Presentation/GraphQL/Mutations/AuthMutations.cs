using FamilyHub.Modules.Auth.Application.Commands.ChangePassword;
using FamilyHub.Modules.Auth.Application.Commands.Login;
using FamilyHub.Modules.Auth.Application.Commands.Logout;
using FamilyHub.Modules.Auth.Application.Commands.RefreshToken;
using FamilyHub.Modules.Auth.Application.Commands.Register;
using FamilyHub.Modules.Auth.Application.Commands.RequestPasswordReset;
using FamilyHub.Modules.Auth.Application.Commands.ResendVerificationEmail;
using FamilyHub.Modules.Auth.Application.Commands.ResetPassword;
using FamilyHub.Modules.Auth.Application.Commands.ResetPasswordWithCode;
using FamilyHub.Modules.Auth.Application.Commands.VerifyEmail;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using HotChocolate.Authorization;
using MediatR;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for authentication operations.
/// Provides local email/password authentication with support for:
/// - User registration with email verification
/// - Login with lockout protection
/// - Password reset (email link and mobile code)
/// - Token refresh and logout
/// </summary>
[ExtendObjectType("Mutation")]
public sealed class AuthMutations
{
    /// <summary>
    /// Register a new user account.
    /// Creates user, personal family, and sends verification email.
    /// </summary>
    [GraphQLDescription("Register a new user account with email and password.")]
    public async Task<RegisterPayload> Register(
        RegisterInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            Email.From(input.Email),
            input.Password,
            input.ConfirmPassword);

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<RegisterResult>>(command, cancellationToken);

        return result.IsSuccess
            ? new RegisterPayload
            {
                UserId = result.Value.UserId.Value,
                Email = result.Value.Email.Value,
                EmailVerificationRequired = result.Value.EmailVerificationRequired,
                AccessToken = result.Value.AccessToken,
                RefreshToken = result.Value.RefreshToken
            }
            : new RegisterPayload { Errors = [new PayloadError("REGISTRATION_FAILED", result.Error)] };
    }

    /// <summary>
    /// Authenticate user with email and password.
    /// Returns JWT access token and refresh token on success.
    /// </summary>
    [GraphQLDescription("Login with email and password.")]
    public async Task<LoginPayload> Login(
        LoginInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(
            Email.From(input.Email),
            input.Password);

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<LoginResult>>(command, cancellationToken);

        return result.IsSuccess
            ? new LoginPayload
            {
                UserId = result.Value.UserId.Value,
                Email = result.Value.Email.Value,
                AccessToken = result.Value.AccessToken,
                RefreshToken = result.Value.RefreshToken,
                ExpiresIn = result.Value.ExpiresIn,
                FamilyId = result.Value.FamilyId?.Value,
                EmailVerified = result.Value.EmailVerified
            }
            : new LoginPayload { Errors = [new PayloadError("LOGIN_FAILED", result.Error)] };
    }

    /// <summary>
    /// Log out the current user.
    /// Optionally revokes the specified refresh token.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Log out and optionally revoke refresh token.")]
    public async Task<LogoutPayload> Logout(
        string? refreshToken,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(refreshToken);

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<LogoutResult>>(command, cancellationToken);

        return result.IsSuccess
            ? new LogoutPayload
            {
                Success = result.Value.Success,
                RevokedSessionCount = result.Value.RevokedSessionCount
            }
            : new LogoutPayload { Errors = [new PayloadError("LOGOUT_FAILED", result.Error)] };
    }

    /// <summary>
    /// Log out from all devices by revoking all refresh tokens.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Log out from all devices.")]
    public async Task<LogoutPayload> LogoutAllDevices(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(LogoutAllDevices: true);

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<LogoutResult>>(command, cancellationToken);

        return result.IsSuccess
            ? new LogoutPayload
            {
                Success = result.Value.Success,
                RevokedSessionCount = result.Value.RevokedSessionCount
            }
            : new LogoutPayload { Errors = [new PayloadError("LOGOUT_FAILED", result.Error)] };
    }

    /// <summary>
    /// Refresh authentication tokens using a refresh token.
    /// Implements token rotation - old refresh token is revoked.
    /// </summary>
    [GraphQLDescription("Refresh access token using refresh token.")]
    public async Task<RefreshTokenPayload> RefreshToken(
        string refreshToken,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(refreshToken);

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<RefreshTokenResult>>(command, cancellationToken);

        return result.IsSuccess
            ? new RefreshTokenPayload
            {
                AccessToken = result.Value.AccessToken,
                RefreshToken = result.Value.RefreshToken,
                ExpiresIn = result.Value.ExpiresIn
            }
            : new RefreshTokenPayload { Errors = [new PayloadError("TOKEN_REFRESH_FAILED", result.Error)] };
    }

    /// <summary>
    /// Change the authenticated user's password.
    /// Requires current password verification.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Change password for the authenticated user.")]
    public async Task<ChangePasswordPayload> ChangePassword(
        ChangePasswordInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand(
            input.CurrentPassword,
            input.NewPassword,
            input.ConfirmNewPassword);

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<ChangePasswordResult>>(command, cancellationToken);

        return result.IsSuccess
            ? new ChangePasswordPayload { Success = true }
            : new ChangePasswordPayload { Errors = [new PayloadError("PASSWORD_CHANGE_FAILED", result.Error)] };
    }

    /// <summary>
    /// Request a password reset.
    /// Sends reset link or code to the user's email.
    /// </summary>
    [GraphQLDescription("Request a password reset link or code.")]
    public async Task<RequestPasswordResetPayload> RequestPasswordReset(
        RequestPasswordResetInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RequestPasswordResetCommand(
            Email.From(input.Email),
            input.UseMobileCode);

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<RequestPasswordResetResult>>(command, cancellationToken);

        return result.IsSuccess
            ? new RequestPasswordResetPayload
            {
                Success = result.Value.Success,
                Message = result.Value.Message
            }
            : new RequestPasswordResetPayload { Errors = [new PayloadError("PASSWORD_RESET_REQUEST_FAILED", result.Error)] };
    }

    /// <summary>
    /// Reset password using a token from email link.
    /// </summary>
    [GraphQLDescription("Reset password using token from email link.")]
    public async Task<ResetPasswordPayload> ResetPassword(
        ResetPasswordInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(
            input.Token,
            input.NewPassword,
            input.ConfirmNewPassword);

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<ResetPasswordResult>>(command, cancellationToken);

        return result.IsSuccess
            ? new ResetPasswordPayload { Success = true }
            : new ResetPasswordPayload { Errors = [new PayloadError("PASSWORD_RESET_FAILED", result.Error)] };
    }

    /// <summary>
    /// Reset password using a 6-digit code (mobile flow).
    /// </summary>
    [GraphQLDescription("Reset password using 6-digit code from email.")]
    public async Task<ResetPasswordPayload> ResetPasswordWithCode(
        ResetPasswordWithCodeInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordWithCodeCommand(
            Email.From(input.Email),
            input.Code,
            input.NewPassword,
            input.ConfirmNewPassword);

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<ResetPasswordWithCodeResult>>(command, cancellationToken);

        return result.IsSuccess
            ? new ResetPasswordPayload { Success = true }
            : new ResetPasswordPayload { Errors = [new PayloadError("PASSWORD_RESET_FAILED", result.Error)] };
    }

    /// <summary>
    /// Verify user's email address using token from email link.
    /// </summary>
    [GraphQLDescription("Verify email address using token.")]
    public async Task<VerifyEmailPayload> VerifyEmail(
        string token,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new VerifyEmailCommand(token);

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<VerifyEmailResult>>(command, cancellationToken);

        return result.IsSuccess
            ? new VerifyEmailPayload
            {
                Success = result.Value.Success,
                Message = result.Value.Message
            }
            : new VerifyEmailPayload { Errors = [new PayloadError("EMAIL_VERIFICATION_FAILED", result.Error)] };
    }

    /// <summary>
    /// Resend email verification link.
    /// Requires authentication.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Resend email verification link.")]
    public async Task<ResendVerificationEmailPayload> ResendVerificationEmail(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ResendVerificationEmailCommand();

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<ResendVerificationEmailResult>>(command, cancellationToken);

        return result.IsSuccess
            ? new ResendVerificationEmailPayload
            {
                Success = result.Value.Success,
                Message = result.Value.Message
            }
            : new ResendVerificationEmailPayload { Errors = [new PayloadError("RESEND_VERIFICATION_FAILED", result.Error)] };
    }
}
