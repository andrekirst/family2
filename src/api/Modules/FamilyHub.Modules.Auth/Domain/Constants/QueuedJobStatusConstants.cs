namespace FamilyHub.Modules.Auth.Domain.Constants;

/// <summary>
/// Constants for queued job status values.
/// </summary>
public static class QueuedJobStatusConstants
{
    /// <summary>
    /// Status value for jobs waiting to be processed.
    /// </summary>
    public const string PendingValue = "pending";

    /// <summary>
    /// Status value for jobs currently being processed.
    /// </summary>
    public const string ProcessingValue = "processing";

    /// <summary>
    /// Status value for jobs that completed successfully.
    /// </summary>
    public const string CompletedValue = "completed";

    /// <summary>
    /// Status value for jobs that failed during processing.
    /// </summary>
    public const string FailedValue = "failed";
}
