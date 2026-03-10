using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Common.Contracts;

/// <summary>
/// Cross-module contract for family context operations.
/// Implemented by the Family module, consumed by other modules that need
/// to check family membership without depending on Family's domain directly.
/// </summary>
public interface IFamilyContext
{
    Task<bool> IsMemberAsync(FamilyId familyId, UserId userId, CancellationToken cancellationToken = default);
    Task<FamilyId?> GetUserFamilyIdAsync(UserId userId, CancellationToken cancellationToken = default);
}
