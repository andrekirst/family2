using System.Security.Cryptography;
using System.Text;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitation;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.Security;
using FluentAssertions;
using NSubstitute;

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
    public async Task Handle_ShouldThrow_WhenInvitationExpired()
    {
        // Arrange — invitation is expired
        var user = CreateTestUser();
        var invitation = CreateExpiredInvitation();
        var tokenHash = InvitationToken.From(SecureTokenHelper.ComputeSha256Hash(PlaintextToken));

        var invitationRepo = Substitute.For<IFamilyInvitationRepository>();
        invitationRepo.GetByTokenHashAsync(tokenHash, CancellationToken.None).Returns(invitation);

        var userRepo = Substitute.For<IUserRepository>();
        userRepo.GetByIdAsync(user.Id, CancellationToken.None).Returns(user);

        var memberRepo = Substitute.For<IFamilyMemberRepository>();
        var handler = new AcceptInvitationCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationCommand(PlaintextToken) { UserId = user.Id };

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
        var tokenHash = InvitationToken.From(SecureTokenHelper.ComputeSha256Hash(PlaintextToken));

        var invitationRepo = Substitute.For<IFamilyInvitationRepository>();
        invitationRepo.GetByTokenHashAsync(tokenHash, CancellationToken.None).Returns(invitation);

        var userRepo = Substitute.For<IUserRepository>();
        userRepo.GetByIdAsync(user.Id, CancellationToken.None).Returns(user);

        var memberRepo = Substitute.For<IFamilyMemberRepository>();
        var handler = new AcceptInvitationCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationCommand(PlaintextToken) { UserId = user.Id };

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Cannot accept invitation in status 'Accepted'");
    }

    // --- Helpers ---

    private static (AcceptInvitationCommandHandler Handler, AcceptInvitationCommand Command, IFamilyInvitationRepository InvitationRepo, IFamilyMemberRepository MemberRepo, IUserRepository UserRepo) CreateHappyPathScenario()
    {
        var user = CreateTestUser();
        var invitation = CreateTestInvitation();
        var tokenHash = InvitationToken.From(SecureTokenHelper.ComputeSha256Hash(PlaintextToken));

        var invitationRepo = Substitute.For<IFamilyInvitationRepository>();
        invitationRepo.GetByTokenHashAsync(tokenHash, CancellationToken.None).Returns(invitation);

        var userRepo = Substitute.For<IUserRepository>();
        userRepo.GetByIdAsync(user.Id, CancellationToken.None).Returns(user);

        var memberRepo = Substitute.For<IFamilyMemberRepository>();
        var handler = new AcceptInvitationCommandHandler(invitationRepo, memberRepo, userRepo);
        var command = new AcceptInvitationCommand(PlaintextToken) { UserId = user.Id };

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
