using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Modules.Family.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the Family repository using FamilyDbContext.
///
/// PHASE 5 STATE: Repository now resides in Family module with its own DbContext.
///
/// CROSS-MODULE QUERIES:
/// - GetFamilyByUserIdAsync and GetMemberCountAsync require data from Auth module
/// - These queries are handled via IUserLookupService abstraction
/// - This maintains bounded context separation while enabling necessary cross-module operations
/// </summary>
/// <param name="context">The Family module database context.</param>
/// <param name="userLookupService">Service for cross-module user lookups.</param>
public sealed class FamilyRepository(FamilyDbContext context, IUserLookupService userLookupService) : IFamilyRepository
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
        // Use IUserLookupService for cross-module query
        var familyId = await userLookupService.GetUserFamilyIdAsync(userId, cancellationToken);

        if (familyId == null)
        {
            return null;
        }

        return await GetByIdAsync(familyId.Value, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetMemberCountAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        // Use IUserLookupService for cross-module query
        return await userLookupService.GetFamilyMemberCountAsync(familyId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(FamilyAggregate family, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(family);
        await context.Families.AddAsync(family, cancellationToken);
    }
}
