using FamilyHub.Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Common.Infrastructure.BlobStaging;

public sealed class BlobStagingRepository(AppDbContext dbContext) : IBlobStagingRepository
{
    public async Task AddAsync(BlobStagingEntry entry, CancellationToken cancellationToken = default)
    {
        dbContext.BlobStagingEntries.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BlobStagingEntry>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.BlobStagingEntries
            .Where(e => e.Status == BlobStagingStatus.Pending)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(BlobStagingEntry entry, CancellationToken cancellationToken = default)
    {
        dbContext.BlobStagingEntries.Update(entry);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BlobStagingEntry>> GetDeadLettersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.BlobStagingEntries
            .Where(e => e.Status == BlobStagingStatus.DeadLetter)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
