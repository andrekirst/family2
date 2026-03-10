using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IFileTagRepository
{
    Task<List<Entities.FileTag>> GetByFileIdAsync(FileId fileId, CancellationToken cancellationToken = default);
    Task<List<Entities.FileTag>> GetByTagIdAsync(TagId tagId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(FileId fileId, TagId tagId, CancellationToken cancellationToken = default);
    Task AddAsync(Entities.FileTag fileTag, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Entities.FileTag> fileTags, CancellationToken cancellationToken = default);
    Task RemoveAsync(Entities.FileTag fileTag, CancellationToken cancellationToken = default);
    Task RemoveByTagIdAsync(TagId tagId, CancellationToken cancellationToken = default);
    Task<int> GetFileCountByTagIdAsync(TagId tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets file IDs that have ALL of the specified tags (AND logic).
    /// </summary>
    Task<List<FileId>> GetFileIdsByTagIdsAsync(IEnumerable<TagId> tagIds, CancellationToken cancellationToken = default);
}
