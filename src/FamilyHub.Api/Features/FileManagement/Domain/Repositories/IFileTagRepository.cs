using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IFileTagRepository
{
    Task<List<Entities.FileTag>> GetByFileIdAsync(FileId fileId, CancellationToken ct = default);
    Task<List<Entities.FileTag>> GetByTagIdAsync(TagId tagId, CancellationToken ct = default);
    Task<bool> ExistsAsync(FileId fileId, TagId tagId, CancellationToken ct = default);
    Task AddAsync(Entities.FileTag fileTag, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Entities.FileTag> fileTags, CancellationToken ct = default);
    Task RemoveAsync(Entities.FileTag fileTag, CancellationToken ct = default);
    Task RemoveByTagIdAsync(TagId tagId, CancellationToken ct = default);
    Task<int> GetFileCountByTagIdAsync(TagId tagId, CancellationToken ct = default);

    /// <summary>
    /// Gets file IDs that have ALL of the specified tags (AND logic).
    /// </summary>
    Task<List<FileId>> GetFileIdsByTagIdsAsync(IEnumerable<TagId> tagIds, CancellationToken ct = default);
}
