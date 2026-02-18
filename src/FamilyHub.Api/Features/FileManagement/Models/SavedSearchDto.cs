namespace FamilyHub.Api.Features.FileManagement.Models;

public sealed record SavedSearchDto(
    Guid Id,
    string Name,
    string Query,
    string? FiltersJson,
    DateTime CreatedAt);
