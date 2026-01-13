using System.Linq.Expressions;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Domain.Specifications;

/// <summary>
/// Specification for finding active families owned by a specific user.
/// </summary>
/// <param name="ownerId">The owner's user ID.</param>
public sealed class FamilyByOwnerSpecification(UserId ownerId) : Specification<Aggregates.Family>
{
    /// <inheritdoc/>
    public override Expression<Func<Aggregates.Family, bool>> ToExpression()
        => family => family.OwnerId == ownerId && family.DeletedAt == null;
}
