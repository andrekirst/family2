
using FamilyHub.Modules.Family.Domain;
using FamilyHub.SharedKernel.ValueObjects;

namespace FamilyHub.Modules.Family.Persistence.Repositories;
/// <summary>
/// Repository for managing EmailOutbox entities.
/// </summary>
public interface IEmailOutboxRepository
{
    /// <summary>
    /// Retrieves an email outbox entry by its identifier.
    /// </summary>
    /// <param name="id">The email outbox identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The email outbox entry if found; otherwise, null.</returns>
    Task<EmailOutbox?> GetByIdAsync(EmailOutboxId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves pending emails ready to be sent.
    /// </summary>
    /// <param name="batchSize">Maximum number of emails to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of pending emails ordered by creation time.</returns>
    Task<List<EmailOutbox>> GetPendingEmailsAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves failed emails that are eligible for retry.
    /// </summary>
    /// <param name="batchSize">Maximum number of emails to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of failed emails eligible for retry.</returns>
    Task<List<EmailOutbox>> GetFailedEmailsForRetryAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new email outbox entry.
    /// </summary>
    /// <param name="emailOutbox">The email outbox entry to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(EmailOutbox emailOutbox, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing email outbox entry.
    /// </summary>
    /// <param name="emailOutbox">The email outbox entry to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(EmailOutbox emailOutbox, CancellationToken cancellationToken = default);
}
