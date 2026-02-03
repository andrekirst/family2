using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.Api.Features.Family.Domain.Repositories;

/// <summary>
/// Repository interface for Family aggregate.
/// Abstracts data access from domain logic.
/// </summary>
public interface IFamilyRepository
{
    /// <summary>
    /// Get family by its unique identifier.
    /// </summary>
    Task<FamilyEntity?> GetByIdAsync(FamilyId id, CancellationToken ct = default);

    /// <summary>
    /// Get family by its unique identifier with members included.
    /// </summary>
    Task<FamilyEntity?> GetByIdWithMembersAsync(FamilyId id, CancellationToken ct = default);

    /// <summary>
    /// Get family by owner ID.
    /// </summary>
    Task<FamilyEntity?> GetByOwnerIdAsync(UserId ownerId, CancellationToken ct = default);

    /// <summary>
    /// Check if a user already owns a family.
    /// </summary>
    Task<bool> UserHasFamilyAsync(UserId userId, CancellationToken ct = default);

    /// <summary>
    /// Add a new family to the repository.
    /// </summary>
    Task AddAsync(FamilyEntity family, CancellationToken ct = default);

    /// <summary>
    /// Update an existing family.
    /// </summary>
    Task UpdateAsync(FamilyEntity family, CancellationToken ct = default);

    /// <summary>
    /// Save all pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
