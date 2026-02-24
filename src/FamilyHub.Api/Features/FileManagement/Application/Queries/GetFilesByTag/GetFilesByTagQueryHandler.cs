using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFilesByTag;

public sealed class GetFilesByTagQueryHandler(
    IFileTagRepository fileTagRepository,
    IStoredFileRepository storedFileRepository)
    : IQueryHandler<GetFilesByTagQuery, List<StoredFileDto>>
{
    public async ValueTask<List<StoredFileDto>> Handle(
        GetFilesByTagQuery query,
        CancellationToken cancellationToken)
    {
        if (query.TagIds.Count == 0)
            return [];

        // Get file IDs that have ALL specified tags (AND logic)
        var fileIds = await fileTagRepository.GetFileIdsByTagIdsAsync(query.TagIds, cancellationToken);

        if (fileIds.Count == 0)
            return [];

        var files = await storedFileRepository.GetByIdsAsync(fileIds, cancellationToken);

        // Filter to only files belonging to the requesting family
        return files
            .Where(f => f.FamilyId == query.FamilyId)
            .Select(FileManagementMapper.ToDto)
            .ToList();
    }
}
