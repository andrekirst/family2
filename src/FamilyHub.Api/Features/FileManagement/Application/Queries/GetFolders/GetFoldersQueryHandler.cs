using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFolders;

public sealed class GetFoldersQueryHandler(
    IFolderRepository folderRepository)
    : IQueryHandler<GetFoldersQuery, List<FolderDto>>
{
    public async ValueTask<List<FolderDto>> Handle(
        GetFoldersQuery query,
        CancellationToken cancellationToken)
    {
        var parentFolderId = query.ParentFolderId;

        if (parentFolderId is null)
        {
            var rootFolder = await folderRepository.GetRootFolderAsync(query.FamilyId, cancellationToken);
            if (rootFolder is null)
            {
                rootFolder = Folder.CreateRoot(query.FamilyId, query.UserId);
                await folderRepository.AddAsync(rootFolder, cancellationToken);
            }

            parentFolderId = rootFolder.Id;
        }

        var folders = await folderRepository.GetChildrenAsync(parentFolderId.Value, cancellationToken);

        return folders
            .Where(f => f.FamilyId == query.FamilyId)
            .Select(FileManagementMapper.ToDto)
            .ToList();
    }
}
