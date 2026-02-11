using System.Security.Cryptography;
using System.Text;
using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitationById;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.UnitTests.Features.Family.Application;

/// <summary>
/// Unit tests for AcceptInvitationByIdCommandHandler.
/// Tests ID lookup, email verification, expiration, duplicate membership, and happy path.
/// Uses in-memory fake repositories (no Moq).
/// </summary>
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
        var userRepo = new FakeUserRepository(user: null);
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
        var emailVO = Email.From(email);
        var name = UserName.From("Invitee User");
        var externalId = ExternalUserId.From("invitee-external-id-" + Guid.NewGuid().ToString()[..8]);

        var user = User.Register(emailVO, name, externalId, emailVerified: true);
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

    // --- Fake Repositories ---

    internal class FakeFamilyMemberRepository : IFamilyMemberRepository
    {
        private readonly FamilyMember? _existingMember;

        public List<FamilyMember> AddedMembers { get; } = [];

        public FakeFamilyMemberRepository(FamilyMember? existingMember = null)
        {
            _existingMember = existingMember;
        }

        public Task<FamilyMember?> GetByUserAndFamilyAsync(UserId userId, FamilyId familyId, CancellationToken ct = default) =>
            Task.FromResult(_existingMember);

        public Task<List<FamilyMember>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default) =>
            Task.FromResult(_existingMember is not null ? [_existingMember] : new List<FamilyMember>());

        public Task AddAsync(FamilyMember member, CancellationToken ct = default)
        {
            AddedMembers.Add(member);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            Task.FromResult(1);
    }

    internal class FakeFamilyInvitationRepository : IFamilyInvitationRepository
    {
        private readonly FamilyInvitation? _existingById;

        public FakeFamilyInvitationRepository(FamilyInvitation? existingById = null)
        {
            _existingById = existingById;
        }

        public Task<FamilyInvitation?> GetByIdAsync(InvitationId id, CancellationToken ct = default) =>
            Task.FromResult(_existingById);

        public Task<FamilyInvitation?> GetByTokenHashAsync(InvitationToken tokenHash, CancellationToken ct = default) =>
            Task.FromResult<FamilyInvitation?>(null);

        public Task<List<FamilyInvitation>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default) =>
            Task.FromResult(new List<FamilyInvitation>());

        public Task<FamilyInvitation?> GetByEmailAndFamilyAsync(Email email, FamilyId familyId, CancellationToken ct = default) =>
            Task.FromResult<FamilyInvitation?>(null);

        public Task<List<FamilyInvitation>> GetPendingByEmailAsync(Email email, CancellationToken ct = default) =>
            Task.FromResult(new List<FamilyInvitation>());

        public Task AddAsync(FamilyInvitation invitation, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            Task.FromResult(1);
    }

    internal class FakeUserRepository : IUserRepository
    {
        private readonly User? _user;

        public User? StoredUser => _user;

        public FakeUserRepository(User? user)
        {
            _user = user;
        }

        public Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default) =>
            Task.FromResult(_user);

        public Task<User?> GetByExternalIdAsync(ExternalUserId externalId, CancellationToken ct = default) =>
            Task.FromResult(_user);

        public Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default) =>
            Task.FromResult(_user);

        public Task AddAsync(User user, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(User user, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            Task.FromResult(1);
    }
}
