using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.SendMessage;

/// <summary>
/// Handler for SendMessageCommand.
/// Creates a new message in the family channel. Transaction behavior handles SaveChanges.
/// </summary>
public sealed class SendMessageCommandHandler(
    IMessageRepository messageRepository)
    : ICommandHandler<SendMessageCommand, SendMessageResult>
{
    public async ValueTask<SendMessageResult> Handle(
        SendMessageCommand command,
        CancellationToken cancellationToken)
    {
        // Build attachments from command data (metadata provided by client from upload response)
        List<MessageAttachment>? attachments = null;
        if (command.Attachments is { Count: > 0 })
        {
            attachments = command.Attachments
                .Select(a => MessageAttachment.Create(
                    a.FileId,
                    a.FileName,
                    a.MimeType,
                    a.FileSize))
                .ToList();
        }

        // Create message aggregate (raises MessageSentEvent + attachment events)
        var message = Message.Create(command.FamilyId, command.SenderId, command.Content, attachments);

        await messageRepository.AddAsync(message, cancellationToken);

        return new SendMessageResult(message.Id);
    }
}
