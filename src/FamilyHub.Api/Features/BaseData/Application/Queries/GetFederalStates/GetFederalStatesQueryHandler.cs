using FamilyHub.Common.Application;
using FamilyHub.Api.Features.BaseData.Application.Mappers;
using FamilyHub.Api.Features.BaseData.Domain.Repositories;
using FamilyHub.Api.Features.BaseData.Models;

namespace FamilyHub.Api.Features.BaseData.Application.Queries.GetFederalStates;

public sealed class GetFederalStatesQueryHandler(
    IFederalStateRepository repository)
    : IQueryHandler<GetFederalStatesQuery, List<FederalStateDto>>
{
    public async ValueTask<List<FederalStateDto>> Handle(
        GetFederalStatesQuery query,
        CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync(cancellationToken);
        return FederalStateMapper.ToDtoList(entities);
    }
}
