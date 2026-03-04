using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using FileSize = FamilyHub.Api.Features.FileManagement.Domain.ValueObjects.FileSize;
using MimeType = FamilyHub.Api.Features.FileManagement.Domain.ValueObjects.MimeType;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.SendMessage;

/// <summary>
/// Handler for SendMessageCommand.
/// Creates a new message in the family channel. For each attachment, a StoredFile entity
/// is created in the File Management module for full file tracking.
/// Transaction behavior handles SaveChanges.
/// </summary>
public sealed class SendMessageCommandHandler(
    IMessageRepository messageRepository,
    IStoredFileRepository storedFileRepository,
    IFolderRepository folderRepository)
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
            // Get the family's root folder for file placement
            var folder = await folderRepository.GetRootFolderAsync(command.FamilyId, cancellationToken)
                ?? throw new DomainException("Family root folder not found", DomainErrorCodes.NotFound);

            attachments = [];
            foreach (var a in command.Attachments)
            {
                // Create a StoredFile entity for full file management integration
                var storedFile = StoredFile.Create(
                    FileName.From(a.FileName),
                    MimeType.From(a.MimeType),
                    FileSize.From(a.FileSize),
                    StorageKey.From(a.StorageKey),
                    Checksum.From(a.Checksum),
                    folder.Id,
                    command.FamilyId,
                    command.SenderId);

                await storedFileRepository.AddAsync(storedFile, cancellationToken);

                attachments.Add(MessageAttachment.Create(
                    storedFile.Id,
                    a.FileName,
                    a.MimeType,
                    a.FileSize,
                    a.StorageKey));
            }
        }

        // Create message aggregate (raises MessageSentEvent + attachment events)
        var message = Message.Create(command.FamilyId, command.SenderId, command.Content, attachments);

        await messageRepository.AddAsync(message, cancellationToken);

        return new SendMessageResult(message.Id);
    }
}
