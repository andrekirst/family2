using FamilyHub.Modules.Auth.Domain.Constants;

namespace FamilyHub.Modules.Auth.Domain.ValueObjects;

/// <summary>
/// Represents the status of a queued background job.
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct QueuedJobStatus
{
    private static readonly string[] ValidStatuses =
    [
        QueuedJobStatusConstants.PendingValue,
        QueuedJobStatusConstants.ProcessingValue,
        QueuedJobStatusConstants.CompletedValue,
        QueuedJobStatusConstants.FailedValue
    ];

    /// <summary>
    /// Job is waiting to be processed.
    /// </summary>
    public static readonly QueuedJobStatus Pending = From(QueuedJobStatusConstants.PendingValue);

    /// <summary>
    /// Job is currently being processed.
    /// </summary>
    public static readonly QueuedJobStatus Processing = From(QueuedJobStatusConstants.ProcessingValue);

    /// <summary>
    /// Job has completed successfully.
    /// </summary>
    public static readonly QueuedJobStatus Completed = From(QueuedJobStatusConstants.CompletedValue);

    /// <summary>
    /// Job has failed during processing.
    /// </summary>
    public static readonly QueuedJobStatus Failed = From(QueuedJobStatusConstants.FailedValue);

    private static Validation Validate(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("Queued job status cannot be empty.")
            : !ValidStatuses.Contains(value.ToLowerInvariant())
                ? Validation.Invalid($"Invalid queued job status. Must be one of: {string.Join(", ", ValidStatuses)}")
                : Validation.Ok;
    }

    private static string NormalizeInput(string input) =>
        input.Trim().ToLowerInvariant();
}
