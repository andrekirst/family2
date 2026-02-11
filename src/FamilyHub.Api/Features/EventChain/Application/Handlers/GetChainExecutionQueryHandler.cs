using FamilyHub.Api.Features.EventChain.Application.Queries;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Handlers;

public static class GetChainExecutionQueryHandler
{
    public static async Task<ChainExecution?> Handle(
        GetChainExecutionQuery query,
        IChainExecutionRepository repository,
        CancellationToken ct)
    {
        return await repository.GetByIdWithStepsAsync(query.Id, ct);
    }
}
