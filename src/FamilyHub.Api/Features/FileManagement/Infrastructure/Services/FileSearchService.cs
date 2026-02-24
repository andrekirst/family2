using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Services;

/// <summary>
/// File search service using in-memory LINQ filtering.
/// Production implementation will be replaced with PostgreSQL FTS using
/// tsvector/tsquery, GIN indexes, pg_trgm for fuzzy matching,
/// and earth_distance for GPS proximity queries.
/// </summary>
public sealed class FileSearchService(
    IStoredFileRepository storedFileRepository,
    IFileTagRepository fileTagRepository,
    ITagRepository tagRepository,
    IFileMetadataRepository fileMetadataRepository) : IFileSearchService
{
    public async Task<List<FileSearchResultDto>> SearchAsync(
        string query,
        FamilyId familyId,
        SearchFiltersDto? filters = null,
        string sortBy = "relevance",
        int skip = 0,
        int take = 20,
        CancellationToken ct = default)
    {
        // Get all files for the family's folders
        // In production, this would be a PostgreSQL FTS query with tsvector
        var allFiles = await storedFileRepository.GetByFamilyIdAsync(familyId, ct);

        var queryLower = query.ToLowerInvariant();

        // Filter by search query (name matching)
        var matched = allFiles
            .Where(f => f.Name.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Apply faceted filters
        if (filters is not null)
        {
            if (filters.MimeTypes is { Count: > 0 })
                matched = matched.Where(f => filters.MimeTypes.Contains(f.MimeType.Value)).ToList();

            if (filters.DateFrom.HasValue)
                matched = matched.Where(f => f.CreatedAt >= filters.DateFrom.Value).ToList();

            if (filters.DateTo.HasValue)
                matched = matched.Where(f => f.CreatedAt <= filters.DateTo.Value).ToList();

            if (filters.FolderId.HasValue)
                matched = matched.Where(f => f.FolderId.Value == filters.FolderId.Value).ToList();

            if (filters.TagIds is { Count: > 0 })
            {
                var tagIds = filters.TagIds.Select(id => TagId.From(id)).ToList();
                var fileIdsWithTags = await fileTagRepository.GetFileIdsByTagIdsAsync(tagIds, ct);
                matched = matched.Where(f => fileIdsWithTags.Contains(f.Id)).ToList();
            }

            if (filters.GpsLatitude.HasValue && filters.GpsLongitude.HasValue && filters.GpsRadiusKm.HasValue)
            {
                var filesWithGps = new List<Domain.Entities.StoredFile>();
                foreach (var file in matched)
                {
                    var metadata = await fileMetadataRepository.GetByFileIdAsync(file.Id, ct);
                    if (metadata?.GpsLatitude is not null && metadata.GpsLongitude is not null)
                    {
                        var distance = HaversineDistance(
                            filters.GpsLatitude.Value, filters.GpsLongitude.Value,
                            (double)metadata.GpsLatitude.Value.Value, (double)metadata.GpsLongitude.Value.Value);

                        if (distance <= filters.GpsRadiusKm.Value)
                            filesWithGps.Add(file);
                    }
                }
                matched = filesWithGps;
            }
        }

        // Sort
        var sorted = sortBy.ToLowerInvariant() switch
        {
            "date" => matched.OrderByDescending(f => f.CreatedAt),
            "name" => matched.OrderBy(f => f.Name.Value),
            "size" => matched.OrderByDescending(f => f.Size.Value),
            _ => matched.OrderBy(f => // relevance: exact match first, then contains
                f.Name.Value.Equals(queryLower, StringComparison.OrdinalIgnoreCase) ? 0 :
                f.Name.Value.StartsWith(queryLower, StringComparison.OrdinalIgnoreCase) ? 1 : 2)
        };

        return sorted
            .Skip(skip)
            .Take(take)
            .Select(f => new FileSearchResultDto
            {
                Id = f.Id.Value,
                Name = f.Name.Value,
                MimeType = f.MimeType.Value,
                Size = f.Size.Value,
                FolderId = f.FolderId.Value,
                HighlightedName = HighlightMatch(f.Name.Value, query),
                Relevance = CalculateRelevance(f.Name.Value, query),
                CreatedAt = f.CreatedAt
            })
            .ToList();
    }

    private static string HighlightMatch(string text, string query)
    {
        var idx = text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return text;

        var before = text[..idx];
        var match = text.Substring(idx, query.Length);
        var after = text[(idx + query.Length)..];
        return $"{before}<b>{match}</b>{after}";
    }

    private static double CalculateRelevance(string text, string query)
    {
        if (text.Equals(query, StringComparison.OrdinalIgnoreCase)) return 1.0;
        if (text.StartsWith(query, StringComparison.OrdinalIgnoreCase)) return 0.8;
        if (text.Contains(query, StringComparison.OrdinalIgnoreCase)) return 0.5;
        return 0.0;
    }

    private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0; // Earth's radius in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
