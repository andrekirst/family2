using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Family.Domain.Events;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyMemberInvitationAggregate = FamilyHub.Modules.Family.Domain.Aggregates.FamilyMemberInvitation;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using FamilyHub.Modules.Family.Domain.ValueObjects;

namespace FamilyHub.Tests.Unit.Auth.Domain;

/// <summary>
/// Unit tests for FamilyMemberInvitationAggregate aggregate.
/// Tests domain logic, validation rules, and business invariants.
/// </summary>
public class FamilyMemberInvitationTests
{
    #region CreateEmailInvitation Tests

    [Fact]
    public void CreateEmailInvitation_WithValidParameters_ShouldCreateInvitation()
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("john@example.com");
        var role = FamilyRole.Member;
        var invitedBy = UserId.New();

        // Act
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyId, email, role, invitedBy);

        // Assert
        invitation.Should().NotBeNull();
        invitation.Id.Value.Should().NotBe(Guid.Empty);
        invitation.FamilyId.Should().Be(familyId);
        invitation.Email.Should().Be(email);
        invitation.Role.Should().Be(role);
        invitation.InvitedByUserId.Should().Be(invitedBy);
        invitation.Status.Should().Be(InvitationStatus.Pending);
        invitation.Token.Value.Should().HaveLength(64);
        invitation.DisplayCode.Value.Should().HaveLength(8);
        invitation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromSeconds(5));
        invitation.AcceptedAt.Should().BeNull();
    }

    [Fact]
    public void CreateEmailInvitation_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("john@example.com");
        var role = FamilyRole.Member;
        var invitedBy = UserId.New();
        var message = "Welcome to the family!";

        // Act
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyId, email, role, invitedBy, message);

        // Assert
        invitation.Message.Should().Be(message);
    }

    [Fact]
    public void CreateEmailInvitation_ShouldPublishFamilyMemberInvitedEvent()
    {
        // Arrange
        var familyId = FamilyId.New();
        var email = Email.From("john@example.com");
        var role = FamilyRole.Member;
        var invitedBy = UserId.New();

        // Act
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyId, email, role, invitedBy);

        // Assert
        invitation.DomainEvents.Should().HaveCount(1);
        var domainEvent = invitation.DomainEvents.First() as FamilyMemberInvitedEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.EventVersion.Should().Be(1);
        domainEvent.InvitationId.Should().Be(invitation.Id);
        domainEvent.FamilyId.Should().Be(familyId);
        domainEvent.Email.Should().Be(email);
        domainEvent.Role.Should().Be(role);
        domainEvent.Token.Should().Be(invitation.Token);
        domainEvent.ExpiresAt.Should().Be(invitation.ExpiresAt);
        domainEvent.InvitedByUserId.Should().Be(invitedBy);
        domainEvent.IsResend.Should().BeFalse();
    }

    #endregion


    #region Accept Tests

    [Fact]
    public void Accept_WithPendingInvitation_ShouldMarkAsAccepted()
    {
        // Arrange
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            FamilyId.New(), Email.From("test@example.com"), FamilyRole.Member, UserId.New());
        var userId = UserId.New();
        invitation.ClearDomainEvents(); // Clear creation event

        // Act
        invitation.Accept(userId);

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Accepted);
        invitation.AcceptedAt.Should().NotBeNull();
        invitation.AcceptedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Accept_WithPendingInvitation_ShouldPublishInvitationAcceptedEvent()
    {
        // Arrange
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            FamilyId.New(), Email.From("test@example.com"), FamilyRole.Member, UserId.New());
        var userId = UserId.New();
        invitation.ClearDomainEvents();

        // Act
        invitation.Accept(userId);

        // Assert
        invitation.DomainEvents.Should().HaveCount(1);
        var domainEvent = invitation.DomainEvents.First() as InvitationAcceptedEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.EventVersion.Should().Be(1);
        domainEvent.InvitationId.Should().Be(invitation.Id);
        domainEvent.FamilyId.Should().Be(invitation.FamilyId);
        domainEvent.UserId.Should().Be(userId);
        domainEvent.AcceptedAt.Should().Be(invitation.AcceptedAt!.Value);
    }

    [Fact]
    public void Accept_WithAlreadyAcceptedInvitation_ShouldThrow()
    {
        // Arrange
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            FamilyId.New(), Email.From("test@example.com"), FamilyRole.Member, UserId.New());
        invitation.Accept(UserId.New());

        // Act
        var act = () => invitation.Accept(UserId.New());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot accept invitation in accepted status*");
    }

    [Fact]
    public void Accept_WithCanceledInvitation_ShouldThrow()
    {
        // Arrange
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            FamilyId.New(), Email.From("test@example.com"), FamilyRole.Member, UserId.New());
        invitation.Cancel(UserId.New());

        // Act
        var act = () => invitation.Accept(UserId.New());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot accept invitation in canceled status*");
    }

    // NOTE: Expiration validation moved to AcceptInvitationCommandValidator
    // The domain Accept() method now only checks status (defensive programming)
    // See AcceptInvitationCommandValidatorTests.Validate_WithExpiredInvitation_ShouldFailWithExpirationMessage

    #endregion

    #region Cancel Tests

    [Fact]
    public void Cancel_WithPendingInvitation_ShouldMarkAsCanceled()
    {
        // Arrange
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            FamilyId.New(), Email.From("test@example.com"), FamilyRole.Member, UserId.New());
        var canceledBy = UserId.New();
        invitation.ClearDomainEvents();

        // Act
        invitation.Cancel(canceledBy);

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Canceled);
    }

    [Fact]
    public void Cancel_WithPendingInvitation_ShouldPublishInvitationCanceledEvent()
    {
        // Arrange
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            FamilyId.New(), Email.From("test@example.com"), FamilyRole.Member, UserId.New());
        var canceledBy = UserId.New();
        invitation.ClearDomainEvents();

        // Act
        invitation.Cancel(canceledBy);

        // Assert
        invitation.DomainEvents.Should().HaveCount(1);
        var domainEvent = invitation.DomainEvents.First() as InvitationCanceledEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.EventVersion.Should().Be(1);
        domainEvent.InvitationId.Should().Be(invitation.Id);
        domainEvent.FamilyId.Should().Be(invitation.FamilyId);
        domainEvent.CanceledByUserId.Should().Be(canceledBy);
        domainEvent.CanceledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Cancel_WithAcceptedInvitation_ShouldThrow()
    {
        // Arrange
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            FamilyId.New(), Email.From("test@example.com"), FamilyRole.Member, UserId.New());
        invitation.Accept(UserId.New());

        // Act
        var act = () => invitation.Cancel(UserId.New());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot cancel invitation in accepted status*");
    }

    [Fact]
    public void Cancel_WithAlreadyCanceledInvitation_ShouldThrow()
    {
        // Arrange
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            FamilyId.New(), Email.From("test@example.com"), FamilyRole.Member, UserId.New());
        invitation.Cancel(UserId.New());

        // Act
        var act = () => invitation.Cancel(UserId.New());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot cancel invitation in canceled status*");
    }

    #endregion

    #region Resend Tests

    [Fact]
    public void Resend_WithPendingEmailInvitation_ShouldGenerateNewTokenAndExtendExpiration()
    {
        // Arrange
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            FamilyId.New(), Email.From("test@example.com"), FamilyRole.Member, UserId.New());
        var originalToken = invitation.Token;
        var originalExpiresAt = invitation.ExpiresAt;
        invitation.ClearDomainEvents();

        // Act
        invitation.Resend(UserId.New());

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Pending);
        invitation.Token.Should().NotBe(originalToken);
        invitation.ExpiresAt.Should().BeAfter(originalExpiresAt);
        invitation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Resend_WithExpiredEmailInvitation_ShouldResetToPending()
    {
        // Arrange
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            FamilyId.New(), Email.From("test@example.com"), FamilyRole.Member, UserId.New());

        // Use reflection to set status and expiration
        var statusProperty = typeof(FamilyMemberInvitationAggregate).GetProperty("Status");
        statusProperty!.SetValue(invitation, InvitationStatus.Expired);

        invitation.ClearDomainEvents();

        // Act
        invitation.Resend(UserId.New());

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Pending);
    }

    [Fact]
    public void Resend_WithEmailInvitation_ShouldPublishFamilyMemberInvitedEventWithResendFlag()
    {
        // Arrange
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            FamilyId.New(), Email.From("test@example.com"), FamilyRole.Member, UserId.New());
        var resentBy = UserId.New();
        invitation.ClearDomainEvents();

        // Act
        invitation.Resend(resentBy);

        // Assert
        invitation.DomainEvents.Should().HaveCount(1);
        var domainEvent = invitation.DomainEvents.First() as FamilyMemberInvitedEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.IsResend.Should().BeTrue();
        domainEvent.InvitedByUserId.Should().Be(resentBy);
    }


    [Fact]
    public void Resend_WithAcceptedInvitation_ShouldThrow()
    {
        // Arrange
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            FamilyId.New(), Email.From("test@example.com"), FamilyRole.Member, UserId.New());
        invitation.Accept(UserId.New());

        // Act
        var act = () => invitation.Resend(UserId.New());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot resend invitation in accepted status*");
    }

    [Fact]
    public void Resend_WithCanceledInvitation_ShouldThrow()
    {
        // Arrange
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            FamilyId.New(), Email.From("test@example.com"), FamilyRole.Member, UserId.New());
        invitation.Cancel(UserId.New());

        // Act
        var act = () => invitation.Resend(UserId.New());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot resend invitation in canceled status*");
    }

    #endregion

}
