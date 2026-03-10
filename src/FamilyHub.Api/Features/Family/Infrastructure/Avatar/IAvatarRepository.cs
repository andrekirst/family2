using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Infrastructure.Avatar;

/// <summary>
/// Repository for Avatar aggregate persistence.
/// </summary>
public interface IAvatarRepository : IWriteRepository<AvatarAggregate, AvatarId>
{
    Task DeleteAsync(AvatarId id, CancellationToken cancellationToken = default);
}
