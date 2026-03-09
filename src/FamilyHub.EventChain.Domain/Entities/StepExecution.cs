using FamilyHub.EventChain.Domain.Enums;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.EventChain.Domain.Entities;

public sealed class StepExecution
{
    public Guid Id { get; private set; }
    public ChainExecutionId ChainExecutionId { get; private set; }
    public string StepAlias { get; private set; }
    public string StepName { get; private set; }
    public string ActionType { get; private set; }
    public StepExecutionStatus Status { get; private set; }
    public string? InputPayload { get; private set; }
    public string? OutputPayload { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public int StepOrder { get; private set; }
    public DateTime? ScheduledAt { get; private set; }
    public DateTime? PickedUpAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CompensatedAt { get; private set; }

#pragma warning disable CS8618
    private StepExecution() { }
#pragma warning restore CS8618

    public static StepExecution Create(
        ChainExecutionId chainExecutionId,
        string stepAlias,
        string stepName,
        string actionType,
        int stepOrder,
        int maxRetries = 3,
        DateTime? scheduledAt = null)
    {
        return new StepExecution
        {
            Id = Guid.NewGuid(),
            ChainExecutionId = chainExecutionId,
            StepAlias = stepAlias,
            StepName = stepName,
            ActionType = actionType,
            Status = StepExecutionStatus.Pending,
            RetryCount = 0,
            MaxRetries = maxRetries,
            StepOrder = stepOrder,
            ScheduledAt = scheduledAt
        };
    }

    public void MarkRunning(DateTimeOffset utcNow)
    {
        Status = StepExecutionStatus.Running;
        StartedAt = utcNow.UtcDateTime;
    }

    public void MarkCompleted(string? outputPayload, DateTimeOffset utcNow)
    {
        Status = StepExecutionStatus.Completed;
        OutputPayload = outputPayload;
        CompletedAt = utcNow.UtcDateTime;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = StepExecutionStatus.Failed;
        ErrorMessage = errorMessage;
    }

    public void MarkSkipped(DateTimeOffset utcNow)
    {
        Status = StepExecutionStatus.Skipped;
        CompletedAt = utcNow.UtcDateTime;
    }

    public void MarkCompensating()
    {
        Status = StepExecutionStatus.Compensating;
    }

    public void MarkCompensated(DateTimeOffset utcNow)
    {
        Status = StepExecutionStatus.Compensated;
        CompensatedAt = utcNow.UtcDateTime;
    }

    public void SetInputPayload(string inputPayload)
    {
        InputPayload = inputPayload;
    }

    public void IncrementRetry()
    {
        RetryCount++;
    }

    public bool CanRetry => RetryCount < MaxRetries;

    public void PickUp(DateTimeOffset utcNow)
    {
        PickedUpAt = utcNow.UtcDateTime;
    }
}
