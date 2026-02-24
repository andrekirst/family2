using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class ProcessingLogRepository(AppDbContext context) : IProcessingLogRepository
{
    public async Task<List<ProcessingLogEntry>> GetByFamilyIdAsync(
        FamilyId familyId, int skip = 0, int take = 50, CancellationToken ct = default)
        => await context.Set<ProcessingLogEntry>()
            .Where(e => e.FamilyId == familyId)
            .OrderByDescending(e => e.ProcessedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

    public async Task AddAsync(ProcessingLogEntry entry, CancellationToken ct = default)
        => await context.Set<ProcessingLogEntry>().AddAsync(entry, ct);

    public async Task AddRangeAsync(IEnumerable<ProcessingLogEntry> entries, CancellationToken ct = default)
        => await context.Set<ProcessingLogEntry>().AddRangeAsync(entries, ct);
}
