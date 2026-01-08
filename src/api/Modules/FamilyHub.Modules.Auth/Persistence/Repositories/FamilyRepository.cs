using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the Family repository.
/// NOTE: Family aggregate no longer has Members navigation property.
/// Member queries should be done separately through UserRepository if needed.
///
/// PHASE 3 STATE: This repository implements IFamilyRepository from Family module
/// but remains in Auth module's Persistence layer for pragmatic reasons:
/// - Avoids circular dependency (Auth -> Family -> Auth)
/// - Shares AuthDbContext with other Auth repositories
/// - All entities remain in same "auth" schema
///
/// FUTURE: In Phase 5+, this will be moved to Family module when we introduce
/// a separate FamilyDbContext and resolve the cross-module database coupling.
/// </summary>
public sealed class FamilyRepository(AuthDbContext context) : IFamilyRepository
{
    /// <inheritdoc />
    public async Task<FamilyAggregate?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default)
    {
        return await context.Families
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FamilyAggregate?> GetFamilyByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        // Find the user first to get their FamilyId
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        // Then fetch the family
        return await GetByIdAsync(user.FamilyId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetMemberCountAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Where(u => u.FamilyId == familyId)
            .CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(FamilyAggregate family, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(family);

        await context.Families.AddAsync(family, cancellationToken);
    }
}
