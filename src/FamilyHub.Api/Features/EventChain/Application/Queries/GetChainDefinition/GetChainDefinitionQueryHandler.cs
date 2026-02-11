using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Queries.GetChainDefinition;

public sealed class GetChainDefinitionQueryHandler(
    IChainDefinitionRepository repository)
    : IQueryHandler<GetChainDefinitionQuery, ChainDefinition?>
{
    public async ValueTask<ChainDefinition?> Handle(
        GetChainDefinitionQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetByIdWithStepsAsync(query.Id, cancellationToken);
    }
}
