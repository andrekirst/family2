using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFileMetadataRepository : IFileMetadataRepository
{
    public List<FileMetadata> Metadata { get; } = [];

    public Task<FileMetadata?> GetByFileIdAsync(FileId fileId, CancellationToken cancellationToken = default)
        => Task.FromResult(Metadata.FirstOrDefault(m => m.FileId == fileId));

    public Task AddAsync(FileMetadata metadata, CancellationToken cancellationToken = default)
    {
        Metadata.Add(metadata);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(FileMetadata metadata, CancellationToken cancellationToken = default)
    {
        Metadata.Remove(metadata);
        return Task.CompletedTask;
    }
}
