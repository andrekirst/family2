using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FamilyHub.Tests.Integration.Family.Persistence;

/// <summary>
/// Integration tests for FamilyDbContext using real PostgreSQL.
/// Tests entity configuration, Vogen converters, and query filters work correctly.
/// </summary>
[Collection("FamilyDatabase")]
public sealed class FamilyDbContextIntegrationTests : IAsyncLifetime
{
    private readonly FamilyPostgreSqlContainerFixture _fixture;
    private FamilyDbContext _context = null!;

    public FamilyDbContextIntegrationTests(FamilyPostgreSqlContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
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
                .UseNpgsql(_fixture.ConnectionString)
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
}
