
using FamilyHub.Modules.Family.Domain;
using FamilyHub.Modules.Family.Domain.Enums;
using FamilyHub.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Family.Persistence.Repositories;

internal sealed class EmailOutboxRepository(FamilyDbContext context) : IEmailOutboxRepository
{
    private readonly FamilyDbContext _context = context;

    public async Task<EmailOutbox?> GetByIdAsync(
        EmailOutboxId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.EmailOutbox
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<EmailOutbox>> GetPendingEmailsAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.EmailOutbox
            .Where(e => e.Status == EmailStatus.Pending)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmailOutbox>> GetFailedEmailsForRetryAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.EmailOutbox
            .Where(e => e.Status == EmailStatus.Failed)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        EmailOutbox emailOutbox,
        CancellationToken cancellationToken = default)
    {
        await _context.EmailOutbox.AddAsync(emailOutbox, cancellationToken);
    }

    public Task UpdateAsync(
        EmailOutbox emailOutbox,
        CancellationToken cancellationToken = default)
    {
        _context.EmailOutbox.Update(emailOutbox);
        return Task.CompletedTask;
    }
}
