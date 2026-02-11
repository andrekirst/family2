using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.EventChain.Domain.Entities;

public sealed class ChainScheduledJob
{
    public Guid Id { get; private set; }
    public Guid StepExecutionId { get; private set; }
    public ChainExecutionId ChainExecutionId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? PickedUpAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public StepExecution? StepExecution { get; private set; }
    public ChainExecution? ChainExecution { get; private set; }

#pragma warning disable CS8618
    private ChainScheduledJob() { }
#pragma warning restore CS8618

    public static ChainScheduledJob Create(
        Guid stepExecutionId,
        ChainExecutionId chainExecutionId,
        DateTime scheduledAt)
    {
        return new ChainScheduledJob
        {
            Id = Guid.NewGuid(),
            StepExecutionId = stepExecutionId,
            ChainExecutionId = chainExecutionId,
            ScheduledAt = scheduledAt,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void PickUp()
    {
        PickedUpAt = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string errorMessage)
    {
        FailedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
        RetryCount++;
    }

    public void Reset()
    {
        PickedUpAt = null;
    }
}
