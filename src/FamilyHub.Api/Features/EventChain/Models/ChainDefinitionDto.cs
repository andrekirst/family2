namespace FamilyHub.Api.Features.EventChain.Models;

public sealed record ChainDefinitionDto(
    Guid Id,
    Guid FamilyId,
    string Name,
    string? Description,
    bool IsEnabled,
    bool IsTemplate,
    string? TemplateName,
    TriggerDefinitionDto Trigger,
    IReadOnlyList<StepDefinitionDto> Steps,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int Version,
    int ExecutionCount,
    DateTime? LastExecutedAt);

public sealed record TriggerDefinitionDto(
    string EventType,
    string Module,
    string Description,
    string OutputSchema);

public sealed record StepDefinitionDto(
    string Alias,
    string Name,
    string ActionType,
    string ActionVersion,
    string Module,
    string InputMappings,
    string? Condition,
    bool IsCompensatable,
    string? CompensationActionType,
    int Order);
