using System.Linq.Expressions;
using FamilyHub.SharedKernel.Domain.Specifications;

namespace FamilyHub.Modules.Auth.Domain.Specifications;

/// <summary>
/// Specification for finding a user by external OAuth provider and user ID.
/// Replaces: GetByExternalProviderAsync(string, string)
/// </summary>
/// <param name="externalProvider">The OAuth provider name (e.g., "zitadel").</param>
/// <param name="externalUserId">The external user ID from the provider.</param>
public sealed class UserByExternalProviderSpecification(string externalProvider, string externalUserId)
    : Specification<User>
{
    private readonly string _externalProvider = externalProvider ?? throw new ArgumentNullException(nameof(externalProvider));
    private readonly string _externalUserId = externalUserId ?? throw new ArgumentNullException(nameof(externalUserId));

    /// <inheritdoc/>
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.ExternalProvider == _externalProvider
                && user.ExternalUserId == _externalUserId
                && user.DeletedAt == null;
}
