using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Application.Commands;
using FamilyHub.Api.Features.Family.Application.Handlers;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FluentAssertions;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.UnitTests.Features.Family.Application;

/// <summary>
/// Unit tests for CreateFamilyCommandHandler.
/// Uses in-memory fake repositories (no Moq).
/// </summary>
public class CreateFamilyCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateFamilyAndReturnResult()
    {
        // Arrange
        var user = CreateTestUser();
        var userRepo = new FakeUserRepository(user);
        var familyRepo = new FakeFamilyRepository();
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"), user.Id);

        // Act
        var result = await CreateFamilyCommandHandler.Handle(command, familyRepo, userRepo, new FakeFamilyMemberRepository(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FamilyId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldAssignUserToFamily()
    {
        // Arrange
        var user = CreateTestUser();
        var userRepo = new FakeUserRepository(user);
        var familyRepo = new FakeFamilyRepository();
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"), user.Id);

        // Act
        var result = await CreateFamilyCommandHandler.Handle(command, familyRepo, userRepo, new FakeFamilyMemberRepository(), CancellationToken.None);

        // Assert
        user.FamilyId.Should().NotBeNull();
        user.FamilyId.Should().Be(result.FamilyId);
    }

    [Fact]
    public async Task Handle_ShouldAddFamilyToRepository()
    {
        // Arrange
        var user = CreateTestUser();
        var userRepo = new FakeUserRepository(user);
        var familyRepo = new FakeFamilyRepository();
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"), user.Id);

        // Act
        await CreateFamilyCommandHandler.Handle(command, familyRepo, userRepo, new FakeFamilyMemberRepository(), CancellationToken.None);

        // Assert
        familyRepo.AddedFamilies.Should().HaveCount(1);
        familyRepo.AddedFamilies[0].Name.Should().Be(command.Name);
        familyRepo.AddedFamilies[0].OwnerId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenUserAlreadyOwnsFamily()
    {
        // Arrange
        var user = CreateTestUser();
        var userRepo = new FakeUserRepository(user);
        var existingFamily = FamilyEntity.Create(FamilyName.From("Existing Family"), user.Id);
        var familyRepo = new FakeFamilyRepository(existingFamilyForOwner: existingFamily);
        var command = new CreateFamilyCommand(FamilyName.From("New Family"), user.Id);

        // Act & Assert
        var act = () => CreateFamilyCommandHandler.Handle(command, familyRepo, userRepo, new FakeFamilyMemberRepository(), CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("User already owns a family");
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenUserNotFound()
    {
        // Arrange
        var userId = UserId.New();
        var userRepo = new FakeUserRepository(user: null);
        var familyRepo = new FakeFamilyRepository();
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"), userId);

        // Act & Assert
        var act = () => CreateFamilyCommandHandler.Handle(command, familyRepo, userRepo, new FakeFamilyMemberRepository(), CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        // Arrange
        var user = CreateTestUser();
        var userRepo = new FakeUserRepository(user);
        var familyRepo = new FakeFamilyRepository();
        var command = new CreateFamilyCommand(FamilyName.From("Smith Family"), user.Id);

        // Act
        await CreateFamilyCommandHandler.Handle(command, familyRepo, userRepo, new FakeFamilyMemberRepository(), CancellationToken.None);

        // Assert
        familyRepo.SaveChangesCalled.Should().BeTrue();
    }

    // --- Helpers ---

    private static User CreateTestUser()
    {
        var email = Email.From("test@example.com");
        var name = UserName.From("Test User");
        var externalId = ExternalUserId.From("test-external-id");

        var user = User.Register(email, name, externalId, emailVerified: true);
        user.ClearDomainEvents();

        return user;
    }

    // --- Fake Repositories ---

    private class FakeFamilyRepository : IFamilyRepository
    {
        private readonly FamilyEntity? _existingFamilyForOwner;

        public List<FamilyEntity> AddedFamilies { get; } = [];
        public bool SaveChangesCalled { get; private set; }

        public FakeFamilyRepository(FamilyEntity? existingFamilyForOwner = null)
        {
            _existingFamilyForOwner = existingFamilyForOwner;
        }

        public Task<FamilyEntity?> GetByIdAsync(FamilyId id, CancellationToken ct = default) =>
            Task.FromResult<FamilyEntity?>(null);

        public Task<FamilyEntity?> GetByIdWithMembersAsync(FamilyId id, CancellationToken ct = default) =>
            Task.FromResult<FamilyEntity?>(null);

        public Task<FamilyEntity?> GetByOwnerIdAsync(UserId ownerId, CancellationToken ct = default) =>
            Task.FromResult(_existingFamilyForOwner);

        public Task<bool> UserHasFamilyAsync(UserId userId, CancellationToken ct = default) =>
            Task.FromResult(_existingFamilyForOwner is not null);

        public Task AddAsync(FamilyEntity family, CancellationToken ct = default)
        {
            AddedFamilies.Add(family);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(FamilyEntity family, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            SaveChangesCalled = true;
            return Task.FromResult(1);
        }
    }

    private class FakeFamilyMemberRepository : IFamilyMemberRepository
    {
        public List<FamilyMember> AddedMembers { get; } = [];
        public bool SaveChangesCalled { get; private set; }

        public Task<FamilyMember?> GetByUserAndFamilyAsync(UserId userId, FamilyId familyId, CancellationToken ct = default) =>
            Task.FromResult<FamilyMember?>(null);

        public Task<List<FamilyMember>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default) =>
            Task.FromResult(new List<FamilyMember>());

        public Task AddAsync(FamilyMember member, CancellationToken ct = default)
        {
            AddedMembers.Add(member);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            SaveChangesCalled = true;
            return Task.FromResult(1);
        }
    }

    private class FakeUserRepository : IUserRepository
    {
        private readonly User? _user;

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
