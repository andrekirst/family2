namespace FamilyHub.Api.Common.Application;

/// <summary>
/// Abstraction for committing changes across repositories.
/// Called by the TransactionBehavior pipeline after handler execution.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
