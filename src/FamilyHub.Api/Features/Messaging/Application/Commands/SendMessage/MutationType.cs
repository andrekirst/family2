using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Application.Mappers;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Models;
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
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IMessageRepository messageRepository,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        [Service] ITopicEventSender topicEventSender,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        if (user.FamilyId is null)
        {
            throw new InvalidOperationException("You must be part of a family to send messages");
        }

        var content = input.Content?.Trim() ?? string.Empty;
        var attachments = input.Attachments?
            .Select(a => new AttachmentData(
                a.StorageKey,
                a.FileName,
                a.MimeType,
                a.FileSize,
                a.Checksum))
            .ToList();

        if (content.Length == 0 && (attachments is null || attachments.Count == 0))
        {
            throw new InvalidOperationException("Message must have content or at least one attachment");
        }

        var command = new SendMessageCommand(
            user.FamilyId.Value,
            user.Id,
            MessageContent.From(content),
            attachments);

        var result = await commandBus.SendAsync(command, cancellationToken);

        // Fetch persisted message for DTO
        var message = await messageRepository.GetByIdAsync(result.MessageId, cancellationToken);
        var messageDto = MessageMapper.ToDto(message!, user.Name.Value, user.AvatarId?.Value);

        // Publish to subscription topic for real-time delivery
        await topicEventSender.SendAsync(
            $"MessageSent_{user.FamilyId.Value.Value}",
            messageDto,
            cancellationToken);

        return messageDto;
    }
}
