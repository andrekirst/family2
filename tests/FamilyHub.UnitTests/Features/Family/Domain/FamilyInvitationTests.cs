using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Events;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.UnitTests.Features.Family.Domain;

/// <summary>
/// Unit tests for FamilyInvitation aggregate root.
/// Tests the full lifecycle: Create, Accept, Decline, Revoke.
/// Validates invariants, domain events, and expiration logic.
/// </summary>
public class FamilyInvitationTests
{
    private static readonly FamilyId TestFamilyId = FamilyId.New();
    private static readonly UserId TestInviterId = UserId.New();
    private static readonly Email TestEmail = Email.From("invitee@example.com");
    private static readonly FamilyRole TestRole = FamilyRole.Member;
    private const string TestTokenHash = "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2";
    private const string TestPlaintextToken = "plaintext-token-for-email";

    // --- Create ---

    [Fact]
    public void Create_ShouldCreateInvitationWithValidData()
    {
        // Act
        var invitation = CreateTestInvitation();

        // Assert
        invitation.Should().NotBeNull();
        invitation.Id.Value.Should().NotBe(Guid.Empty);
        invitation.FamilyId.Should().Be(TestFamilyId);
        invitation.InvitedByUserId.Should().Be(TestInviterId);
        invitation.InviteeEmail.Should().Be(TestEmail);
        invitation.Role.Should().Be(TestRole);
        invitation.Status.Should().Be(InvitationStatus.Pending);
        invitation.TokenHash.Should().Be(InvitationToken.From(TestTokenHash));
        invitation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        invitation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(2));
        invitation.AcceptedByUserId.Should().BeNull();
        invitation.AcceptedAt.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRaiseInvitationSentEvent()
    {
        // Act
        var invitation = CreateTestInvitation();

        // Assert
        invitation.DomainEvents.Should().HaveCount(1);
        var domainEvent = invitation.DomainEvents.First();
        domainEvent.Should().BeOfType<InvitationSentEvent>();

        var sentEvent = (InvitationSentEvent)domainEvent;
        sentEvent.InvitationId.Should().Be(invitation.Id);
        sentEvent.FamilyId.Should().Be(TestFamilyId);
        sentEvent.InvitedByUserId.Should().Be(TestInviterId);
        sentEvent.InviteeEmail.Should().Be(TestEmail);
        sentEvent.Role.Should().Be(TestRole);
        sentEvent.PlaintextToken.Should().Be(TestPlaintextToken);
        sentEvent.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Act
        var invitation1 = CreateTestInvitation();
        var invitation2 = CreateTestInvitation();

        // Assert
        invitation1.Id.Should().NotBe(invitation2.Id);
    }

    // --- Accept ---

    [Fact]
    public void Accept_ShouldTransitionToPendingToAccepted()
    {
        // Arrange
        var invitation = CreateTestInvitation();
        invitation.ClearDomainEvents();
        var acceptingUserId = UserId.New();

        // Act
        invitation.Accept(acceptingUserId);

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Accepted);
        invitation.AcceptedByUserId.Should().Be(acceptingUserId);
        invitation.AcceptedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Accept_ShouldRaiseInvitationAcceptedEvent()
    {
        // Arrange
        var invitation = CreateTestInvitation();
        invitation.ClearDomainEvents();
        var acceptingUserId = UserId.New();

        // Act
        invitation.Accept(acceptingUserId);

        // Assert
        invitation.DomainEvents.Should().HaveCount(1);
        var domainEvent = invitation.DomainEvents.First();
        domainEvent.Should().BeOfType<InvitationAcceptedEvent>();

        var acceptedEvent = (InvitationAcceptedEvent)domainEvent;
        acceptedEvent.InvitationId.Should().Be(invitation.Id);
        acceptedEvent.FamilyId.Should().Be(TestFamilyId);
        acceptedEvent.AcceptedByUserId.Should().Be(acceptingUserId);
        acceptedEvent.Role.Should().Be(TestRole);
    }

    [Fact]
    public void Accept_ShouldThrow_WhenAlreadyAccepted()
    {
        // Arrange
        var invitation = CreateTestInvitation();
        invitation.Accept(UserId.New());

        // Act & Assert
        var act = () => invitation.Accept(UserId.New());
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot accept invitation in status 'Accepted'");
    }

    [Fact]
    public void Accept_ShouldThrow_WhenDeclined()
    {
        // Arrange
        var invitation = CreateTestInvitation();
        invitation.Decline();

        // Act & Assert
        var act = () => invitation.Accept(UserId.New());
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot accept invitation in status 'Declined'");
    }

