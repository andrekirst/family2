using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Photos.Domain.Repositories;

namespace FamilyHub.Api.Features.Photos.Application.Search;

public sealed class PhotosSearchProvider(IPhotoRepository photoRepository) : ISearchProvider
{
    public string ModuleName => "photos";

    public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        SearchContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.FamilyId is null || string.IsNullOrWhiteSpace(context.Query))
        {
            return [];
        }

        // PhotoRepository already filters out deleted photos
        var photos = await photoRepository.GetByFamilyAsync(
            context.FamilyId.Value, skip: 0, take: 100, cancellationToken);

        var queryLower = context.Query.ToLowerInvariant();
        var isGerman = context.IsLocale("de");

        return photos
            .Where(p => p.FileName.Contains(queryLower, StringComparison.OrdinalIgnoreCase) ||
                        (p.Caption?.Contains(queryLower, StringComparison.OrdinalIgnoreCase) == true))
            .OrderByDescending(p => p.CreatedAt)
            .Take(context.Limit)
            .Select(p => new SearchResultItem(
                Title: p.Caption ?? p.FileName,
                Description: FormatUploadDate(p.CreatedAt, isGerman),
                Module: "photos",
                Icon: "camera",
                Route: $"/files?photo={p.Id}"))
            .ToList()
            .AsReadOnly();
    }

    private static string FormatUploadDate(DateTime createdAt, bool isGerman) =>
        isGerman
            ? $"Hochgeladen am {createdAt:dd. MMM}"
            : $"Uploaded {createdAt:MMM dd}";
}
