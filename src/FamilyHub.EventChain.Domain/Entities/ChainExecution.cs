using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.Enums;
using FamilyHub.EventChain.Domain.Events;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.EventChain.Domain.Entities;

public sealed class ChainExecution : AggregateRoot<ChainExecutionId>
{
    private readonly List<StepExecution> _stepExecutions = [];

#pragma warning disable CS8618
    private ChainExecution() { }
#pragma warning restore CS8618

    public static ChainExecution Start(
        ChainDefinitionId chainDefinitionId,
        FamilyId familyId,
        string triggerEventType,
        Guid triggerEventId,
        string triggerPayload)
    {
        var execution = new ChainExecution
        {
            Id = ChainExecutionId.New(),
            ChainDefinitionId = chainDefinitionId,
            FamilyId = familyId,
            CorrelationId = Guid.NewGuid(),
            Status = ChainExecutionStatus.Pending,
            TriggerEventType = triggerEventType,
            TriggerEventId = triggerEventId,
            TriggerPayload = triggerPayload,
            Context = "{}",
            CurrentStepIndex = 0,
            StartedAt = DateTime.UtcNow
        };

        execution.RaiseDomainEvent(new ChainExecutionStartedEvent(
            execution.Id,
            execution.ChainDefinitionId,
            execution.FamilyId,
            execution.CorrelationId,
            execution.TriggerEventType));

        return execution;
    }

    public ChainDefinitionId ChainDefinitionId { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public Guid CorrelationId { get; private set; }
    public ChainExecutionStatus Status { get; private set; }
    public string TriggerEventType { get; private set; }
    public Guid TriggerEventId { get; private set; }
    public string TriggerPayload { get; private set; }
    public string Context { get; private set; }
    public int CurrentStepIndex { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    public ChainDefinition? ChainDefinition { get; private set; }
    public IReadOnlyList<StepExecution> StepExecutions => _stepExecutions.AsReadOnly();

    public void MarkRunning()
    {
        Status = ChainExecutionStatus.Running;
    }

    public void AdvanceStep()
    {
        CurrentStepIndex++;
    }

    public void UpdateContext(string context)
    {
        Context = context;
    }

    public void MarkCompleted()
    {
        Status = ChainExecutionStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ChainExecutionCompletedEvent(
            Id, ChainDefinitionId, FamilyId, CorrelationId,
            ChainExecutionStatus.Completed,
            _stepExecutions.Count(s => s.Status == StepExecutionStatus.Completed),
            _stepExecutions.Count));
    }

    public void MarkPartiallyCompleted()
    {
        Status = ChainExecutionStatus.PartiallyCompleted;
        CompletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ChainExecutionCompletedEvent(
            Id, ChainDefinitionId, FamilyId, CorrelationId,
            ChainExecutionStatus.PartiallyCompleted,
            _stepExecutions.Count(s => s.Status == StepExecutionStatus.Completed),
            _stepExecutions.Count));
    }

    public void MarkFailed(string errorMessage)
    {
        Status = ChainExecutionStatus.Failed;
        FailedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;

        var failedStep = _stepExecutions.FirstOrDefault(s => s.Status == StepExecutionStatus.Failed);

        RaiseDomainEvent(new ChainExecutionFailedEvent(
            Id, ChainDefinitionId, FamilyId, CorrelationId,
            failedStep?.StepAlias ?? "unknown",
            errorMessage));
    }

    public void MarkCompensating()
    {
        Status = ChainExecutionStatus.Compensating;
    }

    public void MarkCompensated()
    {
        Status = ChainExecutionStatus.Compensated;
        CompletedAt = DateTime.UtcNow;
    }

    public void AddStepExecution(StepExecution stepExecution)
    {
        _stepExecutions.Add(stepExecution);
    }
}
