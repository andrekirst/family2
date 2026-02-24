using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Family.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;

namespace FamilyHub.Api.Features.FileManagement.Application.EventHandlers;

/// <summary>
/// Bootstraps root and inbox folders when a new family is created.
/// Runs outside the pipeline — calls SaveChangesAsync explicitly.
/// </summary>
public sealed class FamilyCreatedEventHandler(
    IFolderRepository folderRepository,
    AppDbContext context,
    ILogger<FamilyCreatedEventHandler> logger)
    : IDomainEventHandler<FamilyCreatedEvent>
{
    public async ValueTask Handle(FamilyCreatedEvent @event, CancellationToken cancellationToken)
    {
        // Guard: skip if root/inbox already exist (idempotent)
        var existingRoot = await folderRepository.GetRootFolderAsync(@event.FamilyId, cancellationToken);
        if (existingRoot is not null)
        {
            var existingInbox = await folderRepository.GetInboxFolderAsync(@event.FamilyId, cancellationToken);
            if (existingInbox is not null)
                return;
        }

        // Create root folder if needed
        var root = existingRoot ?? Folder.CreateRoot(@event.FamilyId, @event.OwnerId);
        if (existingRoot is null)
            await folderRepository.AddAsync(root, cancellationToken);

        // Create inbox folder
        var inbox = Folder.CreateInbox(root.Id, @event.FamilyId, @event.OwnerId);
        await folderRepository.AddAsync(inbox, cancellationToken);

        // Explicit save — event handlers run outside the TransactionBehavior pipeline
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Created root and inbox folders for family {FamilyId}",
            @event.FamilyId);
    }
}
