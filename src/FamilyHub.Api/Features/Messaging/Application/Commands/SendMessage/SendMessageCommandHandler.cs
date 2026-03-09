using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
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
/// Creates a new message in a conversation or family channel. For each attachment, a StoredFile entity
/// is created in the File Management module for full file tracking.
/// When a ConversationId is provided, files are placed in the conversation's dedicated folder.
/// Transaction behavior handles SaveChanges.
/// </summary>
public sealed class SendMessageCommandHandler(
    IMessageRepository messageRepository,
    IStoredFileRepository storedFileRepository,
    IFolderRepository folderRepository,
    IConversationRepository conversationRepository,
    TimeProvider timeProvider)
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
            // Determine target folder: conversation folder or family root folder
            var targetFolderId = await ResolveTargetFolderAsync(command, cancellationToken);

            attachments = [];
            foreach (var a in command.Attachments)
            {
                // Create a StoredFile entity for full file management integration
                var utcNow = timeProvider.GetUtcNow();
                var storedFile = StoredFile.Create(
                    FileName.From(a.FileName),
                    MimeType.From(a.MimeType),
                    FileSize.From(a.FileSize),
                    StorageKey.From(a.StorageKey),
                    Checksum.From(a.Checksum),
                    targetFolderId,
                    command.FamilyId,
                    command.UserId,
                    utcNow);

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
        var message = Message.Create(
            command.FamilyId, command.UserId, command.Content,
            attachments, command.ConversationId);

        await messageRepository.AddAsync(message, cancellationToken);

        return new SendMessageResult(message.Id, message);
    }

    private async Task<FolderId> ResolveTargetFolderAsync(
        SendMessageCommand command, CancellationToken cancellationToken)
    {
        // If a conversation is specified and has a dedicated folder, use it
        if (command.ConversationId.HasValue)
        {
            var conversation = await conversationRepository.GetByIdAsync(command.ConversationId.Value, cancellationToken);
            if (conversation is not null)
            {
                if (conversation.FolderId is not null)
                    return conversation.FolderId.Value;

                // Lazy folder creation: conversation exists but has no folder yet
                // (e.g. "General" conversation created before root folder was available)
                return await CreateConversationFolderAsync(conversation, command, cancellationToken);
            }
        }

        // Fallback to family root folder (no conversation context)
        var rootFolder = await folderRepository.GetRootFolderAsync(command.FamilyId, cancellationToken)
            ?? throw new DomainException("Family root folder not found", DomainErrorCodes.NotFound);

        return rootFolder.Id;
    }

    /// <summary>
    /// Creates the folder hierarchy for a conversation that was saved without a FolderId.
    /// Pattern: root → Messages → {ConversationName}
    /// </summary>
    private async Task<FolderId> CreateConversationFolderAsync(
        Conversation conversation, SendMessageCommand command, CancellationToken cancellationToken)
    {
        var rootFolder = await folderRepository.GetRootFolderAsync(command.FamilyId, cancellationToken)
            ?? throw new DomainException("Family root folder not found", DomainErrorCodes.NotFound);

        // Find or create the "Messages" parent folder
        var children = await folderRepository.GetChildrenAsync(rootFolder.Id, cancellationToken);
        var messagesFolder = children.FirstOrDefault(f => f.Name.Value == "Messages");

        if (messagesFolder is null)
        {
            messagesFolder = Folder.Create(
                FileName.From("Messages"),
                rootFolder.Id,
                $"/{rootFolder.Id.Value}/",
                command.FamilyId,
                command.UserId,
                timeProvider.GetUtcNow());

            await folderRepository.AddAsync(messagesFolder, cancellationToken);
        }

        // Create the conversation-specific subfolder
        var conversationFolder = Folder.Create(
            FileName.From(conversation.Name.Value),
            messagesFolder.Id,
            $"{messagesFolder.MaterializedPath}{messagesFolder.Id.Value}/",
            command.FamilyId,
            command.UserId,
            timeProvider.GetUtcNow());

        await folderRepository.AddAsync(conversationFolder, cancellationToken);

        // Assign folder to conversation so future messages skip lazy creation
        conversation.SetFolderId(conversationFolder.Id);

        return conversationFolder.Id;
    }
}
