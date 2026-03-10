using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetShareLinkAccessLog;

public sealed class GetShareLinkAccessLogQueryHandler(
    IShareLinkRepository shareLinkRepository,
    IShareLinkAccessLogRepository accessLogRepository)
    : IQueryHandler<GetShareLinkAccessLogQuery, Result<List<ShareLinkAccessLogDto>>>
{
    public async ValueTask<Result<List<ShareLinkAccessLogDto>>> Handle(
        GetShareLinkAccessLogQuery query,
        CancellationToken cancellationToken)
    {
        var link = await shareLinkRepository.GetByIdAsync(query.ShareLinkId, cancellationToken);
        if (link is null)
        {
            return DomainError.NotFound(DomainErrorCodes.ShareLinkNotFound, "Share link not found");
        }

        if (link.FamilyId != query.FamilyId)
        {
            return DomainError.NotFound(DomainErrorCodes.ShareLinkNotFound, "Share link not found");
        }

        var logs = await accessLogRepository.GetByShareLinkIdAsync(query.ShareLinkId, cancellationToken);

        return logs.Select(FileManagementMapper.ToDto).ToList();
    }
}
