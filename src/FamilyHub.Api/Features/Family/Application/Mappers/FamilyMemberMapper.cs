using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Mappers;

/// <summary>
/// Maps FamilyMember entity to FamilyMemberDto for GraphQL responses.
/// </summary>
public static class FamilyMemberMapper
{
    public static FamilyMemberDto ToDto(FamilyMember member)
    {
        return new FamilyMemberDto
        {
            Id = member.Id.Value,
            UserId = member.UserId.Value,
            UserName = member.User?.Name.Value ?? "",
            UserEmail = member.User?.Email.Value ?? "",
            Role = member.Role.Value,
            JoinedAt = member.JoinedAt,
            IsActive = member.IsActive
        };
    }
}
