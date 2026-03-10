namespace FamilyHub.Api.Common.Infrastructure.BlobStaging;

public sealed class BlobStagingOptions
{
    public const string SectionName = "BlobStaging";
    public int MaxRetries { get; set; } = 5;
    public int BatchSize { get; set; } = 50;
    public string PromotionCron { get; set; } = "*/5 * * * *"; // every 5 minutes
}
