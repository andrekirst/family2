using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.SetPermission;

public sealed class SetPermissionCommandHandler(
    IFilePermissionRepository permissionRepository,
    IStoredFileRepository storedFileRepository,
    IFolderRepository folderRepository)
    : ICommandHandler<SetPermissionCommand, SetPermissionResult>
{
    public async ValueTask<SetPermissionResult> Handle(
        SetPermissionCommand command,
        CancellationToken cancellationToken)
    {
        // Validate resource exists and belongs to the family
        if (command.ResourceType == PermissionResourceType.File)
        {
            var file = await storedFileRepository.GetByIdAsync(
                FileId.From(command.ResourceId), cancellationToken)
                ?? throw new DomainException("File not found", DomainErrorCodes.FileNotFound);

            if (file.FamilyId != command.FamilyId)
                throw new DomainException("File belongs to a different family", DomainErrorCodes.Forbidden);
        }
        else
        {
            var folder = await folderRepository.GetByIdAsync(
                FolderId.From(command.ResourceId), cancellationToken)
                ?? throw new DomainException("Folder not found", DomainErrorCodes.FolderNotFound);

            if (folder.FamilyId != command.FamilyId)
                throw new DomainException("Folder belongs to a different family", DomainErrorCodes.Forbidden);
        }

        // Check for existing permission — update or create
        var existing = await permissionRepository.GetByMemberAndResourceAsync(
            command.MemberId, command.ResourceType, command.ResourceId, cancellationToken);

        if (existing is not null)
        {
            existing.UpdateLevel(command.PermissionLevel, command.GrantedBy);
            return new SetPermissionResult(true, existing.Id.Value);
        }

        var permission = FilePermission.Create(
            command.ResourceType,
            command.ResourceId,
            command.MemberId,
            command.PermissionLevel,
            command.FamilyId,
            command.GrantedBy);

        await permissionRepository.AddAsync(permission, cancellationToken);

        return new SetPermissionResult(true, permission.Id.Value);
    }
}
