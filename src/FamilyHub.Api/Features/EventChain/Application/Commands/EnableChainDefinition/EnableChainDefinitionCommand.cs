using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.EnableChainDefinition;

public sealed record EnableChainDefinitionCommand(
    ChainDefinitionId Id
) : ICommand<Result<EnableChainDefinitionResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
