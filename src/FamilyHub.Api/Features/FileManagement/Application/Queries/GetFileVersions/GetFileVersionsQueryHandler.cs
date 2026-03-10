using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFileVersions;

public sealed class GetFileVersionsQueryHandler(
    IFileVersionRepository versionRepository,
    IStoredFileRepository fileRepository)
    : IQueryHandler<GetFileVersionsQuery, Result<List<FileVersionDto>>>
{
    public async ValueTask<Result<List<FileVersionDto>>> Handle(
        GetFileVersionsQuery query,
        CancellationToken cancellationToken)
    {
        var file = await fileRepository.GetByIdAsync(query.FileId, cancellationToken);
        if (file is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found");
        }

        if (file.FamilyId != query.FamilyId)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found in this family");
        }

        var versions = await versionRepository.GetByFileIdAsync(query.FileId, cancellationToken);

        return versions.Select(FileManagementMapper.ToDto).ToList();
    }
}
