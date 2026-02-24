using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFavorites;

public sealed class GetFavoritesQueryHandler(
    IUserFavoriteRepository userFavoriteRepository,
    IStoredFileRepository storedFileRepository)
    : IQueryHandler<GetFavoritesQuery, List<StoredFileDto>>
{
    public async ValueTask<List<StoredFileDto>> Handle(
        GetFavoritesQuery query,
        CancellationToken cancellationToken)
    {
        var favorites = await userFavoriteRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        if (favorites.Count == 0)
            return [];

        var fileIds = favorites.Select(f => f.FileId).ToList();
        var files = await storedFileRepository.GetByIdsAsync(fileIds, cancellationToken);

        return files.Select(FileManagementMapper.ToDto).ToList();
    }
}
