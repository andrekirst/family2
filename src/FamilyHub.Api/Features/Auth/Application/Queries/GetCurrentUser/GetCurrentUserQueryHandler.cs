using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Application.Mappers;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Auth.Application.Queries.GetCurrentUser;

/// <summary>
/// Handler for GetCurrentUserQuery.
/// Retrieves a user by their external OAuth ID and populates role-based permissions.
/// </summary>
public sealed class GetCurrentUserQueryHandler(
    IUserRepository userRepository,
    IFamilyMemberRepository familyMemberRepository)
    : IQueryHandler<GetCurrentUserQuery, UserDto?>
{
    public async ValueTask<UserDto?> Handle(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(query.ExternalUserId, cancellationToken);
        if (user is null) return null;

        var dto = UserMapper.ToDto(user);

        if (user.FamilyId is not null)
        {
            var member = await familyMemberRepository.GetByUserAndFamilyAsync(user.Id, user.FamilyId.Value, cancellationToken);
            if (member is not null)
            {
                dto.Permissions = member.Role.GetPermissions();
            }
        }

        return dto;
    }
}
