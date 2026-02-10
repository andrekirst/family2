namespace FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;

/// <summary>
/// Namespace type for invitation mutations.
/// ID-based operations (accept, decline, revoke) require InvitationId from parent.
/// Token-based operations (acceptByToken, declineByToken) ignore it.
/// </summary>
public class FamilyInvitationMutation
{
    public FamilyInvitationMutation() { }

    public FamilyInvitationMutation(Guid? invitationId)
    {
        InvitationId = invitationId;
    }

    [GraphQLIgnore]
    public Guid? InvitationId { get; }
}
