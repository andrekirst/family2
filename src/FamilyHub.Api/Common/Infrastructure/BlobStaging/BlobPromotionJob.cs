using Microsoft.Extensions.Options;

namespace FamilyHub.Api.Common.Infrastructure.BlobStaging;

public sealed class BlobPromotionJob(
    IBlobStagingRepository repository,
    IOptions<BlobStagingOptions> options,
    DeadLetterAlertService deadLetterAlertService,
    ILogger<BlobPromotionJob> logger)
{
    public async Task ExecuteAsync()
    {
        var pending = await repository.GetPendingAsync(options.Value.BatchSize);

        foreach (var entry in pending)
        {
            try
            {
                // Mark as promoted (blob is already stored, just confirming it's valid)
                entry.Status = BlobStagingStatus.Promoted;
                entry.PromotedAt = DateTimeOffset.UtcNow;
                await repository.UpdateAsync(entry);

                logger.LogInformation("Promoted blob staging entry {Id} for module {Module}", entry.Id, entry.Module);
            }
            catch (Exception ex)
            {
                entry.RetryCount++;
                entry.ErrorMessage = ex.Message;

                if (entry.RetryCount >= entry.MaxRetries)
                {
                    entry.Status = BlobStagingStatus.DeadLetter;
                    logger.LogError(ex, "Blob staging entry {Id} moved to dead letter after {Retries} retries", entry.Id, entry.RetryCount);
                    await deadLetterAlertService.AlertAsync(entry);
                }

                await repository.UpdateAsync(entry);
            }
        }
    }
}
