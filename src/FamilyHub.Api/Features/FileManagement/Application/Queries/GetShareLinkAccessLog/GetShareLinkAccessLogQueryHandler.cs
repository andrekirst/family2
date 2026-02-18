using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetShareLinkAccessLog;

public sealed class GetShareLinkAccessLogQueryHandler(
    IShareLinkRepository shareLinkRepository,
    IShareLinkAccessLogRepository accessLogRepository)
    : IQueryHandler<GetShareLinkAccessLogQuery, List<ShareLinkAccessLogDto>>
{
    public async ValueTask<List<ShareLinkAccessLogDto>> Handle(
        GetShareLinkAccessLogQuery query,
        CancellationToken cancellationToken)
    {
        var link = await shareLinkRepository.GetByIdAsync(query.ShareLinkId, cancellationToken)
            ?? throw new DomainException("Share link not found", DomainErrorCodes.ShareLinkNotFound);

        if (link.FamilyId != query.FamilyId)
            throw new DomainException("Share link not found", DomainErrorCodes.ShareLinkNotFound);

        var logs = await accessLogRepository.GetByShareLinkIdAsync(query.ShareLinkId, cancellationToken);

        return logs.Select(FileManagementMapper.ToDto).ToList();
    }
}
