using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeShareLinkAccessLogRepository : IShareLinkAccessLogRepository
{
    public List<ShareLinkAccessLog> Logs { get; } = [];

    public Task<List<ShareLinkAccessLog>> GetByShareLinkIdAsync(ShareLinkId shareLinkId, CancellationToken ct = default)
        => Task.FromResult(Logs
            .Where(l => l.ShareLinkId == shareLinkId)
            .OrderByDescending(l => l.AccessedAt)
            .ToList());

    public Task AddAsync(ShareLinkAccessLog log, CancellationToken ct = default)
    {
        Logs.Add(log);
        return Task.CompletedTask;
    }
}
