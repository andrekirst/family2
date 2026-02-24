using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IFileVersionRepository
{
    Task<FileVersion?> GetByIdAsync(FileVersionId id, CancellationToken ct = default);
    Task<List<FileVersion>> GetByFileIdAsync(FileId fileId, CancellationToken ct = default);
    Task<FileVersion?> GetCurrentVersionAsync(FileId fileId, CancellationToken ct = default);
    Task<int> GetMaxVersionNumberAsync(FileId fileId, CancellationToken ct = default);
    Task AddAsync(FileVersion version, CancellationToken ct = default);
}
