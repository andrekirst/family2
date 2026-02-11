using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Queries.GetChainDefinitions;

public sealed class GetChainDefinitionsQueryHandler(
    IChainDefinitionRepository repository)
    : IQueryHandler<GetChainDefinitionsQuery, IReadOnlyList<ChainDefinition>>
{
    public async ValueTask<IReadOnlyList<ChainDefinition>> Handle(
        GetChainDefinitionsQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetByFamilyIdAsync(query.FamilyId, query.IsEnabled, cancellationToken);
    }
}
