using System.Security.Claims;
using FamilyHub.Api.Application.Commands;
using FamilyHub.Api.Application.Common;
using HotChocolate.Authorization;
using MediatR;

namespace FamilyHub.Api.GraphQL;

public class Mutations
{
    public async Task<MutationResult<RegisterResult>> Register(
        RegisterCommand input,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(input, ct);
        return result.IsSuccess
            ? new MutationResult<RegisterResult>(result.Value)
            : new MutationResult<RegisterResult>(new AuthError(result.Error!));
    }

    public async Task<MutationResult<LoginResult>> Login(
        LoginCommand input,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(input, ct);
        return result.IsSuccess
            ? new MutationResult<LoginResult>(result.Value)
            : new MutationResult<LoginResult>(new AuthError(result.Error!));
    }

    [Authorize]
    public async Task<MutationResult<bool>> Logout(
        string? refreshToken,
        ClaimsPrincipal claimsPrincipal,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        var userId = GetUserId(claimsPrincipal);
        if (userId == null)
            return new MutationResult<bool>(new AuthError("Invalid user"));

        var command = new LogoutCommand(userId.Value, refreshToken);
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? new MutationResult<bool>(true)
            : new MutationResult<bool>(new AuthError(result.Error!));
    }

    public async Task<MutationResult<RefreshTokenResult>> RefreshToken(
        RefreshTokenCommand input,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(input, ct);
        return result.IsSuccess
            ? new MutationResult<RefreshTokenResult>(result.Value)
            : new MutationResult<RefreshTokenResult>(new AuthError(result.Error!));
    }

    public async Task<MutationResult<VerifyEmailResult>> VerifyEmail(
        VerifyEmailCommand input,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(input, ct);
        return result.IsSuccess
            ? new MutationResult<VerifyEmailResult>(result.Value)
            : new MutationResult<VerifyEmailResult>(new AuthError(result.Error!));
    }

    [Authorize]
    public async Task<MutationResult<bool>> ResendVerificationEmail(
        ClaimsPrincipal claimsPrincipal,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        var userId = GetUserId(claimsPrincipal);
        if (userId == null)
            return new MutationResult<bool>(new AuthError("Invalid user"));

        var command = new ResendVerificationCommand(userId.Value);
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? new MutationResult<bool>(true)
            : new MutationResult<bool>(new AuthError(result.Error!));
    }

    public async Task<MutationResult<bool>> RequestPasswordReset(
        RequestPasswordResetCommand input,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(input, ct);
        // Always return success to prevent email enumeration
        return new MutationResult<bool>(true);
    }

    public async Task<MutationResult<bool>> ResetPassword(
        ResetPasswordCommand input,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(input, ct);
        return result.IsSuccess
            ? new MutationResult<bool>(true)
            : new MutationResult<bool>(new AuthError(result.Error!));
    }

    [Authorize]
    public async Task<MutationResult<bool>> ChangePassword(
        string currentPassword,
        string newPassword,
        string confirmPassword,
        ClaimsPrincipal claimsPrincipal,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        var userId = GetUserId(claimsPrincipal);
        if (userId == null)
            return new MutationResult<bool>(new AuthError("Invalid user"));

        var command = new ChangePasswordCommand(userId.Value, currentPassword, newPassword, confirmPassword);
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? new MutationResult<bool>(true)
            : new MutationResult<bool>(new AuthError(result.Error!));
    }

    private static Guid? GetUserId(ClaimsPrincipal claimsPrincipal)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

// Wrapper for mutation results with errors
public class MutationResult<T>
{
    public T? Data { get; }
    public AuthError? Error { get; }
    public bool Success => Error == null;

    public MutationResult(T data)
    {
        Data = data;
    }

    public MutationResult(AuthError error)
    {
        Error = error;
    }
}
