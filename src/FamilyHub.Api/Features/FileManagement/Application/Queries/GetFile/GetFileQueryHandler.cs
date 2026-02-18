using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFile;

public sealed class GetFileQueryHandler(
    IStoredFileRepository storedFileRepository)
    : IQueryHandler<GetFileQuery, StoredFileDto?>
{
    public async ValueTask<StoredFileDto?> Handle(
        GetFileQuery query,
        CancellationToken cancellationToken)
    {
        var file = await storedFileRepository.GetByIdAsync(query.FileId, cancellationToken);

        if (file is null || file.FamilyId != query.FamilyId)
        {
            return null;
        }

        return FileManagementMapper.ToDto(file);
    }
}
