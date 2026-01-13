using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Integration tests for cross-DbContext data consistency.
/// Tests that ID references between Auth and Family schemas remain consistent.
/// </summary>
/// <remarks>
/// Family Hub uses a modular monolith architecture where:
/// - AuthDbContext manages the 'auth' schema (users, outbox_events)
/// - FamilyDbContext manages the 'family' schema (families, family_member_invitations)
///
/// Cross-schema references use ID-only patterns (no FK constraints) for bounded context isolation.
/// These tests verify that application-level consistency is maintained.
/// </remarks>
[Collection("DualSchema")]
public sealed class CrossDbContextConsistencyTests(DualSchemaPostgreSqlContainerFixture fixture) : IAsyncLifetime
{
    private AuthDbContext _authContext = null!;
    private FamilyDbContext _familyContext = null!;

    public Task InitializeAsync()
    {
        _authContext = fixture.CreateAuthDbContext();
        _familyContext = fixture.CreateFamilyDbContext();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Clean up test data in correct order (invitations → families → users)
        await CleanupTestDataAsync();

        await _authContext.DisposeAsync();
        await _familyContext.DisposeAsync();
    }

    private async Task CleanupTestDataAsync()
    {
        // Clean Family schema first (has FK to families)
        await _familyContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM family.family_member_invitations");
        await _familyContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM family.families");

        // Then clean Auth schema
        await _authContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM auth.users");
    }

    #region User.FamilyId References Existing Family Tests

    /// <summary>
    /// Tests that when a User references a FamilyId, that Family actually exists in FamilyDbContext.
    /// This validates the cross-schema reference pattern used throughout Family Hub.
    /// </summary>
    [Fact]
    public async Task UserFamilyId_ReferencesExistingFamily_ConsistencyMaintained()
    {
        // Arrange - Create family first (in family schema)
        var ownerId = UserId.New();
        var family = FamilyAggregate.Create(FamilyName.From("Cross-Context Test Family"), ownerId);

        await _familyContext.Families.AddAsync(family);
        await _familyContext.SaveChangesAsync();
        _familyContext.ChangeTracker.Clear();

        // Create user with reference to family (in auth schema)
        var user = User.CreateFromOAuth(
            Email.From($"crosscontext-{Guid.NewGuid():N}@example.com"),
            $"zitadel-{Guid.NewGuid():N}",
            "zitadel",
            family.Id);

        await _authContext.Users.AddAsync(user);
        await _authContext.SaveChangesAsync();
        _authContext.ChangeTracker.Clear();

        // Act - Query family from FamilyDbContext using User's FamilyId
        var familyFromFamilyContext = await _familyContext.Families
            .FirstOrDefaultAsync(f => f.Id == user.FamilyId);

        // Assert
        familyFromFamilyContext.Should().NotBeNull(
            "User.FamilyId should reference an existing family in family schema");
        familyFromFamilyContext.Id.Should().Be(family.Id);
        familyFromFamilyContext.Name.Should().Be(family.Name);
    }

    /// <summary>
    /// Tests that updating a User's FamilyId to a different family maintains consistency.
    /// </summary>
    [Fact]
    public async Task UserFamilyId_AfterUpdate_ReferencesNewFamily()
    {
        // Arrange - Create two families
        var ownerId = UserId.New();
        var originalFamily = FamilyAggregate.Create(FamilyName.From("Original Family"), ownerId);
        var newFamily = FamilyAggregate.Create(FamilyName.From("New Family"), ownerId);

        await _familyContext.Families.AddRangeAsync(originalFamily, newFamily);
        await _familyContext.SaveChangesAsync();
        _familyContext.ChangeTracker.Clear();

        // Create user with original family reference
        var user = User.CreateFromOAuth(
            Email.From($"update-family-{Guid.NewGuid():N}@example.com"),
            $"zitadel-{Guid.NewGuid():N}",
            "zitadel",
            originalFamily.Id);

        await _authContext.Users.AddAsync(user);
        await _authContext.SaveChangesAsync();

        // Act - Update user's family reference
        user.UpdateFamily(newFamily.Id);
        await _authContext.SaveChangesAsync();
        _authContext.ChangeTracker.Clear();

        // Assert - Verify new family reference
        await using var verifyAuthContext = fixture.CreateAuthDbContext();
        var retrievedUser = await verifyAuthContext.Users.FindAsync(user.Id);

        retrievedUser.Should().NotBeNull();
        retrievedUser.FamilyId.Should().Be(newFamily.Id);

        // Verify the referenced family exists
        var referencedFamily = await _familyContext.Families.FindAsync(retrievedUser.FamilyId);
        referencedFamily.Should().NotBeNull();
        referencedFamily.Name.Should().Be(FamilyName.From("New Family"));
    }

