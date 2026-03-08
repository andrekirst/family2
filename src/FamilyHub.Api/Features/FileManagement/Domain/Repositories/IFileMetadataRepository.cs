using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IFileMetadataRepository
{
    Task<Entities.FileMetadata?> GetByFileIdAsync(FileId fileId, CancellationToken cancellationToken = default);
    Task AddAsync(Entities.FileMetadata metadata, CancellationToken cancellationToken = default);
    Task RemoveAsync(Entities.FileMetadata metadata, CancellationToken cancellationToken = default);
}
