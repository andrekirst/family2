using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.SearchFiles;

public sealed class SearchFilesQueryHandler(
    IFileSearchService fileSearchService,
    IRecentSearchRepository recentSearchRepository)
    : IQueryHandler<SearchFilesQuery, List<FileSearchResultDto>>
{
    public async ValueTask<List<FileSearchResultDto>> Handle(
        SearchFilesQuery query,
        CancellationToken cancellationToken)
    {
        // Record the search query for recent searches
        var recentSearch = RecentSearch.Create(query.UserId, query.Query);
        await recentSearchRepository.AddAsync(recentSearch, cancellationToken);
        await recentSearchRepository.RemoveOldestAsync(query.UserId, 10, cancellationToken);

        // Execute the search
        return await fileSearchService.SearchAsync(
            query.Query,
            query.FamilyId,
            query.Filters,
            query.SortBy,
            query.Skip,
            query.Take,
            cancellationToken);
    }
}
