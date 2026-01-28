namespace FamilyHub.Modules.Family.Presentation.GraphQL.Namespaces;

/// <summary>
/// Namespace container for family-related mutations.
/// Accessed via mutation { family { ... } }.
/// </summary>
/// <remarks>
/// <para>
/// This namespace contains mutations for:
/// <list type="bullet">
/// <item><description>Family creation (createFamily)</description></item>
/// <item><description>Family member invitations (inviteMember, cancelInvitation, resendInvitation)</description></item>
/// <item><description>Invitation role management (updateInvitationRole)</description></item>
/// <item><description>Family settings and metadata updates</description></item>
/// </list>
/// </para>
/// <para>
/// Note: acceptInvitation is in AccountMutations as it modifies the User aggregate.
/// </para>
/// <para>
/// All mutations use HotChocolate mutation conventions for consistent error handling.
/// </para>
/// </remarks>
public sealed record FamilyMutations;
