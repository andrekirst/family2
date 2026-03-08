using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFolder;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<FolderDto> CreateFolder(
        CreateFolderRequest input,
        [Service] ICommandBus commandBus,
        [Service] IFolderRepository folderRepository,
        CancellationToken cancellationToken)
    {
        var command = new CreateFolderCommand(
            FileName.From(input.Name.Trim()),
            input.ParentFolderId.HasValue ? FolderId.From(input.ParentFolderId.Value) : null);

        var result = await commandBus.SendAsync(command, cancellationToken);

        var folder = await folderRepository.GetByIdAsync(result.FolderId, cancellationToken)
            ?? throw new InvalidOperationException("Folder creation failed");

        return FileManagementMapper.ToDto(folder);
    }
}
