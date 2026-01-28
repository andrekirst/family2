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
using FamilyHub.SharedKernel.Domain.Exceptions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL.Errors;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using MediatR;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;

/// <summary>
/// GraphQL mutations for authentication operations.
/// Extends the AuthMutations namespace type.
/// </summary>
/// <remarks>
/// <para>
/// Uses HotChocolate mutation conventions for consistent error handling.
/// All mutations automatically include error union types via [Error] attributes.
/// </para>
/// <para>
/// Access pattern: mutation { auth { register(...) { data { ... } errors { ... } } } }
/// </para>
/// </remarks>
[ExtendObjectType(typeof(AuthMutations))]
public sealed class AuthMutationsExtensions
{
    /// <summary>
    /// Register a new user account.
    /// Creates user, personal family, and sends verification email.
    /// </summary>
    [GraphQLDescription("Register a new user account with email and password.")]
    [UseMutationConvention]
    [Error<ValidationError>]
    [Error<BusinessError>]
    [Error<ConflictError>]
    public async Task<RegisterResult> Register(
        RegisterInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            Email.From(input.Email),
            input.Password,
            input.ConfirmPassword);

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<RegisterResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("REGISTRATION_FAILED", result.Error);
        }

        return result.Value;
    }

    /// <summary>
    /// Authenticate user with email and password.
    /// Returns JWT access token and refresh token on success.
    /// </summary>
    [GraphQLDescription("Login with email and password.")]
    [UseMutationConvention]
    [Error<ValidationError>]
    [Error<BusinessError>]
    [Error<UnauthorizedError>]
    public async Task<LoginResult> Login(
        LoginInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(
            Email.From(input.Email),
            input.Password);

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<LoginResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("LOGIN_FAILED", result.Error);
        }

        return result.Value;
    }

    /// <summary>
    /// Log out the current user.
    /// Optionally revokes the specified refresh token.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Log out and optionally revoke refresh token.")]
    [UseMutationConvention]
    [Error<BusinessError>]
    public async Task<LogoutResult> Logout(
        string? refreshToken,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(refreshToken);

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<LogoutResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("LOGOUT_FAILED", result.Error);
        }

        return result.Value;
    }

    /// <summary>
    /// Log out from all devices by revoking all refresh tokens.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Log out from all devices.")]
    [UseMutationConvention]
    [Error<BusinessError>]
    public async Task<LogoutResult> LogoutAllDevices(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(LogoutAllDevices: true);

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<LogoutResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("LOGOUT_FAILED", result.Error);
        }

        return result.Value;
    }

    /// <summary>
    /// Refresh authentication tokens using a refresh token.
    /// Implements token rotation - old refresh token is revoked.
    /// </summary>
    [GraphQLDescription("Refresh access token using refresh token.")]
    [UseMutationConvention]
    [Error<ValidationError>]
    [Error<BusinessError>]
    public async Task<RefreshTokenResult> RefreshToken(
        string refreshToken,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(refreshToken);

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<RefreshTokenResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("TOKEN_REFRESH_FAILED", result.Error);
        }

        return result.Value;
    }

    /// <summary>
    /// Change the authenticated user's password.
    /// Requires current password verification.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Change password for the authenticated user.")]
    [UseMutationConvention]
    [Error<ValidationError>]
    [Error<BusinessError>]
    public async Task<ChangePasswordResult> ChangePassword(
        ChangePasswordInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand(
            input.CurrentPassword,
            input.NewPassword,
            input.ConfirmNewPassword);

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<ChangePasswordResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("PASSWORD_CHANGE_FAILED", result.Error);
        }

        return result.Value;
    }

    /// <summary>
    /// Request a password reset.
    /// Sends reset link or code to the user's email.
    /// </summary>
    [GraphQLDescription("Request a password reset link or code.")]
    [UseMutationConvention]
    [Error<ValidationError>]
    [Error<BusinessError>]
    public async Task<RequestPasswordResetResult> RequestPasswordReset(
        RequestPasswordResetInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RequestPasswordResetCommand(
            Email.From(input.Email),
            input.UseMobileCode);

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<RequestPasswordResetResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("PASSWORD_RESET_REQUEST_FAILED", result.Error);
        }

        return result.Value;
    }

    /// <summary>
    /// Reset password using a token from email link.
    /// </summary>
    [GraphQLDescription("Reset password using token from email link.")]
    [UseMutationConvention]
    [Error<ValidationError>]
    [Error<BusinessError>]
    public async Task<ResetPasswordResult> ResetPassword(
        ResetPasswordInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(
            input.Token,
            input.NewPassword,
            input.ConfirmNewPassword);

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<ResetPasswordResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("PASSWORD_RESET_FAILED", result.Error);
        }

        return result.Value;
    }

    /// <summary>
    /// Reset password using a 6-digit code (mobile flow).
    /// </summary>
    [GraphQLDescription("Reset password using 6-digit code from email.")]
    [UseMutationConvention]
    [Error<ValidationError>]
    [Error<BusinessError>]
    public async Task<ResetPasswordWithCodeResult> ResetPasswordWithCode(
        ResetPasswordWithCodeInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordWithCodeCommand(
            Email.From(input.Email),
            input.Code,
            input.NewPassword,
            input.ConfirmNewPassword);

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<ResetPasswordWithCodeResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("PASSWORD_RESET_FAILED", result.Error);
        }

        return result.Value;
    }

    /// <summary>
    /// Verify user's email address using token from email link.
    /// </summary>
    [GraphQLDescription("Verify email address using token.")]
    [UseMutationConvention]
    [Error<ValidationError>]
    [Error<BusinessError>]
    public async Task<VerifyEmailResult> VerifyEmail(
        string token,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new VerifyEmailCommand(token);

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<VerifyEmailResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("EMAIL_VERIFICATION_FAILED", result.Error);
        }

        return result.Value;
    }

    /// <summary>
    /// Resend email verification link.
    /// Requires authentication.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Resend email verification link.")]
    [UseMutationConvention]
    [Error<BusinessError>]
    public async Task<ResendVerificationEmailResult> ResendVerificationEmail(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ResendVerificationEmailCommand();

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<ResendVerificationEmailResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("RESEND_VERIFICATION_FAILED", result.Error);
        }

        return result.Value;
    }
}
