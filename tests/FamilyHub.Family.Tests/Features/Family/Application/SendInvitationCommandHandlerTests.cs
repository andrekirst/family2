using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Family.Tests.Features.Family.Application;

public class SendInvitationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateInvitationAndReturnResult()
    {
        // Arrange
        var familyId = FamilyId.New();
        var inviterId = UserId.New();
        var invitationRepo = Substitute.For<IFamilyInvitationRepository>();
        var handler = new SendInvitationCommandHandler(invitationRepo, TimeProvider.System);
        var command = new SendInvitationCommand(Email.From("newmember@example.com"), FamilyRole.Member) { UserId = inviterId, FamilyId = familyId };

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
        var invitationRepo = Substitute.For<IFamilyInvitationRepository>();
        var handler = new SendInvitationCommandHandler(invitationRepo, TimeProvider.System);
        var command = new SendInvitationCommand(Email.From("newmember@example.com"), FamilyRole.Member) { UserId = inviterId, FamilyId = familyId };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await invitationRepo.Received(1).AddAsync(
            Arg.Is<FamilyInvitation>(inv =>
                inv.FamilyId == familyId &&
                inv.InvitedByUserId == inviterId &&
                inv.InviteeEmail == Email.From("newmember@example.com") &&
                inv.Role == FamilyRole.Member &&
                inv.Status == InvitationStatus.Pending),
            Arg.Any<CancellationToken>());
    }

}
