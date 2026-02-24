using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetRecentSearches;

public sealed class GetRecentSearchesQueryHandler(
    IRecentSearchRepository recentSearchRepository)
    : IQueryHandler<GetRecentSearchesQuery, List<RecentSearchDto>>
{
    public async ValueTask<List<RecentSearchDto>> Handle(
        GetRecentSearchesQuery query,
        CancellationToken cancellationToken)
    {
        var searches = await recentSearchRepository.GetByUserIdAsync(
            query.UserId, 10, cancellationToken);

        return searches
            .Select(s => new RecentSearchDto(s.Id.Value, s.Query, s.SearchedAt))
            .ToList();
    }
}
