using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFileMetadataRepository : IFileMetadataRepository
{
    public List<FileMetadata> Metadata { get; } = [];

    public Task<FileMetadata?> GetByFileIdAsync(FileId fileId, CancellationToken ct = default)
        => Task.FromResult(Metadata.FirstOrDefault(m => m.FileId == fileId));

    public Task AddAsync(FileMetadata metadata, CancellationToken ct = default)
    {
        Metadata.Add(metadata);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(FileMetadata metadata, CancellationToken ct = default)
    {
        Metadata.Remove(metadata);
        return Task.CompletedTask;
    }
}
