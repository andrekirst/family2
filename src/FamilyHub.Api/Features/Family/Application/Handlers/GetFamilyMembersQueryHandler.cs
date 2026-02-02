using FamilyHub.Api.Features.Auth.Application.Mappers;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Family.Application.Queries;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Handlers;

/// <summary>
/// Handler for GetFamilyMembersQuery.
/// Retrieves all members of the current user's family.
/// </summary>
public static class GetFamilyMembersQueryHandler
{
    public static async Task<List<UserDto>> Handle(
        GetFamilyMembersQuery query,
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetByExternalIdAsync(query.ExternalUserId, ct);
        if (user?.FamilyId == null)
        {
            return [];
        }

        var family = await familyRepository.GetByIdWithMembersAsync(user.FamilyId.Value, ct);
        if (family is null)
        {
            return [];
        }

        return family.Members.Select(UserMapper.ToDto).ToList();
    }
}
