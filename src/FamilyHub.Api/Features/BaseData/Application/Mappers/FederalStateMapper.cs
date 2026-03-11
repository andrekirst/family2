using FamilyHub.Api.Features.BaseData.Domain.Entities;
using FamilyHub.Api.Features.BaseData.Models;

namespace FamilyHub.Api.Features.BaseData.Application.Mappers;

public static class FederalStateMapper
{
    public static FederalStateDto ToDto(FederalState entity)
    {
        return new FederalStateDto
        {
            Id = entity.Id.Value,
            Name = entity.Name.Value,
            Iso3166Code = entity.Iso3166Code.Value
        };
    }

    public static List<FederalStateDto> ToDtoList(IEnumerable<FederalState> entities)
    {
        return entities.Select(ToDto).ToList();
    }
}
