using FamilyHub.Api.Features.EventChain.Application.Queries;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Handlers;

public static class GetChainExecutionsQueryHandler
{
    public static async Task<IReadOnlyList<ChainExecution>> Handle(
        GetChainExecutionsQuery query,
        IChainExecutionRepository repository,
        CancellationToken ct)
    {
        return await repository.GetByFamilyIdAsync(query.FamilyId, query.ChainDefinitionId, query.Status, ct);
    }
}
