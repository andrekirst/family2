using System.Security.Cryptography;
using System.Text;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitationById;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

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
        memberRepo.AddedMembers.Should().HaveCount(1);
        var member = memberRepo.AddedMembers[0];
        member.UserId.Should().Be(command.AcceptingUserId);
        member.Role.Should().Be(FamilyRole.Member);
    }

    [Fact]
    public async Task Handle_ShouldAssignUserToFamily()
    {
        // Arrange
        var (handler, command, _, _, userRepo) = CreateHappyPathScenario();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        userRepo.StoredUser!.FamilyId.Should().Be(result.FamilyId);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenInvitationNotFound()
    {
        // Arrange — no invitation found for ID
        var user = CreateTestUser();
        var memberRepo = new FakeFamilyMemberRepository();
        var invitationRepo = new FakeFamilyInvitationRepository(existingById: null);
        var userRepo = new FakeUserRepository(user);
        var handler = new AcceptInvitationByIdCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationByIdCommand(InvitationId.New(), user.Id);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invitation not found");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailMismatch()
    {
        // Arrange — user has a different email than the invitation
        var invitation = CreateTestInvitation();
        var differentUser = CreateTestUser(email: "different@example.com");
        var memberRepo = new FakeFamilyMemberRepository();
        var invitationRepo = new FakeFamilyInvitationRepository(existingById: invitation);
        var userRepo = new FakeUserRepository(differentUser);
        var handler = new AcceptInvitationByIdCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationByIdCommand(invitation.Id, differentUser.Id);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("This invitation was sent to a different email address");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var invitation = CreateTestInvitation();
        var memberRepo = new FakeFamilyMemberRepository();
        var invitationRepo = new FakeFamilyInvitationRepository(existingById: invitation);
        var userRepo = new FakeUserRepository(existingUser: null);
        var handler = new AcceptInvitationByIdCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationByIdCommand(invitation.Id, UserId.New());

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenAlreadyFamilyMember()
    {
        // Arrange — user is already a member of the family
        var familyId = FamilyId.New();
        var user = CreateTestUser();
        var invitation = CreateTestInvitation(familyId: familyId);
        var existingMember = FamilyMember.Create(familyId, user.Id, FamilyRole.Member);
        var memberRepo = new FakeFamilyMemberRepository(existingMember: existingMember);
        var invitationRepo = new FakeFamilyInvitationRepository(existingById: invitation);
        var userRepo = new FakeUserRepository(user);
        var handler = new AcceptInvitationByIdCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationByIdCommand(invitation.Id, user.Id);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("You are already a member of this family");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenInvitationExpired()
    {
        // Arrange — invitation is expired
        var user = CreateTestUser();
        var invitation = CreateExpiredInvitation();
        var memberRepo = new FakeFamilyMemberRepository();
        var invitationRepo = new FakeFamilyInvitationRepository(existingById: invitation);
        var userRepo = new FakeUserRepository(user);
        var handler = new AcceptInvitationByIdCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationByIdCommand(invitation.Id, user.Id);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invitation has expired");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenInvitationAlreadyAccepted()
    {
        // Arrange — invitation was already accepted
        var user = CreateTestUser();
        var invitation = CreateTestInvitation();
        invitation.Accept(UserId.New()); // accept by someone else first
        var memberRepo = new FakeFamilyMemberRepository();
        var invitationRepo = new FakeFamilyInvitationRepository(existingById: invitation);
        var userRepo = new FakeUserRepository(user);
        var handler = new AcceptInvitationByIdCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationByIdCommand(invitation.Id, user.Id);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Cannot accept invitation in status 'Accepted'");
    }

    // --- Helpers ---

    private static (AcceptInvitationByIdCommandHandler Handler, AcceptInvitationByIdCommand Command, FakeFamilyInvitationRepository InvitationRepo, FakeFamilyMemberRepository MemberRepo, FakeUserRepository UserRepo) CreateHappyPathScenario()
    {
        var user = CreateTestUser();
        var invitation = CreateTestInvitation();
        var memberRepo = new FakeFamilyMemberRepository();
        var invitationRepo = new FakeFamilyInvitationRepository(existingById: invitation);
        var userRepo = new FakeUserRepository(user);
        var handler = new AcceptInvitationByIdCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationByIdCommand(invitation.Id, user.Id);

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

    private static FamilyInvitation CreateTestInvitation(FamilyId? familyId = null)
    {
        var tokenHash = ComputeSha256Hash("test-token-" + Guid.NewGuid());
        return FamilyInvitation.Create(
            familyId ?? FamilyId.New(),
            UserId.New(),
            Email.From(InviteeEmailAddress),
            FamilyRole.Member,
            InvitationToken.From(tokenHash),
            "dummy-token");
    }

    private static FamilyInvitation CreateExpiredInvitation()
    {
        var invitation = CreateTestInvitation();

        // Use reflection to set ExpiresAt to the past
        var expiresAtProperty = typeof(FamilyInvitation).GetProperty(nameof(FamilyInvitation.ExpiresAt));
        expiresAtProperty!.SetValue(invitation, DateTime.UtcNow.AddDays(-1));

        return invitation;
    }
}
