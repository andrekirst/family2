namespace FamilyHub.Modules.Auth.Application.Abstractions;

/// <summary>
/// Unit of Work pattern for managing database transactions.
/// Ensures that all repository operations succeed or fail as a single atomic operation.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of entities written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
