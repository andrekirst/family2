using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.DeleteChainDefinition;

public sealed record DeleteChainDefinitionCommand(
    ChainDefinitionId Id
) : ICommand<DeleteChainDefinitionResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}

public sealed record DeleteChainDefinitionResult(bool Success);
