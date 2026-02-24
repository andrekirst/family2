using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFolder;

public sealed class GetFolderQueryHandler(
    IFolderRepository folderRepository)
    : IQueryHandler<GetFolderQuery, FolderDto?>
{
    public async ValueTask<FolderDto?> Handle(
        GetFolderQuery query,
        CancellationToken cancellationToken)
    {
        var folder = await folderRepository.GetByIdAsync(query.FolderId, cancellationToken);

        if (folder is null || folder.FamilyId != query.FamilyId)
        {
            return null;
        }

        return FileManagementMapper.ToDto(folder);
    }
}
