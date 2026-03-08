using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.ExecuteChain;

public sealed record ExecuteChainCommand(
    ChainDefinitionId ChainDefinitionId,
    string TriggerPayload
) : ICommand<ExecuteChainResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}

public sealed record ExecuteChainResult(ChainExecutionId ChainExecutionId);
