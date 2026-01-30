using FamilyHub.Api.Application.Common;
using FamilyHub.Api.Application.Services;
using FamilyHub.Api.Domain.Entities;
using FamilyHub.Api.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Application.Commands;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<OperationResult<RefreshTokenResult>>;

public sealed record RefreshTokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public sealed class RefreshTokenCommandHandler(
    AppDbContext db,
    ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, OperationResult<RefreshTokenResult>>
{
    public async Task<OperationResult<RefreshTokenResult>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        // Find the refresh token
        var existingToken = await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (existingToken == null)
            return OperationResult.Failure<RefreshTokenResult>("Invalid refresh token");

        if (!existingToken.IsActive)
            return OperationResult.Failure<RefreshTokenResult>("Refresh token is expired or revoked");

        var user = existingToken.User!;

        // Revoke old token
        existingToken.Revoke();

        // Generate new tokens
        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email);
        var newRefreshTokenValue = tokenService.GenerateRefreshToken();
        var refreshTokenExpiration = tokenService.GetRefreshTokenExpiration();

        // Create new refresh token
        var newRefreshToken = RefreshToken.Create(
            user.Id,
            newRefreshTokenValue,
            refreshTokenExpiration);

        db.RefreshTokens.Add(newRefreshToken);
        await db.SaveChangesAsync(ct);

        return new RefreshTokenResult(
            accessToken,
            newRefreshTokenValue,
            DateTime.UtcNow.AddMinutes(15),
            refreshTokenExpiration);
    }
}
