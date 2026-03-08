using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Application.Mappers;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Models;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.SendMessage;

[ExtendObjectType(typeof(MessagingMutation))]
public class MutationType
{
    /// <summary>
    /// Send a message in the family channel.
    /// Publishes to the subscription topic for real-time delivery.
    /// </summary>
    [Authorize]
    public async Task<MessageDto> SendMessage(
        SendMessageRequest input,
        [Service] ICommandBus commandBus,
        [Service] ICurrentUserContext currentUserContext,
        [Service] IUserRepository userRepository,
        [Service] ITopicEventSender topicEventSender,
        CancellationToken cancellationToken)
    {
        var content = input.Content?.Trim() ?? string.Empty;
        var attachments = input.Attachments?
            .Select(a => new AttachmentData(
                a.StorageKey,
                a.FileName,
                a.MimeType,
                a.FileSize,
                a.Checksum))
            .ToList();

        var conversationId = input.ConversationId.HasValue
            ? ConversationId.From(input.ConversationId.Value)
            : (ConversationId?)null;

        var command = new SendMessageCommand(
            MessageContent.From(content),
            attachments,
            conversationId);

        var result = await commandBus.SendAsync(command, cancellationToken);

        // Use entity from result for DTO mapping; fetch supplemental sender data
        var message = result.SentMessage;
        var userInfo = await currentUserContext.GetCurrentUserAsync();
        var user = await userRepository.GetByIdAsync(userInfo.UserId, cancellationToken);
        var messageDto = MessageMapper.ToDto(message, user!.Name.Value, user.AvatarId?.Value);

        // Publish to subscription topic for real-time delivery
        if (userInfo.FamilyId is not null)
        {
            await topicEventSender.SendAsync(
                $"MessageSent_{userInfo.FamilyId.Value.Value}",
                messageDto,
                cancellationToken);
        }

        return messageDto;
    }
}
