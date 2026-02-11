using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Queries.GetChainExecutions;

public sealed class GetChainExecutionsQueryHandler(
    IChainExecutionRepository repository)
    : IQueryHandler<GetChainExecutionsQuery, IReadOnlyList<ChainExecution>>
{
    public async ValueTask<IReadOnlyList<ChainExecution>> Handle(
        GetChainExecutionsQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetByFamilyIdAsync(query.FamilyId, query.ChainDefinitionId, query.Status, cancellationToken);
    }
}
