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
        // Create message aggregate (raises MessageSentEvent)
        var message = Message.Create(command.FamilyId, command.SenderId, command.Content);

        await messageRepository.AddAsync(message, cancellationToken);

        return new SendMessageResult(message.Id);
    }
}
