namespace FamilyHub.Modules.Family.Presentation.GraphQL.Namespaces;

/// <summary>
/// Namespace container for family-related queries.
/// Accessed via query { family { ... } }.
/// </summary>
/// <remarks>
/// <para>
/// This namespace contains queries for:
/// <list type="bullet">
/// <item><description>Family details and metadata</description></item>
/// <item><description>Family members list (with Connection pagination)</description></item>
/// <item><description>Pending invitations (with Connection pagination)</description></item>
/// <item><description>Family roles and permissions</description></item>
/// </list>
/// </para>
/// <para>
/// Most queries require the user to be a member of the family being queried.
/// </para>
/// </remarks>
public sealed record FamilyQueries;
