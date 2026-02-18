using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFileVersions;

public sealed class GetFileVersionsQueryHandler(
    IFileVersionRepository versionRepository,
    IStoredFileRepository fileRepository)
    : IQueryHandler<GetFileVersionsQuery, List<FileVersionDto>>
{
    public async ValueTask<List<FileVersionDto>> Handle(
        GetFileVersionsQuery query,
        CancellationToken cancellationToken)
    {
        var file = await fileRepository.GetByIdAsync(query.FileId, cancellationToken)
            ?? throw new DomainException("File not found", DomainErrorCodes.FileNotFound);

        if (file.FamilyId != query.FamilyId)
            throw new DomainException("File not found in this family", DomainErrorCodes.FileNotFound);

        var versions = await versionRepository.GetByFileIdAsync(query.FileId, cancellationToken);

        return versions.Select(FileManagementMapper.ToDto).ToList();
    }
}
