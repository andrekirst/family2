using FamilyHub.Common.Application;
using FamilyHub.Api.Features.BaseData.Application.Mappers;
using FamilyHub.Api.Features.BaseData.Domain.Repositories;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Models;

namespace FamilyHub.Api.Features.BaseData.Application.Queries.GetFederalStateByIso3166;

public sealed class GetFederalStateByIso3166QueryHandler(
    IFederalStateRepository repository)
    : IQueryHandler<GetFederalStateByIso3166Query, FederalStateDto?>
{
    public async ValueTask<FederalStateDto?> Handle(
        GetFederalStateByIso3166Query query,
        CancellationToken cancellationToken)
    {
        if (!Iso3166Code.TryFrom(query.Code, out var code))
            return null;

        var entity = await repository.GetByIso3166CodeAsync(code, cancellationToken);
        return entity is null ? null : FederalStateMapper.ToDto(entity);
    }
}
