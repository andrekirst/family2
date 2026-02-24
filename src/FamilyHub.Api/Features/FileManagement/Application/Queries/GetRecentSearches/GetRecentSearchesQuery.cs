using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetRecentSearches;

public sealed record GetRecentSearchesQuery(UserId UserId) : IQuery<List<RecentSearchDto>>;
