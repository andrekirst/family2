using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Queries.GetChainDefinitions;

public sealed record GetChainDefinitionsQuery(
    bool? IsEnabled = null
) : IReadOnlyQuery<IReadOnlyList<ChainDefinition>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
