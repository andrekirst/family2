using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.DisableChainDefinition;

public sealed record DisableChainDefinitionCommand(
    ChainDefinitionId Id
) : ICommand<DisableChainDefinitionResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
