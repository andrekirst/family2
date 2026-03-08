using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetFamilyMembersWithRoles;

/// <summary>
/// Query to get family members with roles for the current user's family.
/// </summary>
public sealed record GetFamilyMembersWithRolesQuery : IReadOnlyQuery<List<FamilyMemberDto>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
