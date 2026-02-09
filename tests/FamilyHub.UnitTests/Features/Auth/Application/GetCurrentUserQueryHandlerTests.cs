using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Application.Handlers;
using FamilyHub.Api.Features.Auth.Application.Queries;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.UnitTests.Features.Auth.Application;

/// <summary>
/// Unit tests for GetCurrentUserQueryHandler.
/// Tests user retrieval and role-based permission population.
/// </summary>
public class GetCurrentUserQueryHandlerTests
{
    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNull()
    {
        // Arrange
        var userRepo = new FakeUserRepository(existingUser: null);
        var memberRepo = new FakeFamilyMemberRepository(existingMember: null);
        var query = new GetCurrentUserQuery(ExternalUserId.From("nonexistent-user"));

        // Act
        var result = await GetCurrentUserQueryHandler.Handle(query, userRepo, memberRepo, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UserWithoutFamily_ShouldReturnEmptyPermissions()
    {
        // Arrange
        var user = User.Register(
            Email.From("test@example.com"),
            UserName.From("Test User"),
            ExternalUserId.From("ext-123"),
            emailVerified: true);
        var userRepo = new FakeUserRepository(existingUser: user);
        var memberRepo = new FakeFamilyMemberRepository(existingMember: null);
        var query = new GetCurrentUserQuery(ExternalUserId.From("ext-123"));

        // Act
        var result = await GetCurrentUserQueryHandler.Handle(query, userRepo, memberRepo, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Permissions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UserWithFamily_OwnerRole_ShouldReturnAllPermissions()
    {
        // Arrange
        var user = User.Register(
            Email.From("owner@example.com"),
            UserName.From("Family Owner"),
            ExternalUserId.From("ext-owner"),
            emailVerified: true);
        var familyId = FamilyId.New();
        user.AssignToFamily(familyId);

        var member = FamilyMember.Create(familyId, user.Id, FamilyRole.Owner);
        var userRepo = new FakeUserRepository(existingUser: user);
        var memberRepo = new FakeFamilyMemberRepository(existingMember: member);
        var query = new GetCurrentUserQuery(ExternalUserId.From("ext-owner"));

        // Act
        var result = await GetCurrentUserQueryHandler.Handle(query, userRepo, memberRepo, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Permissions.Should().BeEquivalentTo([
            "family:invite",
            "family:revoke-invitation",
            "family:remove-members",
            "family:edit",
            "family:delete",
            "family:manage-roles"
        ]);
    }

    [Fact]
    public async Task Handle_UserWithFamily_MemberRole_ShouldReturnEmptyPermissions()
    {
        // Arrange
        var user = User.Register(
            Email.From("member@example.com"),
            UserName.From("Family Member"),
            ExternalUserId.From("ext-member"),
            emailVerified: true);
        var familyId = FamilyId.New();
        user.AssignToFamily(familyId);

        var member = FamilyMember.Create(familyId, user.Id, FamilyRole.Member);
        var userRepo = new FakeUserRepository(existingUser: user);
        var memberRepo = new FakeFamilyMemberRepository(existingMember: member);
        var query = new GetCurrentUserQuery(ExternalUserId.From("ext-member"));

        // Act
        var result = await GetCurrentUserQueryHandler.Handle(query, userRepo, memberRepo, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Permissions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UserWithFamily_AdminRole_ShouldReturnOperationalPermissions()
    {
        // Arrange
        var user = User.Register(
            Email.From("admin@example.com"),
            UserName.From("Family Admin"),
            ExternalUserId.From("ext-admin"),
            emailVerified: true);
        var familyId = FamilyId.New();
        user.AssignToFamily(familyId);

        var member = FamilyMember.Create(familyId, user.Id, FamilyRole.Admin);
        var userRepo = new FakeUserRepository(existingUser: user);
        var memberRepo = new FakeFamilyMemberRepository(existingMember: member);
        var query = new GetCurrentUserQuery(ExternalUserId.From("ext-admin"));

        // Act
        var result = await GetCurrentUserQueryHandler.Handle(query, userRepo, memberRepo, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Permissions.Should().BeEquivalentTo([
            "family:invite",
            "family:revoke-invitation",
            "family:remove-members",
            "family:edit"
        ]);
        result.Permissions.Should().NotContain("family:delete");
        result.Permissions.Should().NotContain("family:manage-roles");
    }

    // --- Fake Repositories ---

    private class FakeUserRepository : IUserRepository
    {
        private readonly User? _existingUser;

        public FakeUserRepository(User? existingUser)
        {
            _existingUser = existingUser;
        }

        public Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default) =>
            Task.FromResult(_existingUser?.Id == id ? _existingUser : null);

        public Task<User?> GetByExternalIdAsync(ExternalUserId externalId, CancellationToken ct = default) =>
            Task.FromResult(_existingUser);

        public Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default) =>
            Task.FromResult(_existingUser?.Email == email ? _existingUser : null);

        public Task AddAsync(User user, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(User user, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            Task.FromResult(1);
    }

    private class FakeFamilyMemberRepository : IFamilyMemberRepository
    {
        private readonly FamilyMember? _existingMember;

        public FakeFamilyMemberRepository(FamilyMember? existingMember)
        {
            _existingMember = existingMember;
        }

        public Task<FamilyMember?> GetByUserAndFamilyAsync(UserId userId, FamilyId familyId, CancellationToken ct = default) =>
            Task.FromResult(_existingMember);

        public Task<List<FamilyMember>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default) =>
            Task.FromResult(_existingMember is not null ? new List<FamilyMember> { _existingMember } : new List<FamilyMember>());

        public Task AddAsync(FamilyMember member, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            Task.FromResult(1);
    }
}
