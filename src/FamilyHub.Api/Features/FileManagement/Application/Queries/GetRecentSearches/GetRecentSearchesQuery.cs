using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetRecentSearches;

public sealed record GetRecentSearchesQuery(UserId UserId, FamilyId FamilyId) : IReadOnlyQuery<List<RecentSearchDto>>, IFamilyScoped;
