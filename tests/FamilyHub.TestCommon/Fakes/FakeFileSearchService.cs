using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFileSearchService : IFileSearchService
{
    public List<FileSearchResultDto> Results { get; } = [];

    public Task<List<FileSearchResultDto>> SearchAsync(
        string query,
        FamilyId familyId,
        SearchFiltersDto? filters = null,
        string sortBy = "relevance",
        int skip = 0,
        int take = 20,
        CancellationToken ct = default)
    {
        var filtered = Results
            .Where(r => r.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Skip(skip)
            .Take(take)
            .ToList();

        return Task.FromResult(filtered);
    }
}
