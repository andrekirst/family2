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
        EF.CompileAsyncQuery((AppDbContext ctx, ExternalUserId externalId, CancellationToken cancellationToken) =>
            ctx.Users.FirstOrDefault(u => u.ExternalUserId == externalId));

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await context.Users.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<User?> GetByExternalIdAsync(ExternalUserId externalId, CancellationToken cancellationToken = default)
    {
        return await GetByExternalIdCompiledQuery(context, externalId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(user, cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        // EF Core change tracker detects modifications automatically
        return Task.CompletedTask;
    }
}
