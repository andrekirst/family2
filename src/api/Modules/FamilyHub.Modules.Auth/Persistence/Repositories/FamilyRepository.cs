using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the Family repository.
/// </summary>
public sealed class FamilyRepository(AuthDbContext context) : IFamilyRepository
{
    /// <inheritdoc />
    public async Task<Family?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default)
    {
        return await context.Families
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Family?> GetFamilyByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await context.Families
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.Members.Any(u => u.Id == userId), cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Family family, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(family);

        await context.Families.AddAsync(family, cancellationToken);
    }
}
