using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFiles;

public sealed class GetFilesQueryHandler(
    IStoredFileRepository storedFileRepository,
    IFolderRepository folderRepository)
    : IQueryHandler<GetFilesQuery, List<StoredFileDto>>
{
    public async ValueTask<List<StoredFileDto>> Handle(
        GetFilesQuery query,
        CancellationToken cancellationToken)
    {
        var folderId = query.FolderId;

        if (folderId is null)
        {
            var rootFolder = await folderRepository.GetRootFolderAsync(query.FamilyId, cancellationToken);
            if (rootFolder is null)
            {
                rootFolder = Folder.CreateRoot(query.FamilyId, query.UserId);
                await folderRepository.AddAsync(rootFolder, cancellationToken);
            }

            folderId = rootFolder.Id;
        }

        var files = await storedFileRepository.GetByFolderIdAsync(folderId.Value, cancellationToken);

        return files
            .Where(f => f.FamilyId == query.FamilyId)
            .Select(FileManagementMapper.ToDto)
            .ToList();
    }
}
