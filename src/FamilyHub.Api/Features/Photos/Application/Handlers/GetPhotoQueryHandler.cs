using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Photos.Application.Mappers;
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
        var photo = await repository.GetByIdAsync(query.PhotoId, cancellationToken);
        return photo is not null ? PhotoMapper.ToDto(photo) : null;
    }
}