    #endregion

    #region FamilyMemberInvitation.InvitedByUserId References Existing User Tests

    /// <summary>
    /// Tests that FamilyMemberInvitation.InvitedByUserId references an existing User in AuthDbContext.
    /// </summary>
    [Fact]
    public async Task InvitationInvitedByUserId_ReferencesExistingUser()
    {
        // Arrange - Create user first (in auth schema)
        var inviterId = UserId.New();
        FamilyId.New();

        // Create family
        var family = FamilyAggregate.Create(FamilyName.From("Invitation Test Family"), inviterId);
        await _familyContext.Families.AddAsync(family);
        await _familyContext.SaveChangesAsync();
        _familyContext.ChangeTracker.Clear();

        // Create inviter user
        var inviter = User.CreateFromOAuth(
            Email.From($"inviter-{Guid.NewGuid():N}@example.com"),
            $"zitadel-{Guid.NewGuid():N}",
            "zitadel",
            family.Id);

        await _authContext.Users.AddAsync(inviter);
        await _authContext.SaveChangesAsync();
        _authContext.ChangeTracker.Clear();

        // Create invitation (in family schema)
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            family.Id,
            Email.From($"invitee-{Guid.NewGuid():N}@example.com"),
            FamilyRole.Member,
            inviter.Id,
            "Welcome to the family!");

        await _familyContext.FamilyMemberInvitations.AddAsync(invitation);
        await _familyContext.SaveChangesAsync();
        _familyContext.ChangeTracker.Clear();

        // Act - Query user from AuthDbContext using Invitation's InvitedByUserId
        var userFromAuthContext = await _authContext.Users
            .FirstOrDefaultAsync(u => u.Id == invitation.InvitedByUserId);

