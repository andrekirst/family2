using FamilyHub.Modules.Family.Domain.Abstractions;

namespace FamilyHub.Modules.Family.Persistence;

/// <summary>
/// EF Core implementation of Unit of Work pattern for the Family module.
/// Provides transaction management for Family module operations.
/// </summary>
public sealed class FamilyUnitOfWork(FamilyDbContext context) : IFamilyUnitOfWork
{
    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await context.Database.CommitTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await context.Database.RollbackTransactionAsync(cancellationToken);
    }
}
