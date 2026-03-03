using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Photos.Application.Queries;
using FamilyHub.Api.Features.Photos.Domain.Repositories;
using FamilyHub.Api.Features.Photos.Models;

namespace FamilyHub.Api.Features.Photos.Application.Handlers;

public sealed class GetPhotosQueryHandler(
    IPhotoRepository repository)
    : IQueryHandler<GetPhotosQuery, PhotosPageDto>
{
    public async ValueTask<PhotosPageDto> Handle(
        GetPhotosQuery query,
        CancellationToken cancellationToken)
    {
        var photos = await repository.GetByFamilyAsync(
            query.FamilyId, query.Skip, query.Take, cancellationToken);
        var totalCount = await repository.GetCountByFamilyAsync(
            query.FamilyId, cancellationToken);

        return new PhotosPageDto
        {
            Items = photos,
            TotalCount = totalCount,
            HasMore = query.Skip + query.Take < totalCount
        };
    }
}
