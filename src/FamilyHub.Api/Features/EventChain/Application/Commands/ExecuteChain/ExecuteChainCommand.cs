using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.ExecuteChain;

public sealed record ExecuteChainCommand(
    ChainDefinitionId ChainDefinitionId,
    FamilyId FamilyId,
    string TriggerPayload
) : ICommand<ExecuteChainResult>;

public sealed record ExecuteChainResult(ChainExecutionId ChainExecutionId);
