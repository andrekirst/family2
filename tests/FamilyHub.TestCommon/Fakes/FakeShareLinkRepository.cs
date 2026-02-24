using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeShareLinkRepository : IShareLinkRepository
{
    public List<ShareLink> Links { get; } = [];

    public Task<ShareLink?> GetByIdAsync(ShareLinkId id, CancellationToken ct = default)
        => Task.FromResult(Links.FirstOrDefault(l => l.Id == id));

    public Task<ShareLink?> GetByTokenAsync(string token, CancellationToken ct = default)
        => Task.FromResult(Links.FirstOrDefault(l => l.Token == token));

    public Task<List<ShareLink>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Links
            .Where(l => l.FamilyId == familyId)
            .OrderByDescending(l => l.CreatedAt)
            .ToList());

    public Task<List<ShareLink>> GetActiveByResourceIdAsync(Guid resourceId, CancellationToken ct = default)
        => Task.FromResult(Links
            .Where(l => l.ResourceId == resourceId && !l.IsRevoked)
            .ToList());

    public Task AddAsync(ShareLink link, CancellationToken ct = default)
    {
        Links.Add(link);
        return Task.CompletedTask;
    }
}
