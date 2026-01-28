namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Namespaces;

/// <summary>
/// Namespace container for profile-related queries.
/// Accessed via query { account { profile { ... } } }.
/// </summary>
/// <remarks>
/// <para>
/// This namespace contains queries for:
/// <list type="bullet">
/// <item><description>Current user's profile (myProfile)</description></item>
/// <item><description>Other users' profiles (userProfile - visibility filtered)</description></item>
/// <item><description>Profile change requests (for children pending approval)</description></item>
/// <item><description>Pending approvals (for parents)</description></item>
/// </list>
/// </para>
/// <para>
/// Profile field visibility is controlled by the @visible directive based on
/// field-level visibility settings (HIDDEN, FAMILY, PUBLIC).
/// </para>
/// </remarks>
public sealed record ProfileQueries;
