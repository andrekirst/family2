using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFolder;

public sealed class CreateFolderCommandHandler(
    IFolderRepository folderRepository)
    : ICommandHandler<CreateFolderCommand, CreateFolderResult>
{
    public async ValueTask<CreateFolderResult> Handle(
        CreateFolderCommand command,
        CancellationToken cancellationToken)
    {
        string materializedPath;
        FolderId? effectiveParentId = command.ParentFolderId;

        if (command.ParentFolderId is not null)
        {
            // Validate parent folder exists and belongs to the same family
            var parentFolder = await folderRepository.GetByIdAsync(command.ParentFolderId.Value, cancellationToken)
                ?? throw new DomainException("Parent folder not found", DomainErrorCodes.NotFound);

            if (parentFolder.FamilyId != command.FamilyId)
            {
                throw new DomainException("Parent folder belongs to a different family", DomainErrorCodes.Forbidden);
            }

            // Build materialized path: parent path + parent id + /
            materializedPath = parentFolder.MaterializedPath == "/"
                ? $"/{parentFolder.Id.Value}/"
                : $"{parentFolder.MaterializedPath}{parentFolder.Id.Value}/";
        }
        else
        {
            // No parent specified — place under root folder
            var rootFolder = await folderRepository.GetRootFolderAsync(command.FamilyId, cancellationToken);

            if (rootFolder is null)
            {
                // Auto-create root folder
                rootFolder = Folder.CreateRoot(command.FamilyId, command.CreatedBy);
                await folderRepository.AddAsync(rootFolder, cancellationToken);
            }

            effectiveParentId = rootFolder.Id;
            materializedPath = $"/{rootFolder.Id.Value}/";
        }

        var folder = Folder.Create(
            command.Name,
            effectiveParentId,
            materializedPath,
            command.FamilyId,
            command.CreatedBy);

        await folderRepository.AddAsync(folder, cancellationToken);

        return new CreateFolderResult(folder.Id);
    }
}
