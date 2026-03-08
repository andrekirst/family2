using FamilyHub.Common.Application;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Application.Mappers;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Models;

namespace FamilyHub.Api.Features.Messaging.Application.Queries.GetConversations;

/// <summary>
/// Handler for GetConversationsQuery.
/// Returns conversations where the user is an active member.
/// Auto-creates the "General" family conversation if none exists (self-healing for
/// families created before the conversation feature was deployed).
/// TransactionBehavior handles SaveChanges.
/// </summary>
public sealed class GetConversationsQueryHandler(
    IConversationRepository conversationRepository,
    IFolderRepository folderRepository)
    : IQueryHandler<GetConversationsQuery, List<ConversationDto>>
{
    public async ValueTask<List<ConversationDto>> Handle(
        GetConversationsQuery query,
        CancellationToken cancellationToken)
    {
        var conversations = await conversationRepository.GetByUserAsync(
            query.FamilyId, query.UserId, cancellationToken);

        // Self-healing: create "General" conversation if none exists for this family
        if (!conversations.Any(c => c.Type == ConversationType.Family))
        {
            var general = await EnsureFamilyConversationAsync(query, cancellationToken);
            if (general is not null)
                conversations.Insert(0, general);
        }

        return conversations.Select(ConversationMapper.ToDto).ToList();
    }

    /// <summary>
    /// Creates the default "General" family conversation with folder hierarchy.
    /// Idempotent: checks repository-level to avoid duplicates from concurrent requests.
    /// </summary>
    private async Task<Conversation?> EnsureFamilyConversationAsync(
        GetConversationsQuery query, CancellationToken cancellationToken)
    {
        // Double-check at repository level (another request may have created it)
        var existing = await conversationRepository.GetFamilyConversationAsync(query.FamilyId, cancellationToken);
        if (existing is not null)
            return existing;

        var conversation = Conversation.CreateFamily(query.FamilyId, query.UserId);

        // Create folder hierarchy: root → Messages → General
        var rootFolder = await folderRepository.GetRootFolderAsync(query.FamilyId, cancellationToken);
        if (rootFolder is not null)
        {
            var children = await folderRepository.GetChildrenAsync(rootFolder.Id, cancellationToken);
            var messagesFolder = children.FirstOrDefault(f => f.Name.Value == "Messages");

            if (messagesFolder is null)
            {
                messagesFolder = Folder.Create(
                    FileName.From("Messages"),
                    rootFolder.Id,
                    $"/{rootFolder.Id.Value}/",
                    query.FamilyId,
                    query.UserId);

                await folderRepository.AddAsync(messagesFolder, cancellationToken);
            }

            var generalFolder = Folder.Create(
                FileName.From("General"),
                messagesFolder.Id,
                $"{messagesFolder.MaterializedPath}{messagesFolder.Id.Value}/",
                query.FamilyId,
                query.UserId);

            await folderRepository.AddAsync(generalFolder, cancellationToken);
            conversation.SetFolderId(generalFolder.Id);
        }

        await conversationRepository.AddAsync(conversation, cancellationToken);

        return conversation;
    }
}
