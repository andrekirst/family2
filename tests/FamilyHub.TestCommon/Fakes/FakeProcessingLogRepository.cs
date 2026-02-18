using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeProcessingLogRepository : IProcessingLogRepository
{
    public List<ProcessingLogEntry> Entries { get; } = [];

    public Task<List<ProcessingLogEntry>> GetByFamilyIdAsync(
        FamilyId familyId, int skip = 0, int take = 50, CancellationToken ct = default)
        => Task.FromResult(Entries
            .Where(e => e.FamilyId == familyId)
            .OrderByDescending(e => e.ProcessedAt)
            .Skip(skip)
            .Take(take)
            .ToList());

    public Task AddAsync(ProcessingLogEntry entry, CancellationToken ct = default)
    {
        Entries.Add(entry);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<ProcessingLogEntry> entries, CancellationToken ct = default)
    {
        Entries.AddRange(entries);
        return Task.CompletedTask;
    }
}
