using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Common.Infrastructure.Avatar;

/// <summary>
/// Repository for Avatar aggregate persistence.
/// </summary>
public interface IAvatarRepository
{
    Task<AvatarAggregate?> GetByIdAsync(AvatarId id, CancellationToken ct = default);
    Task AddAsync(AvatarAggregate avatar, CancellationToken ct = default);
    Task DeleteAsync(AvatarId id, CancellationToken ct = default);
}
