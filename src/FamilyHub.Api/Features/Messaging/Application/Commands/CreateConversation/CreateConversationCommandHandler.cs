using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.CreateConversation;

/// <summary>
/// Handler for CreateConversationCommand.
/// Creates a Conversation aggregate and its dedicated folder for attachments.
/// Transaction behavior handles SaveChanges.
/// </summary>
public sealed class CreateConversationCommandHandler(
    IConversationRepository conversationRepository,
    IFolderRepository folderRepository)
    : ICommandHandler<CreateConversationCommand, CreateConversationResult>
{
    public async ValueTask<CreateConversationResult> Handle(
        CreateConversationCommand command,
        CancellationToken cancellationToken)
    {
        // For Family type, ensure only one exists per family
        if (command.Type == ConversationType.Family)
        {
            var existing = await conversationRepository.GetFamilyConversationAsync(command.FamilyId, cancellationToken);
            if (existing is not null)
                return new CreateConversationResult(existing.Id);
        }

        // Create the conversation aggregate
        var memberUserIds = command.MemberIds.Select(UserId.From).ToList();

        var conversation = command.Type == ConversationType.Family
            ? Conversation.CreateFamily(command.FamilyId, command.CreatedBy)
            : Conversation.Create(command.Name, command.Type, command.FamilyId, command.CreatedBy, memberUserIds);

        // Create folder hierarchy: root → Messages → {ConversationName}
        var rootFolder = await folderRepository.GetRootFolderAsync(command.FamilyId, cancellationToken)
            ?? throw new DomainException("Family root folder not found", DomainErrorCodes.NotFound);

        // Find or create the "Messages" parent folder
        var messagesFolder = await FindOrCreateMessagesFolderAsync(
            rootFolder, command.FamilyId, command.CreatedBy, cancellationToken);

        // Create the conversation-specific subfolder
        var conversationFolder = Folder.Create(
            FileName.From(conversation.Name.Value),
            messagesFolder.Id,
            $"{messagesFolder.MaterializedPath}{messagesFolder.Id.Value}/",
            command.FamilyId,
            command.CreatedBy);

        await folderRepository.AddAsync(conversationFolder, cancellationToken);
        conversation.SetFolderId(conversationFolder.Id);

        await conversationRepository.AddAsync(conversation, cancellationToken);

        return new CreateConversationResult(conversation.Id);
    }

    private async Task<Folder> FindOrCreateMessagesFolderAsync(
        Folder rootFolder, FamilyId familyId, UserId createdBy, CancellationToken ct)
    {
        // Look for existing "Messages" folder under root
        var children = await folderRepository.GetChildrenAsync(rootFolder.Id, ct);
        var messagesFolder = children.FirstOrDefault(f => f.Name.Value == "Messages");

        if (messagesFolder is not null)
            return messagesFolder;

        // Create the "Messages" folder
        messagesFolder = Folder.Create(
            FileName.From("Messages"),
            rootFolder.Id,
            $"/{rootFolder.Id.Value}/",
            familyId,
            createdBy);

        await folderRepository.AddAsync(messagesFolder, ct);
        return messagesFolder;
    }
}
