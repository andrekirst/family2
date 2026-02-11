namespace FamilyHub.Api.Features.EventChain.Models;

public sealed record ChainExecutionDto(
    Guid Id,
    Guid ChainDefinitionId,
    Guid FamilyId,
    Guid CorrelationId,
    string Status,
    string TriggerEventType,
    string TriggerPayload,
    string Context,
    DateTime StartedAt,
    DateTime? CompletedAt,
    DateTime? FailedAt,
    string? ErrorMessage,
    IReadOnlyList<StepExecutionDto> StepExecutions);

public sealed record StepExecutionDto(
    Guid Id,
    Guid ChainExecutionId,
    string StepAlias,
    string StepName,
    string ActionType,
    string Status,
    string? InputPayload,
    string? OutputPayload,
    string? ErrorMessage,
    int RetryCount,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime? CompensatedAt);
