using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeGoogleAccountLinkRepository(GoogleAccountLink? existingLink = null) : IGoogleAccountLinkRepository
{
    public List<GoogleAccountLink> AddedLinks { get; } = [];
    public List<GoogleAccountLink> DeletedLinks { get; } = [];
    public GoogleAccountLink? StoredLink => existingLink;
    private int _saveChangesCount;
    public int SaveChangesCount => _saveChangesCount;

    public Task<GoogleAccountLink?> GetByUserIdAsync(UserId userId, CancellationToken ct = default)
    {
        var result = existingLink?.UserId == userId ? existingLink
            : AddedLinks.FirstOrDefault(l => l.UserId == userId);
        return Task.FromResult(result);
    }

    public Task<GoogleAccountLink?> GetByGoogleAccountIdAsync(GoogleAccountId googleAccountId, CancellationToken ct = default)
    {
        var result = existingLink?.GoogleAccountId == googleAccountId ? existingLink
            : AddedLinks.FirstOrDefault(l => l.GoogleAccountId == googleAccountId);
        return Task.FromResult(result);
    }

    public Task<List<GoogleAccountLink>> GetExpiringTokensAsync(DateTime expiringBefore, CancellationToken ct = default)
    {
        var all = new List<GoogleAccountLink>();
        if (existingLink is not null) all.Add(existingLink);
        all.AddRange(AddedLinks);

        var result = all
            .Where(l => l.Status == GoogleLinkStatus.Active && l.AccessTokenExpiresAt <= expiringBefore)
            .ToList();
        return Task.FromResult(result);
    }

    public Task AddAsync(GoogleAccountLink link, CancellationToken ct = default)
    {
        AddedLinks.Add(link);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(GoogleAccountLink link, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task DeleteAsync(GoogleAccountLink link, CancellationToken ct = default)
    {
        DeletedLinks.Add(link);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        _saveChangesCount++;
        return Task.FromResult(1);
    }
}
