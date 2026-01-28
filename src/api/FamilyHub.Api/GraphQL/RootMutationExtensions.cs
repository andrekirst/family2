using FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;
using FamilyHub.Modules.Family.Presentation.GraphQL.Namespaces;
using HotChocolate.Authorization;
using HotChocolate.Types;

namespace FamilyHub.Api.GraphQL;

/// <summary>
/// Extends the root Mutation type with namespace entry points.
/// </summary>
/// <remarks>
/// <para>
/// This extension creates the nested namespace structure:
/// <code>
/// mutation {
///   auth { ... }      # Authentication mutations (login, register, etc.)
///   account { ... }   # Account mutations (profile, settings)
///   family { ... }    # Family mutations (create, invite, etc.)
/// }
/// </code>
/// </para>
/// <para>
/// The namespace containers are empty records. Actual mutations are added
/// via separate type extensions in each module.
/// </para>
/// <para>
/// All mutations use HotChocolate mutation conventions for consistent error handling.
/// </para>
/// </remarks>
[ExtendObjectType("Mutation")]
public sealed class RootMutationExtensions
{
    /// <summary>
    /// Entry point for authentication-related mutations.
    /// Does not require authentication (login/register are pre-auth).
    /// </summary>
    /// <returns>The auth mutations namespace container.</returns>
    public AuthMutations Auth() => new();

    /// <summary>
    /// Entry point for account-related mutations (profile, settings).
    /// Requires authentication.
    /// </summary>
    /// <returns>The account mutations namespace container.</returns>
    [Authorize]
    public AccountMutations Account() => new();

    /// <summary>
    /// Entry point for family-related mutations.
    /// Requires authentication.
    /// </summary>
    /// <returns>The family mutations namespace container.</returns>
    [Authorize]
    public FamilyMutations Family() => new();
}
