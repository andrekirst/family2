namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;

/// <summary>
/// Namespace container for account-related queries.
/// Accessed via query { account { ... } }.
/// </summary>
/// <remarks>
/// <para>
/// This namespace contains queries for:
/// <list type="bullet">
/// <item><description>Current user info (me)</description></item>
/// <item><description>User profile data (profile)</description></item>
/// <item><description>User settings and preferences</description></item>
/// <item><description>Family membership info</description></item>
/// </list>
/// </para>
/// <para>
/// This namespace represents the authenticated user's context and groups
/// all queries related to "what the user can see about themselves".
/// </para>
/// </remarks>
public sealed record AccountQueries;
