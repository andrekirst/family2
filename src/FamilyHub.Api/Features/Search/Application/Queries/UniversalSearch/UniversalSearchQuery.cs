using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Search.Application.Queries.UniversalSearch;

public sealed record UniversalSearchQuery(
    string Query,
    string[]? Modules = null,
    int Limit = 10,
    string[]? UserPermissions = null,
    string? Locale = null
) : IReadOnlyQuery<UniversalSearchResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
