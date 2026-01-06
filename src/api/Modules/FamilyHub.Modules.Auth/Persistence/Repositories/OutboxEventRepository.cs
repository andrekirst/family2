using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the OutboxEvent repository.
/// </summary>
public sealed class OutboxEventRepository(AuthDbContext context) : IOutboxEventRepository
{
    private readonly AuthDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public async Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outboxEvent);

        await _context.OutboxEvents.AddAsync(outboxEvent, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<OutboxEvent> outboxEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outboxEvents);

        await _context.OutboxEvents.AddRangeAsync(outboxEvents, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<OutboxEvent>> GetPendingEventsAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxEvents
            .Where(e => e.Status == OutboxEventStatus.Pending)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<OutboxEvent>> GetEventsForArchivalAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxEvents
            .Where(e => e.Status == OutboxEventStatus.Processed && e.CreatedAt < olderThan)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outboxEvent);

        _context.OutboxEvents.Update(outboxEvent);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteRangeAsync(IEnumerable<OutboxEvent> outboxEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outboxEvents);

        _context.OutboxEvents.RemoveRange(outboxEvents);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<OutboxEvent?> GetByIdAsync(OutboxEventId id, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxEvents
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}
