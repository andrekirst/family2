using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IProcessingLogRepository
{
    Task<List<ProcessingLogEntry>> GetByFamilyIdAsync(
        FamilyId familyId, int skip = 0, int take = 50, CancellationToken ct = default);
    Task AddAsync(ProcessingLogEntry entry, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ProcessingLogEntry> entries, CancellationToken ct = default);
}
