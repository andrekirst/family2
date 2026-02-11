using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Queries.GetChainExecution;

public sealed class GetChainExecutionQueryHandler(
    IChainExecutionRepository repository)
    : IQueryHandler<GetChainExecutionQuery, ChainExecution?>
{
    public async ValueTask<ChainExecution?> Handle(
        GetChainExecutionQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetByIdWithStepsAsync(query.Id, cancellationToken);
    }
}
