using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetFamilyMembersWithRoles;

/// <summary>
/// Handler for GetFamilyMembersWithRolesQuery.
/// Retrieves family members with roles for the current user's family.
/// </summary>
public sealed class GetFamilyMembersWithRolesQueryHandler(
    IFamilyMemberRepository memberRepository)
    : IQueryHandler<GetFamilyMembersWithRolesQuery, List<FamilyMemberDto>>
{
    public async ValueTask<List<FamilyMemberDto>> Handle(
        GetFamilyMembersWithRolesQuery query,
        CancellationToken cancellationToken)
    {
        var members = await memberRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);
        return members.Select(FamilyMemberMapper.ToDto).ToList();
    }
}
