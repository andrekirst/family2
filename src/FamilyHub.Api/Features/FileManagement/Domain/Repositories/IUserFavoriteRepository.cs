using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IUserFavoriteRepository
{
    Task<List<Entities.UserFavorite>> GetByUserIdAsync(UserId userId, CancellationToken ct = default);
    Task<bool> ExistsAsync(UserId userId, FileId fileId, CancellationToken ct = default);
    Task AddAsync(Entities.UserFavorite favorite, CancellationToken ct = default);
    Task RemoveAsync(Entities.UserFavorite favorite, CancellationToken ct = default);
}
