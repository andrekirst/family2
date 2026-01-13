using System.Linq.Expressions;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Specifications;

/// <summary>
/// Specification for finding an active user by email.
/// Replaces: GetByEmailAsync(Email)
/// </summary>
/// <param name="email">The email address to search for.</param>
public sealed class UserByEmailSpecification(Email email) : Specification<User>
{
    /// <inheritdoc/>
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.Email == email && user.DeletedAt == null;
}
