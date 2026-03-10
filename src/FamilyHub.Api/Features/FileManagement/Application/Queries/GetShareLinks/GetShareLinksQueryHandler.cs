using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetShareLinks;

public sealed class GetShareLinksQueryHandler(
    IShareLinkRepository shareLinkRepository,
    TimeProvider timeProvider)
    : IQueryHandler<GetShareLinksQuery, List<ShareLinkDto>>
{
    public async ValueTask<List<ShareLinkDto>> Handle(
        GetShareLinksQuery query,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var links = await shareLinkRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);

        return links.Select(l => FileManagementMapper.ToDto(l, utcNow)).ToList();
    }
}
