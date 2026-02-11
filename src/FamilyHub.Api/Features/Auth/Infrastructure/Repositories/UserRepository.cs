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
    public async Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default)
    {
        return await context.Users.FindAsync([id], cancellationToken: ct);
    }

    public async Task<User?> GetByExternalIdAsync(ExternalUserId externalId, CancellationToken ct = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.ExternalUserId == externalId, ct);
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
        context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}
