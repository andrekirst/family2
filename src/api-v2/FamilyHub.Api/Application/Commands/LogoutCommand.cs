using FamilyHub.Api.Application.Common;
using FamilyHub.Api.Domain.ValueObjects;
using FamilyHub.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Application.Commands;

public sealed record LogoutCommand(
    Guid UserId,
    string? RefreshToken = null) : IRequest<OperationResult>;

public sealed class LogoutCommandHandler(AppDbContext db)
    : IRequestHandler<LogoutCommand, OperationResult>
{
    public async Task<OperationResult> Handle(LogoutCommand request, CancellationToken ct)
    {
        var userId = UserId.From(request.UserId);

        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            // Revoke all refresh tokens for this user
            var tokens = await db.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync(ct);

            foreach (var token in tokens)
                token.Revoke();
        }
        else
        {
            // Revoke specific refresh token
            var token = await db.RefreshTokens
                .FirstOrDefaultAsync(rt =>
                    rt.UserId == userId &&
                    rt.Token == request.RefreshToken &&
                    rt.RevokedAt == null, ct);

            token?.Revoke();
        }

        await db.SaveChangesAsync(ct);
        return OperationResult.Success();
    }
}
