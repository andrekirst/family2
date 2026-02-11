using System.Security.Cryptography;
using System.Text;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitation;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Family.Tests.Features.Family.Application;

public class AcceptInvitationCommandHandlerTests
{
    private const string PlaintextToken = "test-plaintext-token-for-acceptance";

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
    public async Task Handle_ShouldThrow_WhenTokenInvalid()
    {
        // Arrange — no invitation found for token
        var user = CreateTestUser();
        var memberRepo = new FakeFamilyMemberRepository();
        var invitationRepo = new FakeFamilyInvitationRepository(existingByTokenHash: null);
        var userRepo = new FakeUserRepository(user);
        var handler = new AcceptInvitationCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationCommand("invalid-token", user.Id);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid invitation token");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserNotFound()
    {
        // Arrange — user doesn't exist
        var invitation = CreateTestInvitation();
        var memberRepo = new FakeFamilyMemberRepository();
        var invitationRepo = new FakeFamilyInvitationRepository(existingByTokenHash: invitation);
        var userRepo = new FakeUserRepository(existingUser: null);
        var handler = new AcceptInvitationCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationCommand(PlaintextToken, UserId.New());

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
        var invitationRepo = new FakeFamilyInvitationRepository(existingByTokenHash: invitation);
        var userRepo = new FakeUserRepository(user);
        var handler = new AcceptInvitationCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationCommand(PlaintextToken, user.Id);

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
        var invitationRepo = new FakeFamilyInvitationRepository(existingByTokenHash: invitation);
        var userRepo = new FakeUserRepository(user);
        var handler = new AcceptInvitationCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationCommand(PlaintextToken, user.Id);

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
        var invitationRepo = new FakeFamilyInvitationRepository(existingByTokenHash: invitation);
        var userRepo = new FakeUserRepository(user);
        var handler = new AcceptInvitationCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationCommand(PlaintextToken, user.Id);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Cannot accept invitation in status 'Accepted'");
    }

    // --- Helpers ---

    private static (AcceptInvitationCommandHandler Handler, AcceptInvitationCommand Command, FakeFamilyInvitationRepository InvitationRepo, FakeFamilyMemberRepository MemberRepo, FakeUserRepository UserRepo) CreateHappyPathScenario()
    {
        var user = CreateTestUser();
        var invitation = CreateTestInvitation();
        var memberRepo = new FakeFamilyMemberRepository();
        var invitationRepo = new FakeFamilyInvitationRepository(existingByTokenHash: invitation);
        var userRepo = new FakeUserRepository(user);
        var handler = new AcceptInvitationCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationCommand(PlaintextToken, user.Id);

        return (handler, command, invitationRepo, memberRepo, userRepo);
    }

    private static User CreateTestUser()
    {
        var email = Email.From("invitee@example.com");
        var name = UserName.From("Invitee User");
        var externalId = ExternalUserId.From("invitee-external-id");

        var user = User.Register(email, name, externalId, emailVerified: true);
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
        var tokenHash = ComputeSha256Hash(PlaintextToken);
        return FamilyInvitation.Create(
            familyId ?? FamilyId.New(),
            UserId.New(),
            Email.From("invitee@example.com"),
            FamilyRole.Member,
            InvitationToken.From(tokenHash),
            PlaintextToken);
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
