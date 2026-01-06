namespace FamilyHub.Modules.Auth.Domain.Constants;

/// <summary>
/// Constants for queued job status values.
/// </summary>
public static class QueuedJobStatusConstants
{
    public const string PendingValue = "pending";
    public const string ProcessingValue = "processing";
    public const string CompletedValue = "completed";
    public const string FailedValue = "failed";
}
