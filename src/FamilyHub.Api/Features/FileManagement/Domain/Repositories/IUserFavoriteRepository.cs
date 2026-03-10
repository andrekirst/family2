using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IUserFavoriteRepository
{
    Task<List<Entities.UserFavorite>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(UserId userId, FileId fileId, CancellationToken cancellationToken = default);
    Task AddAsync(Entities.UserFavorite favorite, CancellationToken cancellationToken = default);
    Task RemoveAsync(Entities.UserFavorite favorite, CancellationToken cancellationToken = default);
}
