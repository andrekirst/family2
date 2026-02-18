namespace FamilyHub.Api.Features.FileManagement.Models;

public sealed record FileSearchResultDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string MimeType { get; init; } = null!;
    public long Size { get; init; }
    public Guid FolderId { get; init; }
    public string? HighlightedName { get; init; }
    public double? Relevance { get; init; }
    public DateTime CreatedAt { get; init; }
}
