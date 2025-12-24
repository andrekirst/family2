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
    private readonly AuthDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public async Task<Family?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default)
    {
        return await _context.Families
            .Include(f => f.UserFamilies)
            .Where(f => f.DeletedAt == null)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Family>> GetFamiliesByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _context.Families
            .Include(f => f.UserFamilies)
            .Where(f => f.DeletedAt == null && f.UserFamilies.Any(uf => uf.UserId == userId && uf.IsActive))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Family family, CancellationToken cancellationToken = default)
    {
        if (family == null)
        {
            throw new ArgumentNullException(nameof(family));
        }

        await _context.Families.AddAsync(family, cancellationToken);
    }
}
