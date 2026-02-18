using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Services;

/// <summary>
/// Abstracts PostgreSQL full-text search capabilities.
/// Production implementation uses tsvector/tsquery/GIN indexes and pg_trgm for fuzzy matching.
/// </summary>
public interface IFileSearchService
{
    Task<List<FileSearchResultDto>> SearchAsync(
        string query,
        FamilyId familyId,
        SearchFiltersDto? filters = null,
        string sortBy = "relevance",
        int skip = 0,
        int take = 20,
        CancellationToken ct = default);
}
