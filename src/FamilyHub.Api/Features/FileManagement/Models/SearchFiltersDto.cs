namespace FamilyHub.Api.Features.FileManagement.Models;

/// <summary>
/// Faceted filter criteria for file search.
/// </summary>
public sealed record SearchFiltersDto
{
    public List<string>? MimeTypes { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public List<Guid>? TagIds { get; init; }
    public Guid? FolderId { get; init; }
    public double? GpsLatitude { get; init; }
    public double? GpsLongitude { get; init; }
    public double? GpsRadiusKm { get; init; }
}
