namespace FamilyHub.Api.Common.Infrastructure.BlobStaging;

public interface IBlobStagingRepository
{
    Task AddAsync(BlobStagingEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BlobStagingEntry>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
    Task UpdateAsync(BlobStagingEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BlobStagingEntry>> GetDeadLettersAsync(CancellationToken cancellationToken = default);
}
