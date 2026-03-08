using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFolder;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<FolderDto> MoveFolder(
        MoveFolderRequest input,
        [Service] ICommandBus commandBus,
        [Service] IFolderRepository folderRepository,
        CancellationToken cancellationToken)
    {
        var command = new MoveFolderCommand(
            FolderId.From(input.FolderId),
            FolderId.From(input.TargetParentFolderId));

        var result = await commandBus.SendAsync(command, cancellationToken);

        var folder = await folderRepository.GetByIdAsync(result.FolderId, cancellationToken)
            ?? throw new InvalidOperationException("Folder move failed");

        return FileManagementMapper.ToDto(folder);
    }
}
