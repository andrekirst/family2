using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Mappers;

/// <summary>
/// Maps FamilyInvitation entity to InvitationDto for GraphQL responses.
/// </summary>
public static class InvitationMapper
{
    public static InvitationDto ToDto(FamilyInvitation invitation)
    {
        return new InvitationDto
        {
            Id = invitation.Id.Value,
            FamilyId = invitation.FamilyId.Value,
            FamilyName = invitation.Family.Name.Value,
            InvitedByName = invitation.InvitedByUser.Name.Value,
            InviteeEmail = invitation.InviteeEmail.Value,
            Role = invitation.Role.Value,
            Status = invitation.Status.Value,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt
        };
    }
}
