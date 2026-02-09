using FamilyHub.Api.Features.Auth.Application.Mappers;
using FamilyHub.Api.Features.Auth.Application.Queries;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Auth.Application.Handlers;

/// <summary>
/// Handler for GetCurrentUserQuery.
/// Retrieves a user by their external OAuth ID and populates role-based permissions.
/// </summary>
public static class GetCurrentUserQueryHandler
{
    public static async Task<UserDto?> Handle(
        GetCurrentUserQuery query,
        IUserRepository userRepository,
        IFamilyMemberRepository familyMemberRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetByExternalIdAsync(query.ExternalUserId, ct);
        if (user is null) return null;

        var dto = UserMapper.ToDto(user);

        if (user.FamilyId is not null)
        {
            var member = await familyMemberRepository.GetByUserAndFamilyAsync(user.Id, user.FamilyId.Value, ct);
            if (member is not null)
            {
                dto.Permissions = member.Role.GetPermissions();
            }
        }

        return dto;
    }
}
