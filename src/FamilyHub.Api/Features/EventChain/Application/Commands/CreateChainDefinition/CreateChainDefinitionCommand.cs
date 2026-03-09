using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.CreateChainDefinition;

public sealed record CreateChainDefinitionCommand(
    ChainName Name,
    string? Description,
    string TriggerEventType,
    IReadOnlyList<CreateStepCommand> Steps,
    bool IsEnabled = true
) : ICommand<Result<CreateChainDefinitionResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}

public sealed record CreateStepCommand(
    StepAlias Alias,
    string Name,
    string ActionType,
    ActionVersion ActionVersion,
    string InputMappings,
    string? Condition,
    int Order);
