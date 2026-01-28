namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;

/// <summary>
/// Namespace container for account-related mutations.
/// Accessed via mutation { account { ... } }.
/// </summary>
/// <remarks>
/// <para>
/// This namespace contains mutations for:
/// <list type="bullet">
/// <item><description>Profile updates (updateProfile)</description></item>
/// <item><description>Settings modifications</description></item>
/// <item><description>Invitation acceptance (acceptInvitation - modifies User aggregate)</description></item>
/// </list>
/// </para>
/// <para>
/// This namespace represents actions the authenticated user takes on their own account.
/// </para>
/// </remarks>
public sealed record AccountMutations;
