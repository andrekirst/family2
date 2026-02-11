using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands;

public sealed record CreateChainDefinitionCommand(
    ChainName Name,
    string? Description,
    FamilyId FamilyId,
    UserId CreatedByUserId,
    string TriggerEventType,
    IReadOnlyList<CreateStepCommand> Steps,
    bool IsEnabled = true
) : ICommand<CreateChainDefinitionResult>;

public sealed record CreateStepCommand(
    StepAlias Alias,
    string Name,
    string ActionType,
    ActionVersion ActionVersion,
    string InputMappings,
    string? Condition,
    int Order);

public sealed record CreateChainDefinitionResult(ChainDefinitionId ChainDefinitionId);
