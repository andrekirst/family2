using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Models;

namespace FamilyHub.Api.Features.Messaging.Application.Mappers;

/// <summary>
/// Maps Message aggregate to MessageDto for GraphQL responses.
/// Sender info (name, avatar) is resolved at query time, not stored on the aggregate.
/// </summary>
public static class MessageMapper
{
    public static MessageDto ToDto(Message message, string? senderName = null, Guid? senderAvatarId = null)
    {
        return new MessageDto
        {
            Id = message.Id.Value,
            FamilyId = message.FamilyId.Value,
            SenderId = message.SenderId.Value,
            SenderName = senderName ?? string.Empty,
            SenderAvatarId = senderAvatarId,
            Content = message.Content.Value,
            SentAt = message.SentAt
        };
    }
}
