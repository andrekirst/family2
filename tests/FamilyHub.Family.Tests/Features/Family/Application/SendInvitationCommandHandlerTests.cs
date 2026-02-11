using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;
using FamilyHub.Api.Features.Family.Application.Services;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Family.Tests.Features.Family.Application;

public class SendInvitationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateInvitationAndReturnResult()
    {
        // Arrange
        var familyId = FamilyId.New();
        var inviterId = UserId.New();
        var inviterMember = FamilyMember.Create(familyId, inviterId, FamilyRole.Owner);
        var memberRepo = new FakeFamilyMemberRepository(existingMember: inviterMember);
        var invitationRepo = new FakeFamilyInvitationRepository();
        var authService = new FamilyAuthorizationService(memberRepo);
        var handler = new SendInvitationCommandHandler(authService, invitationRepo);
        var command = new SendInvitationCommand(familyId, inviterId, Email.From("newmember@example.com"), FamilyRole.Member);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.InvitationId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldStoreInvitationInRepository()
    {
        // Arrange
        var familyId = FamilyId.New();
        var inviterId = UserId.New();
        var inviterMember = FamilyMember.Create(familyId, inviterId, FamilyRole.Owner);
        var memberRepo = new FakeFamilyMemberRepository(existingMember: inviterMember);
        var invitationRepo = new FakeFamilyInvitationRepository();
        var authService = new FamilyAuthorizationService(memberRepo);
        var handler = new SendInvitationCommandHandler(authService, invitationRepo);
        var command = new SendInvitationCommand(familyId, inviterId, Email.From("newmember@example.com"), FamilyRole.Member);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        invitationRepo.AddedInvitations.Should().HaveCount(1);
        var stored = invitationRepo.AddedInvitations[0];
        stored.FamilyId.Should().Be(familyId);
        stored.InvitedByUserId.Should().Be(inviterId);
        stored.InviteeEmail.Should().Be(Email.From("newmember@example.com"));
        stored.Role.Should().Be(FamilyRole.Member);
        stored.Status.Should().Be(InvitationStatus.Pending);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserLacksPermission()
    {
        // Arrange — user is a Member (cannot invite)
        var familyId = FamilyId.New();
        var inviterId = UserId.New();
        var regularMember = FamilyMember.Create(familyId, inviterId, FamilyRole.Member);
        var memberRepo = new FakeFamilyMemberRepository(existingMember: regularMember);
        var invitationRepo = new FakeFamilyInvitationRepository();
        var authService = new FamilyAuthorizationService(memberRepo);
        var handler = new SendInvitationCommandHandler(authService, invitationRepo);
        var command = new SendInvitationCommand(familyId, inviterId, Email.From("newmember@example.com"), FamilyRole.Member);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("You do not have permission to send invitations for this family");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserNotFamilyMember()
    {
        // Arrange — user is not a member of the family at all
        var familyId = FamilyId.New();
        var inviterId = UserId.New();
        var memberRepo = new FakeFamilyMemberRepository(existingMember: null);
        var invitationRepo = new FakeFamilyInvitationRepository();
        var authService = new FamilyAuthorizationService(memberRepo);
        var handler = new SendInvitationCommandHandler(authService, invitationRepo);
        var command = new SendInvitationCommand(familyId, inviterId, Email.From("newmember@example.com"), FamilyRole.Member);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("You do not have permission to send invitations for this family");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenDuplicateInvitationExists()
    {
        // Arrange
        var familyId = FamilyId.New();
        var inviterId = UserId.New();
        var inviterMember = FamilyMember.Create(familyId, inviterId, FamilyRole.Owner);
        var memberRepo = new FakeFamilyMemberRepository(existingMember: inviterMember);
        var existingInvitation = FamilyInvitation.Create(
            familyId, inviterId, Email.From("duplicate@example.com"), FamilyRole.Member,
            InvitationToken.From("a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2"),
            "dummy-token");
        var invitationRepo = new FakeFamilyInvitationRepository(existingByEmail: existingInvitation);
        var authService = new FamilyAuthorizationService(memberRepo);
        var handler = new SendInvitationCommandHandler(authService, invitationRepo);
        var command = new SendInvitationCommand(familyId, inviterId, Email.From("duplicate@example.com"), FamilyRole.Member);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("An invitation has already been sent to this email for this family");
    }

    [Fact]
    public async Task Handle_AdminShouldBeAbleToInvite()
    {
        // Arrange — Admin role (should be allowed)
        var familyId = FamilyId.New();
        var inviterId = UserId.New();
        var adminMember = FamilyMember.Create(familyId, inviterId, FamilyRole.Admin);
        var memberRepo = new FakeFamilyMemberRepository(existingMember: adminMember);
        var invitationRepo = new FakeFamilyInvitationRepository();
        var authService = new FamilyAuthorizationService(memberRepo);
        var handler = new SendInvitationCommandHandler(authService, invitationRepo);
        var command = new SendInvitationCommand(familyId, inviterId, Email.From("newmember@example.com"), FamilyRole.Member);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        invitationRepo.AddedInvitations.Should().HaveCount(1);
    }
}
