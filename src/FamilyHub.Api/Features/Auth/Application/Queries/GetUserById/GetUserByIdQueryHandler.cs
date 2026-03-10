using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Application.Mappers;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Models;

namespace FamilyHub.Api.Features.Auth.Application.Queries.GetUserById;

/// <summary>
/// Handler for GetUserByIdQuery.
/// Retrieves a user by their unique identifier, scoped to the caller's family.
/// </summary>
public sealed class GetUserByIdQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetUserByIdQuery, UserDto?>
{
    public async ValueTask<UserDto?> Handle(
        GetUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(query.TargetUserId, cancellationToken);
        if (user is null || user.FamilyId != query.FamilyId)
        {
            return null;
        }

        return UserMapper.ToDto(user);
    }
}
