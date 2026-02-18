using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetSavedSearches;

public sealed record GetSavedSearchesQuery(UserId UserId) : IQuery<List<SavedSearchDto>>;
