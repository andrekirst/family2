using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFiles;

public sealed class GetFilesQueryHandler(
    IStoredFileRepository storedFileRepository)
    : IQueryHandler<GetFilesQuery, List<StoredFileDto>>
{
    public async ValueTask<List<StoredFileDto>> Handle(
        GetFilesQuery query,
        CancellationToken cancellationToken)
    {
        var files = await storedFileRepository.GetByFolderIdAsync(query.FolderId, cancellationToken);

        return files
            .Where(f => f.FamilyId == query.FamilyId)
            .Select(FileManagementMapper.ToDto)
            .ToList();
    }
}
