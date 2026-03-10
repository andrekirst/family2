using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeShareLinkRepository : IShareLinkRepository
{
    public List<ShareLink> Links { get; } = [];

    public Task<ShareLink?> GetByIdAsync(ShareLinkId id, CancellationToken cancellationToken = default)
        => Task.FromResult(Links.FirstOrDefault(l => l.Id == id));

    public Task<bool> ExistsByIdAsync(ShareLinkId id, CancellationToken cancellationToken = default)
        => Task.FromResult(Links.Any(l => l.Id == id));

    public Task<ShareLink?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => Task.FromResult(Links.FirstOrDefault(l => l.Token == token));

    public Task<List<ShareLink>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
        => Task.FromResult(Links
            .Where(l => l.FamilyId == familyId)
            .OrderByDescending(l => l.CreatedAt)
            .ToList());

    public Task<List<ShareLink>> GetActiveByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken = default)
        => Task.FromResult(Links
            .Where(l => l.ResourceId == resourceId && !l.IsRevoked)
            .ToList());

    public Task AddAsync(ShareLink link, CancellationToken cancellationToken = default)
    {
        Links.Add(link);
        return Task.CompletedTask;
    }
}
