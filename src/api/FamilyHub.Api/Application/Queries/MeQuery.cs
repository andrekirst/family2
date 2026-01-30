using FamilyHub.Api.Application.Common;
using FamilyHub.Api.Domain.ValueObjects;
using FamilyHub.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Application.Queries;

public sealed record MeQuery(Guid UserId) : IRequest<OperationResult<MeResult>>;

public sealed record MeResult(
    Guid Id,
    string Email,
    bool EmailVerified,
    DateTime? EmailVerifiedAt,
    DateTime CreatedAt);

public sealed class MeQueryHandler(AppDbContext db)
    : IRequestHandler<MeQuery, OperationResult<MeResult>>
{
    public async Task<OperationResult<MeResult>> Handle(MeQuery request, CancellationToken ct)
    {
        var userId = UserId.From(request.UserId);

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
            return OperationResult.Failure<MeResult>("User not found");

        return new MeResult(
            user.Id.Value,
            user.Email.Value,
            user.EmailVerified,
            user.EmailVerifiedAt,
            user.CreatedAt);
    }
}
