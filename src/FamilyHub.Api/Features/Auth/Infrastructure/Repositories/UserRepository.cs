using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Auth.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IUserRepository.
/// </summary>
public sealed class UserRepository(AppDbContext context) : IUserRepository
{
    private static readonly Func<AppDbContext, ExternalUserId, CancellationToken, Task<User?>> GetByExternalIdCompiledQuery =
        EF.CompileAsyncQuery((AppDbContext ctx, ExternalUserId externalId, CancellationToken ct) =>
            ctx.Users.FirstOrDefault(u => u.ExternalUserId == externalId));

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default)
    {
        return await context.Users.FindAsync([id], cancellationToken: ct);
    }

    public async Task<User?> GetByExternalIdAsync(ExternalUserId externalId, CancellationToken ct = default)
    {
        return await GetByExternalIdCompiledQuery(context, externalId, ct);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await context.Users.AddAsync(user, ct);
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        // EF Core change tracker detects modifications automatically
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}
