using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IShareLinkRepository
{
    Task<ShareLink?> GetByIdAsync(ShareLinkId id, CancellationToken ct = default);
    Task<ShareLink?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<List<ShareLink>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);
    Task<List<ShareLink>> GetActiveByResourceIdAsync(Guid resourceId, CancellationToken ct = default);
    Task AddAsync(ShareLink link, CancellationToken ct = default);
}
