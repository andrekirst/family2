using FamilyHub.Modules.Auth.Application.Abstractions;

namespace FamilyHub.Modules.Auth.Persistence;

/// <summary>
/// EF Core implementation of Unit of Work pattern.
/// </summary>
public sealed class UnitOfWork(AuthDbContext context) : IUnitOfWork
{
    private readonly AuthDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
