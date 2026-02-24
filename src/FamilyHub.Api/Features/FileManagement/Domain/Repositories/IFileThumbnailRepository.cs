using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IFileThumbnailRepository
{
    Task<List<FileThumbnail>> GetByFileIdAsync(FileId fileId, CancellationToken ct = default);
    Task<FileThumbnail?> GetByFileIdAndSizeAsync(FileId fileId, int width, int height, CancellationToken ct = default);
    Task AddAsync(FileThumbnail thumbnail, CancellationToken ct = default);
    Task RemoveByFileIdAsync(FileId fileId, CancellationToken ct = default);
}
