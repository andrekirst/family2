using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Application.Mappers;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Models;

namespace FamilyHub.Api.Features.Messaging.Application.Queries.GetConversationMessages;

/// <summary>
/// Handler for GetConversationMessagesQuery.
/// Fetches paginated messages for a conversation and resolves sender info.
/// </summary>
public sealed class GetConversationMessagesQueryHandler(
    IMessageRepository messageRepository,
    IUserRepository userRepository)
    : IQueryHandler<GetConversationMessagesQuery, List<MessageDto>>
{
    public async ValueTask<List<MessageDto>> Handle(
        GetConversationMessagesQuery query,
        CancellationToken cancellationToken)
    {
        var messages = await messageRepository.GetByConversationAsync(
            query.ConversationId, query.Limit, query.Before, cancellationToken);

        var dtos = new List<MessageDto>(messages.Count);
        foreach (var message in messages)
        {
            var sender = await userRepository.GetByIdAsync(message.SenderId, cancellationToken);
            dtos.Add(MessageMapper.ToDto(
                message,
                sender?.Name.Value,
                sender?.AvatarId?.Value));
        }

        return dtos;
    }
}
