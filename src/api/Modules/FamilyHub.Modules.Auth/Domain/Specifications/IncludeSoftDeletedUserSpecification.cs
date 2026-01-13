using System.Linq.Expressions;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Specifications;

/// <summary>
/// Specification for finding a user by email, including soft-deleted users.
/// Use for admin operations or cleanup scenarios.
/// </summary>
/// <param name="email">The email address to search for.</param>
public sealed class IncludeSoftDeletedUserSpecification(Email email) : Specification<User>
{
    /// <inheritdoc/>
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.Email == email;

    /// <summary>
    /// Ignores EF Core global query filters (soft-delete).
    /// </summary>
    public override bool IgnoreQueryFilters => true;
}
