using FamilyHub.Api.Features.EventChain.Application.Queries;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Handlers;

public static class GetChainDefinitionsQueryHandler
{
    public static async Task<IReadOnlyList<ChainDefinition>> Handle(
        GetChainDefinitionsQuery query,
        IChainDefinitionRepository repository,
        CancellationToken ct)
    {
        return await repository.GetByFamilyIdAsync(query.FamilyId, query.IsEnabled, ct);
    }
}
