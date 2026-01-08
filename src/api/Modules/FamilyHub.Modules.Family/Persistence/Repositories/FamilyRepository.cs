using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Family.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the Family repository.
/// </summary>
public sealed class FamilyRepository(FamilyDbContext context) : IFamilyRepository
{
    /// <inheritdoc />
    public async Task<global::FamilyHub.Modules.Family.Domain.Family?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default)
    {
        return await context.Families
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<global::FamilyHub.Modules.Family.Domain.Family?> GetFamilyByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        // NOTE: In the Family module, we don't have access to User navigation property
        // This query relies on the existing auth.users table structure
        // The Auth module will handle user-family relationships
        // For now, we query by OwnerId only
        return await context.Families
            .FirstOrDefaultAsync(f => f.OwnerId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(global::FamilyHub.Modules.Family.Domain.Family family, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(family);

        await context.Families.AddAsync(family, cancellationToken);
    }
}
