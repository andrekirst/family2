namespace FamilyHub.Api.Features.Search.Models;

public sealed record SearchResultItemDto(
    string Title,
    string? Description,
    string Module,
    string Icon,
    string Route);
