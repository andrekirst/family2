using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Tests.Integration.Family.Persistence;

/// <summary>
/// Integration tests for FamilyDbContext using real PostgreSQL.
/// Tests entity configuration, Vogen converters, and query filters work correctly.
/// </summary>
[Collection("FamilyDatabase")]
public sealed class FamilyDbContextIntegrationTests(FamilyPostgreSqlContainerFixture fixture) : IAsyncLifetime
{
    private FamilyDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        _context = new FamilyDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        // Container cleanup is handled by the fixture - just dispose the context
        await _context.DisposeAsync();
    }

    #region Vogen Value Converter Tests

    [Fact]
    public async Task SaveChanges_WithVogenFamilyId_PersistsCorrectly()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Test Family VogenId"),
            ownerId);

        // Act
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        // Assert - Verify we can retrieve it back
        var retrieved = await _context.Families.FindAsync(family.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(family.Id);
    }

    [Fact]
    public async Task SaveChanges_WithVogenFamilyName_PersistsCorrectly()
    {
        // Arrange
        var ownerId = UserId.New();
        var familyName = FamilyName.From("Integration Test Family");
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(familyName, ownerId);

        // Act
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        // Clear change tracker to force reload from DB
        _context.ChangeTracker.Clear();

        // Assert
        var retrieved = await _context.Families.FindAsync(family.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be(familyName);
    }

    [Fact]
    public async Task SaveChanges_WithVogenUserId_AsOwnerId_PersistsCorrectly()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Owner Test Family"),
            ownerId);

        // Act
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var retrieved = await _context.Families.FindAsync(family.Id);
        retrieved.Should().NotBeNull();
        retrieved!.OwnerId.Should().Be(ownerId);
    }

    #endregion

    #region Soft Delete Query Filter Tests

    [Fact]
    public async Task Query_WithSoftDeletedFamily_ExcludesDeletedRecords()
    {
        // Arrange
        var ownerId = UserId.New();
        var activeFamily = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Active Family Filter"),
            ownerId);
        var deletedFamily = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Deleted Family Filter"),
            ownerId);

        await _context.Families.AddRangeAsync(activeFamily, deletedFamily);
        await _context.SaveChangesAsync();

        // Soft delete one family
        deletedFamily.Delete();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var families = await _context.Families
            .Where(f => f.OwnerId == ownerId)
            .ToListAsync();

        // Assert - Only active family should be returned
        families.Should().HaveCount(1);
        families.First().Id.Should().Be(activeFamily.Id);
    }

    [Fact]
    public async Task Query_WithIgnoreQueryFilters_IncludesDeletedRecords()
    {
        // Arrange
        var ownerId = UserId.New();
        var activeFamily = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Active Family IgnoreFilter"),
            ownerId);
        var deletedFamily = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Deleted Family IgnoreFilter"),
            ownerId);

        await _context.Families.AddRangeAsync(activeFamily, deletedFamily);
        await _context.SaveChangesAsync();

        // Soft delete one family
        deletedFamily.Delete();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act - Query with IgnoreQueryFilters
        var allFamilies = await _context.Families
            .IgnoreQueryFilters()
            .Where(f => f.OwnerId == ownerId)
            .ToListAsync();

        // Assert - Both families should be returned
        allFamilies.Should().HaveCount(2);
    }

    #endregion

    #region Concurrent Operations Tests

    [Fact]
    public async Task ConcurrentSaves_WithDifferentFamilies_SucceedIndependently()
    {
        // Arrange
        var ownerId = UserId.New();
        var tasks = Enumerable.Range(1, 5).Select(async i =>
        {
            var options = new DbContextOptionsBuilder<FamilyDbContext>()
                .UseNpgsql(fixture.ConnectionString)
                .UseSnakeCaseNamingConvention()
                .Options;

            await using var localContext = new FamilyDbContext(options);
            var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
                FamilyName.From($"Concurrent Family {i}"),
                ownerId);
            await localContext.Families.AddAsync(family);
            await localContext.SaveChangesAsync();
            return family.Id;
        });

        // Act
        var familyIds = await Task.WhenAll(tasks);

        // Assert
        familyIds.Should().HaveCount(5);
        familyIds.Distinct().Should().HaveCount(5, "all family IDs should be unique");
    }

    #endregion

    #region Index Verification Tests

    [Fact]
    public async Task Query_ByOwnerId_Works()
    {
        // Arrange
        var ownerId1 = UserId.New();
        var ownerId2 = UserId.New();

        var family1 = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Owner Query Family 1"),
            ownerId1);
        var family2 = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Owner Query Family 2"),
            ownerId2);
        var family3 = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Owner Query Family 3"),
            ownerId1);

        await _context.Families.AddRangeAsync(family1, family2, family3);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act - Query by OwnerId
        var owner1Families = await _context.Families
            .Where(f => f.OwnerId == ownerId1)
            .ToListAsync();

        // Assert
        owner1Families.Should().HaveCount(2);
        owner1Families.Should().AllSatisfy(f => f.OwnerId.Should().Be(ownerId1));
    }

    #endregion

    #region Audit Field Tests

    [Fact]
    public async Task SaveChanges_SetsCreatedAtOnInsert()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Audit Test Family"),
            ownerId);
        var beforeSave = DateTime.UtcNow;

        // Act
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        // Assert
        family.CreatedAt.Should().BeOnOrAfter(beforeSave.AddSeconds(-1));
        family.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow.AddSeconds(1));
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateFamily_ChangeName_PersistsNewValue()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Original Name"),
            ownerId);

        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act - Load and update
        var loadedFamily = await _context.Families.FindAsync(family.Id);
        loadedFamily!.UpdateName(FamilyName.From("Updated Name"));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var verifyFamily = await _context.Families.FindAsync(family.Id);
        verifyFamily.Should().NotBeNull();
        verifyFamily!.Name.Should().Be(FamilyName.From("Updated Name"));
    }

    [Fact]
    public async Task UpdateFamily_TransferOwnership_PersistsNewOwner()
    {
        // Arrange
        var originalOwnerId = UserId.New();
        var newOwnerId = UserId.New();
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Ownership Transfer Test"),
            originalOwnerId);

        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var loadedFamily = await _context.Families.FindAsync(family.Id);
        loadedFamily!.TransferOwnership(newOwnerId);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var verifyFamily = await _context.Families.FindAsync(family.Id);
        verifyFamily.Should().NotBeNull();
        verifyFamily!.OwnerId.Should().Be(newOwnerId);
    }

    [Fact]
    public async Task ConcurrentUpdates_DifferentFamilies_AllSucceed()
    {
        // Arrange - Create multiple families
        var ownerId = UserId.New();
        var families = Enumerable.Range(1, 5)
            .Select(i => FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
                FamilyName.From($"Concurrent Update Family {i}"),
                ownerId))
            .ToList();

        await _context.Families.AddRangeAsync(families);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act - Update all families concurrently
        var updateTasks = families.Select(async (f, i) =>
        {
            var options = new DbContextOptionsBuilder<FamilyDbContext>()
                .UseNpgsql(fixture.ConnectionString)
                .UseSnakeCaseNamingConvention()
                .Options;

            await using var localContext = new FamilyDbContext(options);
            var family = await localContext.Families.FindAsync(f.Id);
            family!.UpdateName(FamilyName.From($"Updated Concurrent Family {i}"));
            await localContext.SaveChangesAsync();
            return family.Id;
        });

        var updatedIds = await Task.WhenAll(updateTasks);

        // Assert
        updatedIds.Should().HaveCount(5);

        foreach (var family in families)
        {
            var verifyFamily = await _context.Families.FindAsync(family.Id);
            verifyFamily.Should().NotBeNull();
            verifyFamily!.Name.Value.Should().StartWith("Updated Concurrent Family");
        }
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteFamily_SoftDelete_SetsDeletedAt()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Soft Delete Test Family"),
            ownerId);

        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var loadedFamily = await _context.Families.FindAsync(family.Id);
        loadedFamily!.Delete();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert - Query with IgnoreQueryFilters to see deleted record
        var deletedFamily = await _context.Families
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == family.Id);

        deletedFamily.Should().NotBeNull();
        deletedFamily!.DeletedAt.Should().NotBeNull();
        deletedFamily.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteFamily_CanQueryWithIgnoreQueryFilters()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("IgnoreFilter Delete Test"),
            ownerId);

        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        family.Delete();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act - Normal query should not find it
        var normalQuery = await _context.Families.FindAsync(family.Id);

        // Query with IgnoreQueryFilters should find it
        var ignoredQuery = await _context.Families
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == family.Id);

        // Assert
        normalQuery.Should().BeNull("soft deleted family should be filtered by default");
        ignoredQuery.Should().NotBeNull("IgnoreQueryFilters should include soft deleted family");
    }

    #endregion

    #region FamilyMemberInvitation Tests

    [Fact]
    public async Task CreateInvitation_WithForeignKey_PersistsCorrectly()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Invitation FK Test Family"),
            ownerId);

        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            family.Id,
            Email.From($"invited-{Guid.NewGuid():N}@example.com"),
            FamilyRole.Member,
            ownerId,
            "Welcome to the family!");

        // Act
        await _context.FamilyMemberInvitations.AddAsync(invitation);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var retrieved = await _context.FamilyMemberInvitations.FindAsync(invitation.Id);
        retrieved.Should().NotBeNull();
        retrieved!.FamilyId.Should().Be(family.Id);
    }

    [Fact]
    public async Task GetInvitation_ByToken_ReturnsInvitation()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Token Query Test Family"),
            ownerId);

        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            family.Id,
            Email.From($"token-test-{Guid.NewGuid():N}@example.com"),
            FamilyRole.Member,
            ownerId);

        await _context.FamilyMemberInvitations.AddAsync(invitation);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _context.FamilyMemberInvitations
            .FirstOrDefaultAsync(i => i.Token == invitation.Token);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(invitation.Id);
    }

    [Fact]
    public async Task UpdateInvitation_AcceptStatus_UpdatesCorrectly()
    {
        // Arrange
        var ownerId = UserId.New();
        var acceptingUserId = UserId.New();
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Accept Status Test Family"),
            ownerId);

        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            family.Id,
            Email.From($"accept-test-{Guid.NewGuid():N}@example.com"),
            FamilyRole.Member,
            ownerId);

        await _context.FamilyMemberInvitations.AddAsync(invitation);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var loadedInvitation = await _context.FamilyMemberInvitations.FindAsync(invitation.Id);
        loadedInvitation!.Accept(acceptingUserId);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var verifyInvitation = await _context.FamilyMemberInvitations.FindAsync(invitation.Id);
        verifyInvitation.Should().NotBeNull();
        verifyInvitation!.Status.Should().Be(InvitationStatus.Accepted);
        verifyInvitation.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExpiredInvitations_QueryByExpiresAt_Works()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Expiration Query Test Family"),
            ownerId);

        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        // Create multiple invitations
        var invitations = Enumerable.Range(1, 3)
            .Select(i => FamilyMemberInvitation.CreateEmailInvitation(
                family.Id,
                Email.From($"expiry-test-{i}-{Guid.NewGuid():N}@example.com"),
                FamilyRole.Member,
                ownerId))
            .ToList();

        await _context.FamilyMemberInvitations.AddRangeAsync(invitations);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act - Query for invitations expiring in the future
        var futureDate = DateTime.UtcNow.AddDays(7);
        var pendingInvitations = await _context.FamilyMemberInvitations
            .Where(i => i.FamilyId == family.Id)
            .Where(i => i.ExpiresAt > DateTime.UtcNow)
            .Where(i => i.ExpiresAt <= futureDate.AddDays(10))
            .ToListAsync();

        // Assert
        pendingInvitations.Should().HaveCount(3);
        pendingInvitations.Should().AllSatisfy(i => i.ExpiresAt.Should().BeAfter(DateTime.UtcNow));
    }

    [Fact]
    public async Task InvitationVogenConverters_AllTypesPersistCorrectly()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Vogen Invitation Test Family"),
            ownerId);

        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var email = Email.From($"vogen-test-{Guid.NewGuid():N}@example.com");
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            family.Id,
            email,
            FamilyRole.Member,
            ownerId,
            "Test message");

        // Store original values for comparison
        var originalToken = invitation.Token;
        var originalDisplayCode = invitation.DisplayCode;

        await _context.FamilyMemberInvitations.AddAsync(invitation);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _context.FamilyMemberInvitations.FindAsync(invitation.Id);

        // Assert - All Vogen types should round-trip correctly
        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be(email);
        retrieved.Token.Should().Be(originalToken);
        retrieved.DisplayCode.Should().Be(originalDisplayCode);
        retrieved.FamilyId.Should().Be(family.Id);
        retrieved.InvitedByUserId.Should().Be(ownerId);
        retrieved.Role.Should().Be(FamilyRole.Member);
    }

    [Fact]
    public async Task MultipleInvitations_SameFamily_PersistCorrectly()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = FamilyHub.Modules.Family.Domain.Aggregates.Family.Create(
            FamilyName.From("Multiple Invitations Test Family"),
            ownerId);

        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var invitations = Enumerable.Range(1, 5)
            .Select(i => FamilyMemberInvitation.CreateEmailInvitation(
                family.Id,
                Email.From($"multi-invite-{i}-{Guid.NewGuid():N}@example.com"),
                FamilyRole.Member,
                ownerId))
            .ToList();

        // Act
        await _context.FamilyMemberInvitations.AddRangeAsync(invitations);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var familyInvitations = await _context.FamilyMemberInvitations
            .Where(i => i.FamilyId == family.Id)
            .ToListAsync();

        familyInvitations.Should().HaveCount(5);
        familyInvitations.Select(i => i.Token).Distinct().Should().HaveCount(5,
            "each invitation should have a unique token");
    }

    #endregion
}
