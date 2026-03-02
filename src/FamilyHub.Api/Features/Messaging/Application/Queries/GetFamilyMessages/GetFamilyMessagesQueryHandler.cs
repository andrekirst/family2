using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Application.Mappers;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Models;

namespace FamilyHub.Api.Features.Messaging.Application.Queries.GetFamilyMessages;

/// <summary>
/// Handler for GetFamilyMessagesQuery.
/// Fetches paginated messages and resolves sender names at query time.
/// </summary>
public sealed class GetFamilyMessagesQueryHandler(
    IMessageRepository messageRepository,
    IUserRepository userRepository)
    : IQueryHandler<GetFamilyMessagesQuery, List<MessageDto>>
{
    public async ValueTask<List<MessageDto>> Handle(
        GetFamilyMessagesQuery query,
        CancellationToken cancellationToken)
    {
        var messages = await messageRepository.GetByFamilyAsync(
            query.FamilyId, query.Limit, query.Before, cancellationToken);

        // Resolve sender info for each message
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
