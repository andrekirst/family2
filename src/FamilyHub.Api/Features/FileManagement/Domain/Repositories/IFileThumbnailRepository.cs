using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IFileThumbnailRepository
{
    Task<List<FileThumbnail>> GetByFileIdAsync(FileId fileId, CancellationToken cancellationToken = default);
    Task<FileThumbnail?> GetByFileIdAndSizeAsync(FileId fileId, int width, int height, CancellationToken cancellationToken = default);
    Task AddAsync(FileThumbnail thumbnail, CancellationToken cancellationToken = default);
    Task RemoveByFileIdAsync(FileId fileId, CancellationToken cancellationToken = default);
}
