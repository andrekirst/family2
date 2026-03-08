using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;

namespace FamilyHub.Api.Features.FileManagement.Application.Search;

public sealed class FileManagementSearchProvider(
    IStoredFileRepository storedFileRepository,
    IFolderRepository folderRepository,
    IAlbumRepository albumRepository,
    ITagRepository tagRepository,
    ISecureNoteRepository secureNoteRepository,
    IShareLinkRepository shareLinkRepository) : ISearchProvider
{
    public string ModuleName => "files";

    public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        SearchContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.FamilyId is null || string.IsNullOrWhiteSpace(context.Query))
        {
            return [];
        }

        var queryLower = context.Query.ToLowerInvariant();
        var isGerman = context.IsLocale("de");
        var results = new List<SearchResultItem>();
        var familyId = context.FamilyId.Value;

        // Search stored files
        var files = await storedFileRepository.GetByFamilyIdAsync(familyId, cancellationToken);
        results.AddRange(files
            .Where(f => f.Name.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
            .Take(context.Limit)
            .Select(f => new SearchResultItem(
                Title: f.Name.Value,
                Description: f.MimeType.Value,
                Module: "files",
                Icon: "file",
                Route: $"/files/browse?file={f.Id.Value}")));

        // Search folders via root + descendants
        var rootFolder = await folderRepository.GetRootFolderAsync(familyId, cancellationToken);
        if (rootFolder is not null)
        {
            var folders = await folderRepository.GetDescendantsAsync(
                rootFolder.MaterializedPath, familyId, cancellationToken);
            results.AddRange(folders
                .Where(f => f.Name.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
                .Take(context.Limit)
                .Select(f => new SearchResultItem(
                    Title: f.Name.Value,
                    Description: isGerman ? "Ordner" : "Folder",
                    Module: "files",
                    Icon: "folder",
                    Route: $"/files/browse?folder={f.Id.Value}")));
        }

        // Search albums
        var albums = await albumRepository.GetByFamilyIdAsync(familyId, cancellationToken);
        results.AddRange(albums
            .Where(a => a.Name.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase) ||
                        (a.Description?.Contains(queryLower, StringComparison.OrdinalIgnoreCase) == true))
            .Take(context.Limit)
            .Select(a => new SearchResultItem(
                Title: a.Name.Value,
                Description: a.Description ?? (isGerman ? "Album" : "Album"),
                Module: "files",
                Icon: "image",
                Route: $"/files/albums/{a.Id.Value}")));

        // Search tags
        var tags = await tagRepository.GetByFamilyIdAsync(familyId, cancellationToken);
        results.AddRange(tags
            .Where(t => t.Name.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
            .Take(context.Limit)
            .Select(t => new SearchResultItem(
                Title: t.Name.Value,
                Description: isGerman ? "Schlagwort" : "Tag",
                Module: "files",
                Icon: "tag",
                Route: $"/files/browse?tag={t.Id.Value}")));

        // Search secure notes by category only (content is encrypted)
        var notes = await secureNoteRepository.GetByUserIdAsync(
            context.UserId, familyId, cancellationToken);
        results.AddRange(notes
            .Where(n => n.Category.ToString().Contains(queryLower, StringComparison.OrdinalIgnoreCase))
            .Take(context.Limit)
            .Select(n => new SearchResultItem(
                Title: n.Category.ToString(),
                Description: isGerman ? "Sichere Notiz" : "Secure note",
                Module: "files",
                Icon: "lock",
                Route: $"/files/notes?note={n.Id.Value}")));

        // Search share links (active only)
        var shareLinks = await shareLinkRepository.GetByFamilyIdAsync(familyId, cancellationToken);
        results.AddRange(shareLinks
            .Where(l => l.IsAccessible)
            .Where(l => l.ResourceType.ToString().Contains(queryLower, StringComparison.OrdinalIgnoreCase))
            .Take(context.Limit)
            .Select(l => new SearchResultItem(
                Title: $"{l.ResourceType} share link",
                Description: isGerman ? "Aktiver Freigabelink" : "Active share link",
                Module: "files",
                Icon: "link",
                Route: $"/files/sharing?link={l.Id.Value}")));

        return results
            .Take(context.Limit)
            .ToList()
            .AsReadOnly();
    }
}
