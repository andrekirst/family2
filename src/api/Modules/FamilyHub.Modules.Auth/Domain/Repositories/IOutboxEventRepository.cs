using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Repositories;

/// <summary>
/// Repository for managing outbox events.
/// </summary>
public interface IOutboxEventRepository
{
    /// <summary>
    /// Adds a new outbox event to the repository.
    /// </summary>
    Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple outbox events to the repository.
    /// </summary>
    Task AddRangeAsync(IEnumerable<OutboxEvent> outboxEvents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending outbox events for processing.
    /// </summary>
    /// <param name="batchSize">Maximum number of events to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of pending outbox events ordered by creation time.</returns>
    Task<List<OutboxEvent>> GetPendingEventsAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets outbox events older than the specified date for archival.
    /// </summary>
    /// <param name="olderThan">Date threshold for archival.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of processed events older than the specified date.</returns>
    Task<List<OutboxEvent>> GetEventsForArchivalAsync(DateTime olderThan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing outbox event.
    /// </summary>
    Task UpdateAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes outbox events (used for archival cleanup).
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<OutboxEvent> outboxEvents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an outbox event by its identifier.
    /// </summary>
    Task<OutboxEvent?> GetByIdAsync(OutboxEventId id, CancellationToken cancellationToken = default);
}
