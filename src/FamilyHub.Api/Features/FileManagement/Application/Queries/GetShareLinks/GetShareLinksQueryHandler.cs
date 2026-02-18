using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetShareLinks;

public sealed class GetShareLinksQueryHandler(
    IShareLinkRepository shareLinkRepository)
    : IQueryHandler<GetShareLinksQuery, List<ShareLinkDto>>
{
    public async ValueTask<List<ShareLinkDto>> Handle(
        GetShareLinksQuery query,
        CancellationToken cancellationToken)
    {
        var links = await shareLinkRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);

        return links.Select(FileManagementMapper.ToDto).ToList();
    }
}
