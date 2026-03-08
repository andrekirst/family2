using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IFileVersionRepository : IWriteRepository<FileVersion, FileVersionId>
{
    Task<List<FileVersion>> GetByFileIdAsync(FileId fileId, CancellationToken cancellationToken = default);
    Task<FileVersion?> GetCurrentVersionAsync(FileId fileId, CancellationToken cancellationToken = default);
    Task<int> GetMaxVersionNumberAsync(FileId fileId, CancellationToken cancellationToken = default);
}
