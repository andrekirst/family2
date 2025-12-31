using FamilyHub.Modules.Auth.Application.Abstractions;

namespace FamilyHub.Modules.Auth.Persistence;

/// <summary>
/// EF Core implementation of Unit of Work pattern.
/// </summary>
public sealed class UnitOfWork(AuthDbContext context) : IUnitOfWork
{
    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
