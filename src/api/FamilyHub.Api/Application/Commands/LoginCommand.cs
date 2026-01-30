using FamilyHub.Api.Application.Common;
using FamilyHub.Api.Application.Services;
using FamilyHub.Api.Domain.Entities;
using FamilyHub.Api.Domain.ValueObjects;
using FamilyHub.Api.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Application.Commands;

public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<OperationResult<LoginResult>>;

public sealed record LoginResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    UserInfo User);

public sealed record UserInfo(
    Guid Id,
    string Email,
    bool EmailVerified);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(
    AppDbContext db,
    IPasswordService passwordService,
    ITokenService tokenService)
    : IRequestHandler<LoginCommand, OperationResult<LoginResult>>
{
    public async Task<OperationResult<LoginResult>> Handle(LoginCommand request, CancellationToken ct)
    {
        // Validate email format
        if (!Email.TryFrom(request.Email, out var email))
            return OperationResult.Failure<LoginResult>("Invalid email or password");

        // Find user
        var user = await db.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user == null)
            return OperationResult.Failure<LoginResult>("Invalid email or password");

        // Check lockout
        user.CheckAndClearExpiredLockout();
        if (user.IsLockedOut)
            return OperationResult.Failure<LoginResult>("Account is locked. Please try again later.");

        // Verify password
        if (!passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await db.SaveChangesAsync(ct);
            return OperationResult.Failure<LoginResult>("Invalid email or password");
        }

        // Generate tokens
        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshTokenValue = tokenService.GenerateRefreshToken();
        var refreshTokenExpiration = tokenService.GetRefreshTokenExpiration();

        // Create refresh token entity
        var refreshToken = RefreshToken.Create(
            user.Id,
            refreshTokenValue,
            refreshTokenExpiration);

        user.RefreshTokens.Add(refreshToken);
        user.RecordSuccessfulLogin();
        await db.SaveChangesAsync(ct);

        return new LoginResult(
            accessToken,
            refreshTokenValue,
            DateTime.UtcNow.AddMinutes(15),
            refreshTokenExpiration,
            new UserInfo(user.Id.Value, user.Email.Value, user.EmailVerified));
    }
}
