using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetSavedSearches;

public sealed class GetSavedSearchesQueryHandler(
    ISavedSearchRepository savedSearchRepository)
    : IQueryHandler<GetSavedSearchesQuery, List<SavedSearchDto>>
{
    public async ValueTask<List<SavedSearchDto>> Handle(
        GetSavedSearchesQuery query,
        CancellationToken cancellationToken)
    {
        var searches = await savedSearchRepository.GetByUserIdAsync(
            query.UserId, cancellationToken);

        return searches
            .Select(s => new SavedSearchDto(
                s.Id.Value, s.Name, s.Query, s.FiltersJson, s.CreatedAt))
            .ToList();
    }
}
