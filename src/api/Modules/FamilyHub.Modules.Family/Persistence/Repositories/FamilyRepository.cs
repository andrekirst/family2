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
public sealed class FamilyRepository : IFamilyRepository
{
    private readonly FamilyDbContext _context;
    private readonly IUserLookupService _userLookupService;

    public FamilyRepository(FamilyDbContext context, IUserLookupService userLookupService)
    {
        _context = context;
        _userLookupService = userLookupService;
    }

    /// <inheritdoc />
    public async Task<FamilyAggregate?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default)
    {
        return await _context.Families
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FamilyAggregate?> GetFamilyByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        // Use IUserLookupService for cross-module query
        var familyId = await _userLookupService.GetUserFamilyIdAsync(userId, cancellationToken);

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
        return await _userLookupService.GetFamilyMemberCountAsync(familyId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(FamilyAggregate family, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(family);
        await _context.Families.AddAsync(family, cancellationToken);
    }
}
