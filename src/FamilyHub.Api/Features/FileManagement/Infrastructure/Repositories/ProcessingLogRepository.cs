using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class ProcessingLogRepository(AppDbContext context) : IProcessingLogRepository
{
    public async Task<List<ProcessingLogEntry>> GetByFamilyIdAsync(
        FamilyId familyId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        => await context.Set<ProcessingLogEntry>()
            .Where(e => e.FamilyId == familyId)
            .OrderByDescending(e => e.ProcessedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ProcessingLogEntry entry, CancellationToken cancellationToken = default)
        => await context.Set<ProcessingLogEntry>().AddAsync(entry, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<ProcessingLogEntry> entries, CancellationToken cancellationToken = default)
        => await context.Set<ProcessingLogEntry>().AddRangeAsync(entries, cancellationToken);
}
