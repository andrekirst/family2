using FamilyHub.Api.Features.FileManagement.Application.Mappers;
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
        var folders = await folderRepository.GetChildrenAsync(query.ParentFolderId, cancellationToken);

        return folders
            .Where(f => f.FamilyId == query.FamilyId)
            .Select(FileManagementMapper.ToDto)
            .ToList();
    }
}
