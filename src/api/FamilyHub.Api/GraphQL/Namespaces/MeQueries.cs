namespace FamilyHub.Api.GraphQL.Namespaces;

/// <summary>
/// Namespace container for user-centric "me" queries.
/// Accessed via query { me { ... } }.
/// </summary>
/// <remarks>
/// <para>
/// This namespace consolidates all queries related to the current authenticated user:
/// <list type="bullet">
/// <item><description>profile - User's own profile with audit info and pending changes</description></item>
/// <item><description>family - User's family (or reason why they have no family)</description></item>
/// <item><description>pendingInvitations - Invitations received by the user</description></item>
/// </list>
/// </para>
/// <para>
/// All queries in this namespace require authentication.
/// </para>
/// </remarks>
public sealed record MeQueries;
