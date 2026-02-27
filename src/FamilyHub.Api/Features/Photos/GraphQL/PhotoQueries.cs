using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Photos.Application.Queries;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Models;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Photos.GraphQL;

[ExtendObjectType(typeof(FamilyQuery))]
public class PhotoQueries
{
    [Authorize]
    public async Task<PhotosPageDto> GetPhotos(
        Guid familyId,
        int skip,
        int take,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var query = new GetPhotosQuery(
            FamilyId.From(familyId),
            skip,
            Math.Min(take, 30));

        return await queryBus.QueryAsync<PhotosPageDto>(query, ct);
    }

    [Authorize]
    public async Task<PhotoDto?> GetPhoto(
        Guid id,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var query = new GetPhotoQuery(PhotoId.From(id));
        return await queryBus.QueryAsync<PhotoDto?>(query, ct);
    }

    [Authorize]
    public async Task<AdjacentPhotosDto> GetAdjacentPhotos(
        Guid familyId,
        Guid currentPhotoId,
        DateTime currentCreatedAt,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var query = new GetAdjacentPhotosQuery(
            FamilyId.From(familyId),
            PhotoId.From(currentPhotoId),
            currentCreatedAt);

        return await queryBus.QueryAsync<AdjacentPhotosDto>(query, ct);
    }
}
