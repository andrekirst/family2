using FamilyHub.SharedKernel.Interfaces;

namespace FamilyHub.Modules.Auth.Persistence;

/// <summary>
/// EF Core implementation of Unit of Work pattern.
/// </summary>
public sealed class UnitOfWork(AuthDbContext context) : IUnitOfWork
{
    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await context.SaveChangesAsync(cancellationToken);

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) => await context.Database.BeginTransactionAsync(cancellationToken);

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default) => await context.Database.CommitTransactionAsync(cancellationToken);

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => await context.Database.RollbackTransactionAsync(cancellationToken);
}
