using FamilyHub.Api.Features.Auth.Application.Mappers;
using FamilyHub.Api.Features.Auth.Application.Queries;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Models;

namespace FamilyHub.Api.Features.Auth.Application.Handlers;

/// <summary>
/// Handler for GetCurrentUserQuery.
/// Retrieves a user by their external OAuth ID.
/// </summary>
public static class GetCurrentUserQueryHandler
{
    public static async Task<UserDto?> Handle(
        GetCurrentUserQuery query,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetByExternalIdAsync(query.ExternalUserId, ct);
        return user is not null ? UserMapper.ToDto(user) : null;
    }
}
