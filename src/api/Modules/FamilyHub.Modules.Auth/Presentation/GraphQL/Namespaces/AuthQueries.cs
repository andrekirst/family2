namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;

/// <summary>
/// Namespace container for authentication-related queries.
/// Accessed via query { auth { ... } }.
/// </summary>
/// <remarks>
/// <para>
/// This namespace contains queries for:
/// <list type="bullet">
/// <item><description>Password validation and generation helpers</description></item>
/// <item><description>Role metadata and permissions</description></item>
/// <item><description>Authentication status checks</description></item>
/// </list>
/// </para>
/// <para>
/// Note: The "me" query is in AccountQueries, not here, as it represents
/// the authenticated user's account context rather than authentication operations.
/// </para>
/// </remarks>
public sealed record AuthQueries;
