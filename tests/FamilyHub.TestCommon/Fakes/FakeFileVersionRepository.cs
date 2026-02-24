using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFileVersionRepository : IFileVersionRepository
{
    public List<FileVersion> Versions { get; } = [];

    public Task<FileVersion?> GetByIdAsync(FileVersionId id, CancellationToken ct = default)
        => Task.FromResult(Versions.FirstOrDefault(v => v.Id == id));

    public Task<List<FileVersion>> GetByFileIdAsync(FileId fileId, CancellationToken ct = default)
        => Task.FromResult(Versions
            .Where(v => v.FileId == fileId)
            .OrderByDescending(v => v.VersionNumber)
            .ToList());

    public Task<FileVersion?> GetCurrentVersionAsync(FileId fileId, CancellationToken ct = default)
        => Task.FromResult(Versions.FirstOrDefault(v => v.FileId == fileId && v.IsCurrent));

    public Task<int> GetMaxVersionNumberAsync(FileId fileId, CancellationToken ct = default)
    {
        var versions = Versions.Where(v => v.FileId == fileId).ToList();
        return Task.FromResult(versions.Count > 0 ? versions.Max(v => v.VersionNumber) : 0);
    }

    public Task AddAsync(FileVersion version, CancellationToken ct = default)
    {
        Versions.Add(version);
        return Task.CompletedTask;
    }
}
