using FamilyHub.Common.Application;
using FamilyHub.Api.Features.GoogleIntegration.Application.Mappers;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Models;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetLinkedAccounts;

public sealed class GetLinkedAccountsQueryHandler(
    IGoogleAccountLinkRepository linkRepository)
    : IQueryHandler<GetLinkedAccountsQuery, List<LinkedAccountDto>>
{
    public async ValueTask<List<LinkedAccountDto>> Handle(
        GetLinkedAccountsQuery query,
        CancellationToken cancellationToken)
    {
        var link = await linkRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        return link is null
            ? []
            : [GoogleIntegrationMapper.ToLinkedAccountDto(link)];
    }
}
