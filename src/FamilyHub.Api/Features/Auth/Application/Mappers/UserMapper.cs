using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Models;

namespace FamilyHub.Api.Features.Auth.Application.Mappers;

/// <summary>
/// Maps User aggregate to UserDto for GraphQL responses.
/// </summary>
public static class UserMapper
{
    public static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id.Value,
            Email = user.Email.Value,
            Name = user.Name.Value,
            Username = user.Username,
            FamilyId = user.FamilyId?.Value,
            AvatarId = user.AvatarId?.Value,
            EmailVerified = user.EmailVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
