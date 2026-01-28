namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Namespaces;

/// <summary>
/// Namespace container for profile-related mutations.
/// Accessed via mutation { account { profile { ... } } }.
/// </summary>
/// <remarks>
/// <para>
/// This namespace contains mutations for:
/// <list type="bullet">
/// <item><description>Profile updates (updateProfile) - may require approval for children</description></item>
/// <item><description>Profile change approval (approveChange) - for parents</description></item>
/// <item><description>Profile change rejection (rejectChange) - for parents</description></item>
/// <item><description>Field visibility settings</description></item>
/// </list>
/// </para>
/// <para>
/// Child profiles have restricted mutation capabilities with parental approval workflow.
/// </para>
/// <para>
/// All mutations use HotChocolate mutation conventions for consistent error handling.
/// </para>
/// </remarks>
public sealed record ProfileMutations;
