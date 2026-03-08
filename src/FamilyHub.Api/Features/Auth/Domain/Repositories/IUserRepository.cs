using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;

namespace FamilyHub.Api.Features.Auth.Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate.
/// Abstracts data access from domain logic.
/// </summary>
public interface IUserRepository : IWriteRepository<User, UserId>
{
    /// <summary>
    /// Get user by their OAuth provider external ID.
    /// </summary>
    Task<User?> GetByExternalIdAsync(ExternalUserId externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by their email address.
    /// </summary>
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing user.
    /// Note: EF Core tracks changes automatically, this is mainly for explicit saves.
    /// </summary>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
