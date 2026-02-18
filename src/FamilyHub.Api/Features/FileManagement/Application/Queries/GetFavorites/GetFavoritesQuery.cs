using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFavorites;

public sealed record GetFavoritesQuery(
    UserId UserId
) : IQuery<List<StoredFileDto>>;
