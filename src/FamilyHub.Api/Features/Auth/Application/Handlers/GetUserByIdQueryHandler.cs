using FamilyHub.Api.Features.Auth.Application.Mappers;
using FamilyHub.Api.Features.Auth.Application.Queries;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Models;

namespace FamilyHub.Api.Features.Auth.Application.Handlers;

/// <summary>
/// Handler for GetUserByIdQuery.
/// Retrieves a user by their unique identifier.
/// </summary>
public static class GetUserByIdQueryHandler
{
    public static async Task<UserDto?> Handle(
        GetUserByIdQuery query,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(query.UserId, ct);
        return user is not null ? UserMapper.ToDto(user) : null;
    }
}
