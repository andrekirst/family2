namespace FamilyHub.Api.Common.Infrastructure.BlobStaging;

public sealed class BlobStagingEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Module { get; init; }
    public required string StorageKey { get; init; }
    public string Status { get; set; } = BlobStagingStatus.Pending;
    public int RetryCount { get; set; }
    public int MaxRetries { get; init; } = 5;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PromotedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Metadata { get; set; }
}

public static class BlobStagingStatus
{
    public const string Pending = "pending";
    public const string Promoted = "promoted";
    public const string DeadLetter = "dead_letter";
}
