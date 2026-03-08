using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Family.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;

namespace FamilyHub.Api.Features.Messaging.Application.EventHandlers;

/// <summary>
/// Listens to FamilyCreatedEvent to auto-create a "General" conversation
/// with a dedicated /Messages/General/ folder for attachments.
/// Runs outside the pipeline — calls SaveChangesAsync explicitly.
/// </summary>
public sealed class FamilyCreatedConversationHandler(
    IConversationRepository conversationRepository,
    IFolderRepository folderRepository,
    IUnitOfWork unitOfWork,
    ILogger<FamilyCreatedConversationHandler> logger)
    : IDomainEventHandler<FamilyCreatedEvent>
{
    public async ValueTask Handle(FamilyCreatedEvent @event, CancellationToken cancellationToken)
    {
        // Idempotent: skip if General conversation already exists
        var existing = await conversationRepository.GetFamilyConversationAsync(@event.FamilyId, cancellationToken);
        if (existing is not null)
            return;

        // Create the "General" family conversation
        var conversation = Conversation.CreateFamily(@event.FamilyId, @event.OwnerId);

        // Create folder hierarchy: root → Messages → General
        var rootFolder = await folderRepository.GetRootFolderAsync(@event.FamilyId, cancellationToken);
        if (rootFolder is not null)
        {
            // Find or create the "Messages" folder
            var children = await folderRepository.GetChildrenAsync(rootFolder.Id, cancellationToken);
            var messagesFolder = children.FirstOrDefault(f => f.Name.Value == "Messages");

            if (messagesFolder is null)
            {
                messagesFolder = Folder.Create(
                    FileName.From("Messages"),
                    rootFolder.Id,
                    $"/{rootFolder.Id.Value}/",
                    @event.FamilyId,
                    @event.OwnerId);

                await folderRepository.AddAsync(messagesFolder, cancellationToken);
            }

            // Create the "General" subfolder
            var generalFolder = Folder.Create(
                FileName.From("General"),
                messagesFolder.Id,
                $"{messagesFolder.MaterializedPath}{messagesFolder.Id.Value}/",
                @event.FamilyId,
                @event.OwnerId);

            await folderRepository.AddAsync(generalFolder, cancellationToken);
            conversation.SetFolderId(generalFolder.Id);
        }

        await conversationRepository.AddAsync(conversation, cancellationToken);

        // Explicit save — event handlers run outside the TransactionBehavior pipeline
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Created General conversation with folder for family {FamilyId}",
            @event.FamilyId);
    }
}
