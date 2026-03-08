using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;
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
        var invitationRepo = new FakeFamilyInvitationRepository();
        var handler = new SendInvitationCommandHandler(invitationRepo);
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
        var invitationRepo = new FakeFamilyInvitationRepository();
        var handler = new SendInvitationCommandHandler(invitationRepo);
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

}
