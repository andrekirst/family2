using FamilyHub.Api.Features.Family.Models;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.Api.Features.Family.Application.Mappers;

/// <summary>
/// Maps Family aggregate to FamilyDto for GraphQL responses.
/// </summary>
public static class FamilyMapper
{
    public static FamilyDto ToDto(FamilyEntity family, int? memberCount = null)
    {
        return new FamilyDto
        {
            Id = family.Id.Value,
            Name = family.Name.Value,
            OwnerId = family.OwnerId.Value,
            CreatedAt = family.CreatedAt,
            MemberCount = memberCount ?? family.Members?.Count ?? 0
        };
    }
}
