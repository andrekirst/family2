using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the User repository.
/// </summary>
public sealed class UserRepository(AuthDbContext context) : IUserRepository
{
    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByExternalProviderAsync(
        string externalProvider,
        string externalUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalProvider))
        {
            throw new ArgumentException("External provider cannot be null or whitespace.", nameof(externalProvider));
        }

        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            throw new ArgumentException("External user ID cannot be null or whitespace.", nameof(externalUserId));
        }

        return await context.Users
            .FirstOrDefaultAsync(
                u => u.ExternalProvider == externalProvider && u.ExternalUserId == externalUserId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByExternalUserIdAsync(
        string externalUserId,
        string externalProvider,
        CancellationToken cancellationToken = default)
    {
        // Delegate to GetByExternalProviderAsync with reversed parameter order
        return await GetByExternalProviderAsync(externalProvider, externalUserId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await context.Users.AddAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // In EF Core with change tracking, this is often not necessary
        // EF tracks changes automatically when entities are loaded from the context
        // However, we include it for explicit updates from detached entities
        context.Users.Update(user);
    }

    /// <inheritdoc />
    public void Remove(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        context.Users.Remove(user);
    }

    /// <inheritdoc />
    public async Task<List<User>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Where(u => u.FamilyId == familyId)
            .ToListAsync(cancellationToken);
    }
}