        // Assert
        userFromAuthContext.Should().NotBeNull(
            "Invitation.InvitedByUserId should reference an existing user in auth schema");
        userFromAuthContext.Id.Should().Be(inviter.Id);
    }

    #endregion

    #region Family.OwnerId References Existing User Tests

    /// <summary>
    /// Tests that Family.OwnerId references an existing User in AuthDbContext.
    /// </summary>
    [Fact]
    public async Task FamilyOwnerId_ReferencesExistingUser()
    {
        // Arrange - Create user first
        UserId.New();
        var tempFamilyId = FamilyId.New(); // Temp value for user creation

        var owner = User.CreateFromOAuth(
            Email.From($"owner-{Guid.NewGuid():N}@example.com"),
            $"zitadel-{Guid.NewGuid():N}",
            "zitadel",
            tempFamilyId);

        await _authContext.Users.AddAsync(owner);
        await _authContext.SaveChangesAsync();
        _authContext.ChangeTracker.Clear();

        // Create family with owner reference
        var family = FamilyAggregate.Create(FamilyName.From("Owner Reference Test Family"), owner.Id);
        await _familyContext.Families.AddAsync(family);
        await _familyContext.SaveChangesAsync();
        _familyContext.ChangeTracker.Clear();

        // Update user's family reference to the actual family
        owner.UpdateFamily(family.Id);
        await _authContext.SaveChangesAsync();
        _authContext.ChangeTracker.Clear();

        // Act - Query user from AuthDbContext using Family's OwnerId
        var ownerFromAuthContext = await _authContext.Users
            .FirstOrDefaultAsync(u => u.Id == family.OwnerId);

        // Assert
        ownerFromAuthContext.Should().NotBeNull(
            "Family.OwnerId should reference an existing user in auth schema");
        ownerFromAuthContext.Id.Should().Be(owner.Id);
    }

    /// <summary>
    /// Tests that ownership transfer updates Family.OwnerId to a valid user.
    /// </summary>
    [Fact]
    public async Task FamilyOwnershipTransfer_NewOwnerIdReferencesExistingUser()
    {
        // Arrange - Create original owner and new owner
        var tempFamilyId = FamilyId.New();

        var originalOwner = User.CreateFromOAuth(
            Email.From($"original-owner-{Guid.NewGuid():N}@example.com"),
            $"zitadel-{Guid.NewGuid():N}",
            "zitadel",
            tempFamilyId);

        var newOwner = User.CreateFromOAuth(
            Email.From($"new-owner-{Guid.NewGuid():N}@example.com"),
            $"zitadel-{Guid.NewGuid():N}",
            "zitadel",
            tempFamilyId);

        await _authContext.Users.AddRangeAsync(originalOwner, newOwner);
        await _authContext.SaveChangesAsync();
        _authContext.ChangeTracker.Clear();

        // Create family with original owner
        var family = FamilyAggregate.Create(FamilyName.From("Ownership Transfer Test Family"), originalOwner.Id);
        await _familyContext.Families.AddAsync(family);
        await _familyContext.SaveChangesAsync();

        // Act - Transfer ownership
        family.TransferOwnership(newOwner.Id);
        await _familyContext.SaveChangesAsync();
        _familyContext.ChangeTracker.Clear();

        // Assert - Verify new owner exists in auth schema
        await using var verifyFamilyContext = fixture.CreateFamilyDbContext();
        var retrievedFamily = await verifyFamilyContext.Families.FindAsync(family.Id);

        retrievedFamily.Should().NotBeNull();
        retrievedFamily.OwnerId.Should().Be(newOwner.Id);

        var newOwnerFromAuth = await _authContext.Users.FindAsync(retrievedFamily.OwnerId);
        newOwnerFromAuth.Should().NotBeNull();
        newOwnerFromAuth.Email.Should().Be(newOwner.Email);
    }

    #endregion

    #region Query Consistency Tests

    /// <summary>
    /// Tests that the same data can be retrieved consistently from both contexts.
    /// </summary>
    [Fact]
    public async Task CreateFamily_QueryFromBothContexts_DataConsistent()
    {
        // Arrange & Act - Create family
        var ownerId = UserId.New();
        var familyName = FamilyName.From("Consistency Test Family");
        var family = FamilyAggregate.Create(familyName, ownerId);

        await _familyContext.Families.AddAsync(family);
        await _familyContext.SaveChangesAsync();
        _familyContext.ChangeTracker.Clear();

        // Query from fresh contexts
        await using var context1 = fixture.CreateFamilyDbContext();
        await using var context2 = fixture.CreateFamilyDbContext();

        var familyFromContext1 = await context1.Families.FindAsync(family.Id);
        var familyFromContext2 = await context2.Families.FindAsync(family.Id);

        // Assert - Both contexts return same data
        familyFromContext1.Should().NotBeNull();
        familyFromContext2.Should().NotBeNull();

        familyFromContext1.Id.Should().Be(familyFromContext2.Id);
        familyFromContext1.Name.Should().Be(familyFromContext2.Name);
        familyFromContext1.OwnerId.Should().Be(familyFromContext2.OwnerId);
    }

    /// <summary>
    /// Tests that concurrent writes from both contexts don't corrupt data.
    /// </summary>
    [Fact]
    public async Task ConcurrentWrites_BothContexts_DataIntegrity()
    {
        // Arrange - Create shared user
        UserId.New();
        var familyId = FamilyId.New();

        var user = User.CreateFromOAuth(
            Email.From($"concurrent-{Guid.NewGuid():N}@example.com"),
            $"zitadel-{Guid.NewGuid():N}",
            "zitadel",
            familyId);

        await _authContext.Users.AddAsync(user);
        await _authContext.SaveChangesAsync();

        // Act - Create multiple families concurrently, all with same owner
        var familyTasks = Enumerable.Range(1, 5).Select(async i =>
        {
            await using var localFamilyContext = fixture.CreateFamilyDbContext();
            var family = FamilyAggregate.Create(FamilyName.From($"Concurrent Family {i}"), user.Id);
            await localFamilyContext.Families.AddAsync(family);
            await localFamilyContext.SaveChangesAsync();
            return family.Id;
        });

        var familyIds = await Task.WhenAll(familyTasks);

        // Assert - All families created with correct owner reference
        familyIds.Should().HaveCount(5);
        familyIds.Distinct().Should().HaveCount(5, "all family IDs should be unique");

        foreach (var familyId2 in familyIds)
        {
            var family = await _familyContext.Families.FindAsync(familyId2);
            family.Should().NotBeNull();
            family.OwnerId.Should().Be(user.Id);

            // Verify owner exists in auth context
            var owner = await _authContext.Users.FindAsync(family.OwnerId);
            owner.Should().NotBeNull();
        }
    }

    #endregion
}
