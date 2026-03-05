using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Models;

namespace FamilyHub.Api.Features.Messaging.Application.Mappers;

/// <summary>
/// Maps Conversation aggregate to ConversationDto.
/// </summary>
public static class ConversationMapper
{
    public static ConversationDto ToDto(Conversation conversation)
    {
        return new ConversationDto
        {
            Id = conversation.Id.Value,
            Name = conversation.Name.Value,
            Type = conversation.Type.ToString(),
            FamilyId = conversation.FamilyId.Value,
            CreatedBy = conversation.CreatedBy.Value,
            FolderId = conversation.FolderId?.Value,
            CreatedAt = conversation.CreatedAt,
            Members = conversation.Members
                .Select(m => new ConversationMemberDto
                {
                    Id = m.Id.Value,
                    UserId = m.UserId.Value,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt,
                    LeftAt = m.LeftAt
                })
                .ToList()
        };
    }
}
