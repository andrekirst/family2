using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Tests.Unit.Builders;

/// <summary>
/// Builder for creating FamilyMemberInvitation test entities.
/// Uses the builder pattern for fluent, readable test data creation.
/// </summary>
public sealed class InvitationBuilder
{
    private FamilyId _familyId = FamilyId.New();
    private Email _email = Email.From("invite@example.com");
    private FamilyRole _role = FamilyRole.Member;
    private UserId _invitedByUserId = UserId.New();
    private string? _message;

    /// <summary>
    /// Sets the FamilyId for the invitation.
    /// </summary>
    public InvitationBuilder WithFamilyId(FamilyId familyId)
    {
        _familyId = familyId;
        return this;
    }

    /// <summary>
    /// Sets the email for the invitation.
    /// </summary>
    public InvitationBuilder WithEmail(Email email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the email for the invitation from a string.
    /// </summary>
    public InvitationBuilder WithEmail(string email)
    {
        _email = Email.From(email);
        return this;
    }

    /// <summary>
    /// Sets the role for the invitation.
    /// </summary>
    public InvitationBuilder WithRole(FamilyRole role)
    {
        _role = role;
        return this;
    }

    /// <summary>
    /// Sets the UserId of who sent the invitation.
    /// </summary>
    public InvitationBuilder WithInvitedByUserId(UserId invitedByUserId)
    {
        _invitedByUserId = invitedByUserId;
        return this;
    }

    /// <summary>
    /// Sets an optional message for the invitation.
    /// </summary>
    public InvitationBuilder WithMessage(string? message)
    {
        _message = message;
        return this;
    }

    /// <summary>
    /// Builds the FamilyMemberInvitation entity using the CreateEmailInvitation factory method.
    /// </summary>
    public FamilyMemberInvitation Build()
    {
        return FamilyMemberInvitation.CreateEmailInvitation(
            _familyId,
            _email,
            _role,
            _invitedByUserId,
            _message);
    }
}
