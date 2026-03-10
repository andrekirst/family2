using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IShareLinkAccessLogRepository
{
    Task<List<ShareLinkAccessLog>> GetByShareLinkIdAsync(ShareLinkId shareLinkId, CancellationToken cancellationToken = default);
    Task AddAsync(ShareLinkAccessLog log, CancellationToken cancellationToken = default);
}
