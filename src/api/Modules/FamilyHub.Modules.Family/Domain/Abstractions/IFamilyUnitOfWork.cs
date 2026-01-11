using FamilyHub.SharedKernel.Interfaces;

namespace FamilyHub.Modules.Family.Domain.Abstractions;

/// <summary>
/// Unit of Work interface for the Family module.
/// Extends base IUnitOfWork with module-specific functionality if needed.
///
/// This interface enables the Family module to manage its own transactions
/// independently from other modules, supporting bounded context separation.
/// </summary>
public interface IFamilyUnitOfWork : IUnitOfWork
{
    // Module-specific methods can be added here if needed
    // Currently inherits all methods from IUnitOfWork:
    // - SaveChangesAsync
    // - BeginTransactionAsync
    // - CommitTransactionAsync
    // - RollbackTransactionAsync
}
