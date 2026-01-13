using System.Linq.Expressions;
using FamilyHub.SharedKernel.Domain.Specifications;

namespace FamilyHub.Modules.Auth.Domain.Specifications;

/// <summary>
/// Specification for active (non-deleted) users.
/// </summary>
public sealed class ActiveUserSpecification : Specification<User>
{
    /// <inheritdoc/>
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.DeletedAt == null;
}
