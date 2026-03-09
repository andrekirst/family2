using System.Security.Cryptography;
using System.Text;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitationById;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Family.Tests.Features.Family.Application;

public class AcceptInvitationByIdCommandHandlerTests
{
    private const string InviteeEmailAddress = "invitee@example.com";

    [Fact]
    public async Task Handle_ShouldAcceptInvitationAndReturnResult()
    {
        // Arrange
        var (handler, command, _, _, _) = CreateHappyPathScenario();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FamilyId.Value.Should().NotBe(Guid.Empty);
        result.FamilyMemberId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyMember()
    {
        // Arrange
        var (handler, command, _, memberRepo, _) = CreateHappyPathScenario();

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await memberRepo.Received(1).AddAsync(
            Arg.Is<FamilyMember>(m =>
                m.UserId == command.UserId &&
                m.Role == FamilyRole.Member),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAssignUserToFamily()
    {
        // Arrange
        var (handler, command, _, _, _) = CreateHappyPathScenario();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.FamilyId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailMismatch()
    {
        // Arrange -- user has a different email than the invitation
        var invitation = CreateTestInvitation();
        var differentUser = CreateTestUser(email: "different@example.com");

        var invitationRepo = Substitute.For<IFamilyInvitationRepository>();
        invitationRepo.GetByIdAsync(invitation.Id, CancellationToken.None).Returns(invitation);

        var userRepo = Substitute.For<IUserRepository>();
        userRepo.GetByIdAsync(differentUser.Id, CancellationToken.None).Returns(differentUser);

        var memberRepo = Substitute.For<IFamilyMemberRepository>();
        var handler = new AcceptInvitationByIdCommandHandler(invitationRepo, memberRepo, userRepo, TimeProvider.System);
        var command = new AcceptInvitationByIdCommand(invitation.Id) { UserId = differentUser.Id };

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("This invitation was sent to a different email address");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenInvitationExpired()
    {
        // Arrange -- invitation is expired
        var user = CreateTestUser();
        var invitation = CreateExpiredInvitation();

        var invitationRepo = Substitute.For<IFamilyInvitationRepository>();
        invitationRepo.GetByIdAsync(invitation.Id, CancellationToken.None).Returns(invitation);

        var userRepo = Substitute.For<IUserRepository>();
        userRepo.GetByIdAsync(user.Id, CancellationToken.None).Returns(user);

        var memberRepo = Substitute.For<IFamilyMemberRepository>();
        var handler = new AcceptInvitationByIdCommandHandler(invitationRepo, memberRepo, userRepo, TimeProvider.System);
        var command = new AcceptInvitationByIdCommand(invitation.Id) { UserId = user.Id };

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invitation has expired");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenInvitationAlreadyAccepted()
    {
        // Arrange -- invitation was already accepted
        var user = CreateTestUser();
        var invitation = CreateTestInvitation();
        invitation.Accept(UserId.New(), DateTimeOffset.UtcNow); // accept by someone else first

        var invitationRepo = Substitute.For<IFamilyInvitationRepository>();
        invitationRepo.GetByIdAsync(invitation.Id, CancellationToken.None).Returns(invitation);

        var userRepo = Substitute.For<IUserRepository>();
        userRepo.GetByIdAsync(user.Id, CancellationToken.None).Returns(user);

        var memberRepo = Substitute.For<IFamilyMemberRepository>();
        var handler = new AcceptInvitationByIdCommandHandler(invitationRepo, memberRepo, userRepo, TimeProvider.System);
        var command = new AcceptInvitationByIdCommand(invitation.Id) { UserId = user.Id };

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Cannot accept invitation in status 'Accepted'");
    }

    // --- Helpers ---

    private static (AcceptInvitationByIdCommandHandler Handler, AcceptInvitationByIdCommand Command, IFamilyInvitationRepository InvitationRepo, IFamilyMemberRepository MemberRepo, IUserRepository UserRepo) CreateHappyPathScenario()
    {
        var user = CreateTestUser();
        var invitation = CreateTestInvitation();

        var invitationRepo = Substitute.For<IFamilyInvitationRepository>();
        invitationRepo.GetByIdAsync(invitation.Id, CancellationToken.None).Returns(invitation);

        var userRepo = Substitute.For<IUserRepository>();
        userRepo.GetByIdAsync(user.Id, CancellationToken.None).Returns(user);

        var memberRepo = Substitute.For<IFamilyMemberRepository>();
        var handler = new AcceptInvitationByIdCommandHandler(invitationRepo, memberRepo, userRepo, TimeProvider.System);
        var command = new AcceptInvitationByIdCommand(invitation.Id) { UserId = user.Id };

        return (handler, command, invitationRepo, memberRepo, userRepo);
    }

    private static User CreateTestUser(string email = InviteeEmailAddress)
    {
        var emailVo = Email.From(email);
        var name = UserName.From("Invitee User");
        var externalId = ExternalUserId.From("invitee-external-id-" + Guid.NewGuid().ToString()[..8]);

        var user = User.Register(emailVo, name, externalId, emailVerified: true);
        user.ClearDomainEvents();

        return user;
    }

    private static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    private static FamilyInvitation CreateTestInvitation(FamilyId? familyId = null, DateTimeOffset? utcNow = null)
    {
        var tokenHash = ComputeSha256Hash("test-token-" + Guid.NewGuid());
        return FamilyInvitation.Create(
            familyId ?? FamilyId.New(),
            UserId.New(),
            Email.From(InviteeEmailAddress),
            FamilyRole.Member,
            InvitationToken.From(tokenHash),
            "dummy-token",
            utcNow ?? DateTimeOffset.UtcNow);
    }

    private static FamilyInvitation CreateExpiredInvitation()
    {
        // Create invitation with a time 31 days in the past so it's already expired (30-day validity)
        return CreateTestInvitation(utcNow: DateTimeOffset.UtcNow.AddDays(-31));
    }
}
