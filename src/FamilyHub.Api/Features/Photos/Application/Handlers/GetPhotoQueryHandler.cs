using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Photos.Application.Queries;
using FamilyHub.Api.Features.Photos.Domain.Repositories;
using FamilyHub.Api.Features.Photos.Models;

namespace FamilyHub.Api.Features.Photos.Application.Handlers;

public sealed class GetPhotoQueryHandler(
    IPhotoRepository repository)
    : IQueryHandler<GetPhotoQuery, PhotoDto?>
{
    public async ValueTask<PhotoDto?> Handle(
        GetPhotoQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(query.PhotoId.Value, cancellationToken);
    }
}
