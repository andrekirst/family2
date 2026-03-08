namespace FamilyHub.Common.Application;

/// <summary>
/// Abstraction for committing changes across repositories.
/// Called by the TransactionBehavior pipeline after handler execution.
/// Provides explicit transaction support for atomic command handling.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    bool HasChanges { get; }
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    bool HasActiveTransaction { get; }
}
