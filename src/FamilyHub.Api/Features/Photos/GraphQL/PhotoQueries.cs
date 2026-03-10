using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Photos.Application.Queries;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Models;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Photos.GraphQL;

[ExtendObjectType(typeof(FamilyQuery))]
public class PhotoQueries
{
    [Authorize]
    public async Task<PhotosPageDto> GetPhotos(
        int skip,
        int take,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetPhotosQuery(skip, Math.Min(take, 30));
        return await queryBus.QueryAsync(query, cancellationToken);
    }

    [Authorize]
    public async Task<PhotoDto?> GetPhoto(
        Guid id,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetPhotoQuery(PhotoId.From(id));
        return await queryBus.QueryAsync(query, cancellationToken);
    }

    [Authorize]
    public async Task<AdjacentPhotosDto> GetAdjacentPhotos(
        Guid currentPhotoId,
        DateTime currentCreatedAt,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetAdjacentPhotosQuery(
            PhotoId.From(currentPhotoId),
            currentCreatedAt);

        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
