using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Common.Contracts;

/// <summary>
/// Cross-module contract for user lookup operations.
/// Implemented by the Auth module, consumed by other modules that need
/// to resolve users without depending on Auth's domain entities directly.
/// </summary>
public interface IUserLookup
{
    Task<UserInfo?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<UserInfo?> GetByExternalIdAsync(ExternalUserId externalId, CancellationToken cancellationToken = default);
    Task<UserInfo?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
}

/// <summary>
/// Read-only projection of a user for cross-module consumption.
/// Contains only the data other modules need, not the full aggregate.
/// </summary>
public sealed record UserInfo(
    UserId Id,
    ExternalUserId ExternalId,
    Email Email,
    string DisplayName,
    FamilyId? FamilyId);
