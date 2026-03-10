using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetMyFamily;

/// <summary>
/// Handler for GetMyFamilyQuery.
/// Retrieves the current user's family.
/// </summary>
public sealed class GetMyFamilyQueryHandler(
    IFamilyRepository familyRepository)
    : IQueryHandler<GetMyFamilyQuery, FamilyDto?>
{
    public async ValueTask<FamilyDto?> Handle(
        GetMyFamilyQuery query,
        CancellationToken cancellationToken)
    {
        var family = await familyRepository.GetByIdWithMembersAsync(query.FamilyId, cancellationToken);
        return family is not null ? FamilyMapper.ToDto(family) : null;
    }
}
