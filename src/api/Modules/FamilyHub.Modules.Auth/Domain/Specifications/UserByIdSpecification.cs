using System.Linq.Expressions;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Specifications;

/// <summary>
/// Specification for finding a user by ID.
/// Replaces: GetByIdAsync(UserId)
/// </summary>
/// <param name="id">The user ID to search for.</param>
public sealed class UserByIdSpecification(UserId id) : Specification<User>
{
    /// <inheritdoc/>
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.Id == id;
}
