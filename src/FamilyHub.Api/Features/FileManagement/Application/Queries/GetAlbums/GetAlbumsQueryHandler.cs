using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetAlbums;

public sealed class GetAlbumsQueryHandler(
    IAlbumRepository albumRepository,
    IAlbumItemRepository albumItemRepository)
    : IQueryHandler<GetAlbumsQuery, List<AlbumDto>>
{
    public async ValueTask<List<AlbumDto>> Handle(
        GetAlbumsQuery query,
        CancellationToken cancellationToken)
    {
        var albums = await albumRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);

        var result = new List<AlbumDto>();
        foreach (var album in albums)
        {
            var itemCount = await albumItemRepository.GetItemCountAsync(album.Id, cancellationToken);
            result.Add(FileManagementMapper.ToDto(album, itemCount));
        }

        return result;
    }
}
