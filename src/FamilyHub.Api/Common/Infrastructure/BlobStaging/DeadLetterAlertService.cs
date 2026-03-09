namespace FamilyHub.Api.Common.Infrastructure.BlobStaging;

public sealed class DeadLetterAlertService(ILogger<DeadLetterAlertService> logger)
{
    public Task AlertAsync(BlobStagingEntry entry)
    {
        logger.LogCritical(
            "DEAD LETTER: Blob staging entry {Id} for module {Module} with storage key {StorageKey} has exhausted all retries. Error: {Error}",
            entry.Id, entry.Module, entry.StorageKey, entry.ErrorMessage);

        // Future: send admin notification (email, Slack, etc.)
        return Task.CompletedTask;
    }
}
