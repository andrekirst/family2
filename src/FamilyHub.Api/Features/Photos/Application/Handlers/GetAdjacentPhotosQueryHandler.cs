using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Photos.Application.Queries;
using FamilyHub.Api.Features.Photos.Domain.Repositories;
using FamilyHub.Api.Features.Photos.Models;

namespace FamilyHub.Api.Features.Photos.Application.Handlers;

public sealed class GetAdjacentPhotosQueryHandler(
    IPhotoRepository repository)
    : IQueryHandler<GetAdjacentPhotosQuery, AdjacentPhotosDto>
{
    public async ValueTask<AdjacentPhotosDto> Handle(
        GetAdjacentPhotosQuery query,
        CancellationToken cancellationToken)
    {
        var previous = await repository.GetPreviousAsync(
            query.FamilyId, query.CurrentCreatedAt, query.CurrentPhotoId.Value, cancellationToken);
        var next = await repository.GetNextAsync(
            query.FamilyId, query.CurrentCreatedAt, query.CurrentPhotoId.Value, cancellationToken);

        return new AdjacentPhotosDto
        {
            Previous = previous,
            Next = next
        };
    }
}
