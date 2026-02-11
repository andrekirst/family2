namespace FamilyHub.EventChain.Domain.Enums;

public enum ChainExecutionStatus
{
    Pending,
    Running,
    Completed,
    PartiallyCompleted,
    Failed,
    Compensating,
    Compensated
}
