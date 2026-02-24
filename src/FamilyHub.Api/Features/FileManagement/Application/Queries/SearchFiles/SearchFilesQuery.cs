using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.SearchFiles;

public sealed record SearchFilesQuery(
    string Query,
    FamilyId FamilyId,
    UserId UserId,
    SearchFiltersDto? Filters = null,
    string SortBy = "relevance",
    int Skip = 0,
    int Take = 20
) : IQuery<List<FileSearchResultDto>>;
