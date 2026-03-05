namespace FamilyHub.Api.Features.Search.Models;

public sealed record UniversalSearchRequest(
    string Query,
    string[]? Modules = null,
    int? Limit = null,
    string? Locale = null);
