using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Application.Mappers;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetFamilyMembers;

/// <summary>
/// Handler for GetFamilyMembersQuery.
/// Retrieves all members of the current user's family.
/// </summary>
public sealed class GetFamilyMembersQueryHandler(
    IUserRepository userRepository,
    IFamilyRepository familyRepository)
    : IQueryHandler<GetFamilyMembersQuery, List<UserDto>>
{
    public async ValueTask<List<UserDto>> Handle(
        GetFamilyMembersQuery query,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(query.ExternalUserId, cancellationToken);
        if (user?.FamilyId == null)
        {
            return [];
        }

        var family = await familyRepository.GetByIdWithMembersAsync(user.FamilyId.Value, cancellationToken);
        return family is null ? [] : family.Members.Select(UserMapper.ToDto).ToList();
    }
}
