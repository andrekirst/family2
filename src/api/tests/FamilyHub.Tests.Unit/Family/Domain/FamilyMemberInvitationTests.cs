using FamilyHub.Modules.Family.Domain;
using FamilyHub.Modules.Family.Domain.Events;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FamilyHub.Tests.Unit.Family.Domain;

/// <summary>
/// Unit tests for the FamilyMemberInvitation aggregate root.
/// Tests invitation lifecycle and business rules.
/// </summary>
public class FamilyMemberInvitationTests
{
    [Fact]
    public void CreateEmailInvitation_WithValidParameters_ShouldCreateInvitation()
    {
        // Arrange
        var familyId = FamilyId.From(Guid.NewGuid());
        var email = Email.From("john.doe@example.com");
        var role = FamilyRole.Member;
        var invitedByUserId = UserId.From(Guid.NewGuid());
        var message = "Welcome to our family!";

        // Act
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            familyId,
            email,
            role,
            invitedByUserId,
            message);

        // Assert
        invitation.Should().NotBeNull();
        invitation.FamilyId.Should().Be(familyId);
        invitation.Email.Should().Be(email);
        invitation.Role.Should().Be(role);
        invitation.InvitedByUserId.Should().Be(invitedByUserId);
        invitation.Message.Should().Be(message);
        invitation.Status.Should().Be(InvitationStatus.Pending);
        invitation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromSeconds(1));
        invitation.Token.Value.Should().NotBeNullOrEmpty();
        invitation.DisplayCode.Value.Should().NotBeNullOrEmpty();

        // Should raise domain event
        invitation.DomainEvents.Should().ContainSingle();
        invitation.DomainEvents.First().Should().BeOfType<FamilyMemberInvitedEvent>();
        var domainEvent = (FamilyMemberInvitedEvent)invitation.DomainEvents.First();
        domainEvent.InvitationId.Should().Be(invitation.Id);
        domainEvent.FamilyId.Should().Be(familyId);
        domainEvent.Email.Should().Be(email);
        domainEvent.IsResend.Should().BeFalse();
    }

    [Fact]
    public void Accept_WithPendingStatus_ShouldAcceptInvitation()
    {
        // Arrange
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            FamilyId.From(Guid.NewGuid()),
            Email.From("john.doe@example.com"),
            FamilyRole.Member,
            UserId.From(Guid.NewGuid()));
        var userId = UserId.From(Guid.NewGuid());

        // Act
        invitation.Accept(userId);

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Accepted);
        invitation.AcceptedAt.Should().NotBeNull();
        invitation.AcceptedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Should raise domain event (includes creation event)
        invitation.DomainEvents.Should().HaveCount(2);
        invitation.DomainEvents.Last().Should().BeOfType<InvitationAcceptedEvent>();
        var domainEvent = (InvitationAcceptedEvent)invitation.DomainEvents.Last();
        domainEvent.InvitationId.Should().Be(invitation.Id);
        domainEvent.UserId.Should().Be(userId);
    }

    [Fact]
    public void Accept_WithNonPendingStatus_ShouldThrowException()
    {
        // Arrange
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            FamilyId.From(Guid.NewGuid()),
            Email.From("john.doe@example.com"),
            FamilyRole.Member,
            UserId.From(Guid.NewGuid()));
        invitation.Accept(UserId.From(Guid.NewGuid())); // Already accepted
        var anotherUserId = UserId.From(Guid.NewGuid());

        // Act & Assert
        var act = () => invitation.Accept(anotherUserId);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot accept invitation in accepted status*");
    }

    [Fact]
    public void Cancel_WithPendingStatus_ShouldCancelInvitation()
    {
        // Arrange
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            FamilyId.From(Guid.NewGuid()),
            Email.From("john.doe@example.com"),
            FamilyRole.Member,
            UserId.From(Guid.NewGuid()));
        var canceledByUserId = UserId.From(Guid.NewGuid());

        // Act
        invitation.Cancel(canceledByUserId);

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Canceled);

        // Should raise domain event (includes creation event)
        invitation.DomainEvents.Should().HaveCount(2);
        invitation.DomainEvents.Last().Should().BeOfType<InvitationCanceledEvent>();
        var domainEvent = (InvitationCanceledEvent)invitation.DomainEvents.Last();
        domainEvent.InvitationId.Should().Be(invitation.Id);
        domainEvent.CanceledByUserId.Should().Be(canceledByUserId);
    }

    [Fact]
    public void Cancel_WithNonPendingStatus_ShouldThrowException()
    {
        // Arrange
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            FamilyId.From(Guid.NewGuid()),
            Email.From("john.doe@example.com"),
            FamilyRole.Member,
            UserId.From(Guid.NewGuid()));
        invitation.Accept(UserId.From(Guid.NewGuid())); // Already accepted
        var canceledByUserId = UserId.From(Guid.NewGuid());

        // Act & Assert
        var act = () => invitation.Cancel(canceledByUserId);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot cancel invitation in accepted status*");
    }

    [Fact]
    public void Resend_WithPendingStatus_ShouldRegenerateTokenAndExtendExpiration()
    {
        // Arrange
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            FamilyId.From(Guid.NewGuid()),
            Email.From("john.doe@example.com"),
            FamilyRole.Member,
            UserId.From(Guid.NewGuid()));
        var originalToken = invitation.Token;
        var originalExpiresAt = invitation.ExpiresAt;
        var resentByUserId = UserId.From(Guid.NewGuid());

        // Act
        invitation.Resend(resentByUserId);

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Pending);
        invitation.Token.Should().NotBe(originalToken);
        invitation.ExpiresAt.Should().BeAfter(originalExpiresAt);
        invitation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromSeconds(1));

        // Should raise domain event (includes creation event)
        invitation.DomainEvents.Should().HaveCount(2);
        invitation.DomainEvents.Last().Should().BeOfType<FamilyMemberInvitedEvent>();
        var domainEvent = (FamilyMemberInvitedEvent)invitation.DomainEvents.Last();
        domainEvent.InvitationId.Should().Be(invitation.Id);
        domainEvent.IsResend.Should().BeTrue();
    }

    [Fact]
    public void UpdateRole_WithPendingStatus_ShouldUpdateRole()
    {
        // Arrange
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            FamilyId.From(Guid.NewGuid()),
            Email.From("john.doe@example.com"),
            FamilyRole.Member,
            UserId.From(Guid.NewGuid()));
        var newRole = FamilyRole.Admin;

        // Act
        invitation.UpdateRole(newRole);

        // Assert
        invitation.Role.Should().Be(newRole);
    }

    [Fact]
    public void UpdateRole_WithOwnerRole_ShouldThrowException()
    {
        // Arrange
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            FamilyId.From(Guid.NewGuid()),
            Email.From("john.doe@example.com"),
            FamilyRole.Member,
            UserId.From(Guid.NewGuid()));

        // Act & Assert
        var act = () => invitation.UpdateRole(FamilyRole.Owner);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot update role to OWNER*");
    }

    [Fact]
    public void UpdateRole_WithNonPendingStatus_ShouldThrowException()
    {
        // Arrange
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            FamilyId.From(Guid.NewGuid()),
            Email.From("john.doe@example.com"),
            FamilyRole.Member,
            UserId.From(Guid.NewGuid()));
        invitation.Accept(UserId.From(Guid.NewGuid())); // Already accepted

        // Act & Assert
        var act = () => invitation.UpdateRole(FamilyRole.Admin);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Can only update role of pending invitations*");
    }

    [Fact]
    public void MarkAsAccepted_ShouldAcceptInvitationAndRaiseEvent()
    {
        // Arrange
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            FamilyId.From(Guid.NewGuid()),
            Email.From("john.doe@example.com"),
            FamilyRole.Member,
            UserId.From(Guid.NewGuid()));
        var userId = UserId.From(Guid.NewGuid());

        // Act
        invitation.MarkAsAccepted(userId);

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Accepted);
        invitation.AcceptedAt.Should().NotBeNull();

        // Should raise domain event (includes creation event)
        invitation.DomainEvents.Should().HaveCount(2);
        invitation.DomainEvents.Last().Should().BeOfType<InvitationAcceptedEvent>();
    }
}
