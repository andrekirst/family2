using FamilyHub.Api.Features.EventChain.Application.Queries;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Handlers;

public static class GetChainDefinitionQueryHandler
{
    public static async Task<ChainDefinition?> Handle(
        GetChainDefinitionQuery query,
        IChainDefinitionRepository repository,
        CancellationToken ct)
    {
        return await repository.GetByIdWithStepsAsync(query.Id, ct);
    }
}
