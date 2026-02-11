using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;
using FamilyHub.Api.Features.Family.Application.Services;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.UnitTests.Features.Family.Application;

/// <summary>
/// Unit tests for SendInvitationCommandHandler.
/// Tests authorization, duplicate detection, and invitation creation.
/// Uses in-memory fake repositories (no Moq).
/// </summary>
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

    // --- Fake Repositories ---

    private class FakeFamilyMemberRepository : IFamilyMemberRepository
    {
        private readonly FamilyMember? _existingMember;

        public FakeFamilyMemberRepository(FamilyMember? existingMember = null)
        {
            _existingMember = existingMember;
        }

        public Task<FamilyMember?> GetByUserAndFamilyAsync(UserId userId, FamilyId familyId, CancellationToken ct = default) =>
            Task.FromResult(_existingMember);

        public Task<List<FamilyMember>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default) =>
            Task.FromResult(_existingMember is not null ? [_existingMember] : new List<FamilyMember>());

        public Task AddAsync(FamilyMember member, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            Task.FromResult(1);
    }

    private class FakeFamilyInvitationRepository : IFamilyInvitationRepository
    {
        private readonly FamilyInvitation? _existingByEmail;
        private readonly FamilyInvitation? _existingByTokenHash;

        public List<FamilyInvitation> AddedInvitations { get; } = [];

        public FakeFamilyInvitationRepository(
            FamilyInvitation? existingByEmail = null,
            FamilyInvitation? existingByTokenHash = null)
        {
            _existingByEmail = existingByEmail;
            _existingByTokenHash = existingByTokenHash;
        }

        public Task<FamilyInvitation?> GetByIdAsync(InvitationId id, CancellationToken ct = default) =>
            Task.FromResult<FamilyInvitation?>(null);

        public Task<FamilyInvitation?> GetByTokenHashAsync(InvitationToken tokenHash, CancellationToken ct = default) =>
            Task.FromResult(_existingByTokenHash);

        public Task<List<FamilyInvitation>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default) =>
            Task.FromResult(new List<FamilyInvitation>());

        public Task<FamilyInvitation?> GetByEmailAndFamilyAsync(Email email, FamilyId familyId, CancellationToken ct = default) =>
            Task.FromResult(_existingByEmail);

        public Task<List<FamilyInvitation>> GetPendingByEmailAsync(Email email, CancellationToken ct = default) =>
            Task.FromResult(new List<FamilyInvitation>());

        public Task AddAsync(FamilyInvitation invitation, CancellationToken ct = default)
        {
            AddedInvitations.Add(invitation);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            Task.FromResult(1);
    }
}
