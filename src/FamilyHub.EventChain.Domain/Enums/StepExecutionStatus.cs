namespace FamilyHub.EventChain.Domain.Enums;

public enum StepExecutionStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Skipped,
    Compensating,
    Compensated
}
