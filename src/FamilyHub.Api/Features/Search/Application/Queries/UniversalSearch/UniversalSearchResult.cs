using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Search.Models;

namespace FamilyHub.Api.Features.Search.Application.Queries.UniversalSearch;

public sealed record UniversalSearchResult(
    IReadOnlyList<SearchResultItemDto> Results,
    IReadOnlyList<CommandDescriptor> Commands);
