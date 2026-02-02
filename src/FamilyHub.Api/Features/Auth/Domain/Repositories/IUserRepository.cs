using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Auth.Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate.
/// Abstracts data access from domain logic.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Get user by their unique identifier.
    /// </summary>
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);

    /// <summary>
    /// Get user by their OAuth provider external ID.
    /// </summary>
    Task<User?> GetByExternalIdAsync(ExternalUserId externalId, CancellationToken ct = default);

    /// <summary>
    /// Get user by their email address.
    /// </summary>
    Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default);

    /// <summary>
    /// Add a new user to the repository.
    /// </summary>
    Task AddAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Update an existing user.
    /// Note: EF Core tracks changes automatically, this is mainly for explicit saves.
    /// </summary>
    Task UpdateAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Save all pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