    [Fact]
    public void Accept_ShouldThrow_WhenRevoked()
    {
        // Arrange
        var invitation = CreateTestInvitation();
        invitation.Revoke();

        // Act & Assert
        var act = () => invitation.Accept(UserId.New());
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot accept invitation in status 'Revoked'");
    }

    [Fact]
    public void Accept_ShouldThrow_WhenExpired()
    {
        // Arrange — create an invitation that's already expired
        var invitation = CreateExpiredInvitation();

        // Act & Assert
        var act = () => invitation.Accept(UserId.New());
        act.Should().Throw<DomainException>()
            .WithMessage("Invitation has expired");
    }

    // --- Decline ---

    [Fact]
    public void Decline_ShouldTransitionToDeclined()
    {
        // Arrange
        var invitation = CreateTestInvitation();
        invitation.ClearDomainEvents();

        // Act
        invitation.Decline();

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Declined);
    }

    [Fact]
    public void Decline_ShouldRaiseInvitationDeclinedEvent()
    {
        // Arrange
        var invitation = CreateTestInvitation();
        invitation.ClearDomainEvents();

        // Act
        invitation.Decline();

        // Assert
        invitation.DomainEvents.Should().HaveCount(1);
        var domainEvent = invitation.DomainEvents.First();
        domainEvent.Should().BeOfType<InvitationDeclinedEvent>();

        var declinedEvent = (InvitationDeclinedEvent)domainEvent;
        declinedEvent.InvitationId.Should().Be(invitation.Id);
        declinedEvent.FamilyId.Should().Be(TestFamilyId);
    }

    [Fact]
    public void Decline_ShouldThrow_WhenAlreadyAccepted()
    {
        // Arrange
        var invitation = CreateTestInvitation();
        invitation.Accept(UserId.New());

        // Act & Assert
        var act = () => invitation.Decline();
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot decline invitation in status 'Accepted'");
    }

    [Fact]
    public void Decline_ShouldThrow_WhenAlreadyDeclined()
    {
        // Arrange
        var invitation = CreateTestInvitation();
        invitation.Decline();

        // Act & Assert
        var act = () => invitation.Decline();
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot decline invitation in status 'Declined'");
    }

    // --- Revoke ---

    [Fact]
    public void Revoke_ShouldTransitionToRevoked()
    {
        // Arrange
        var invitation = CreateTestInvitation();
        invitation.ClearDomainEvents();

        // Act
        invitation.Revoke();

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Revoked);
    }

    [Fact]
    public void Revoke_ShouldRaiseInvitationRevokedEvent()
    {
        // Arrange
        var invitation = CreateTestInvitation();
        invitation.ClearDomainEvents();

        // Act
        invitation.Revoke();

        // Assert
        invitation.DomainEvents.Should().HaveCount(1);
        var domainEvent = invitation.DomainEvents.First();
        domainEvent.Should().BeOfType<InvitationRevokedEvent>();

        var revokedEvent = (InvitationRevokedEvent)domainEvent;
        revokedEvent.InvitationId.Should().Be(invitation.Id);
        revokedEvent.FamilyId.Should().Be(TestFamilyId);
    }

    [Fact]
    public void Revoke_ShouldThrow_WhenAlreadyAccepted()
    {
        // Arrange
        var invitation = CreateTestInvitation();
        invitation.Accept(UserId.New());

        // Act & Assert
        var act = () => invitation.Revoke();
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot revoke invitation in status 'Accepted'");
    }

    // --- IsExpired ---

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenNotExpired()
    {
        // Arrange
        var invitation = CreateTestInvitation();

        // Act & Assert
        invitation.IsExpired().Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenExpired()
    {
        // Arrange
        var invitation = CreateExpiredInvitation();

        // Act & Assert
        invitation.IsExpired().Should().BeTrue();
    }

    // --- Helpers ---

    private static FamilyInvitation CreateTestInvitation()
    {
        return FamilyInvitation.Create(
            TestFamilyId,
            TestInviterId,
            TestEmail,
            TestRole,
            InvitationToken.From(TestTokenHash),
            TestPlaintextToken);
    }

    /// <summary>
    /// Creates an invitation that's already expired by using reflection to set ExpiresAt in the past.
    /// This is necessary because the factory always creates invitations with ExpiresAt = UtcNow + 30 days.
    /// </summary>
    private static FamilyInvitation CreateExpiredInvitation()
    {
        var invitation = CreateTestInvitation();

        // Use reflection to set ExpiresAt to the past (domain model doesn't expose a setter)
        var expiresAtProperty = typeof(FamilyInvitation).GetProperty(nameof(FamilyInvitation.ExpiresAt));
        expiresAtProperty!.SetValue(invitation, DateTime.UtcNow.AddDays(-1));

        return invitation;
    }
}
