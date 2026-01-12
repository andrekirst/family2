using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Tests.Unit.Family.Persistence;

/// <summary>
/// Unit tests for FamilyDbContext.
/// Verifies DbContext configuration, schema setup, and entity configuration auto-discovery.
/// </summary>
public sealed class FamilyDbContextTests : IDisposable
{
    private readonly FamilyDbContext _context;

    public FamilyDbContextTests()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FamilyDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithOptions_CreatesDbContext()
    {
        // Assert
        _context.Should().NotBeNull();
        _context.Should().BeAssignableTo<DbContext>();
    }

    #endregion

    #region DbSet Accessor Tests

    [Fact]
    public void Families_DbSet_ReturnsCorrectType()
    {
        // Act
        var familiesDbSet = _context.Families;

        // Assert
        familiesDbSet.Should().NotBeNull();
        familiesDbSet.Should().BeAssignableTo<DbSet<Modules.Family.Domain.Aggregates.Family>>();
    }

    [Fact]
    public void FamilyMemberInvitations_DbSet_ReturnsCorrectType()
    {
        // Act
        var invitationsDbSet = _context.FamilyMemberInvitations;

        // Assert
        invitationsDbSet.Should().NotBeNull();
        invitationsDbSet.Should().BeAssignableTo<DbSet<FamilyMemberInvitation>>();
    }

    #endregion

    #region OnModelCreating Tests

    [Fact]
    public void OnModelCreating_SetsDefaultSchema_ToFamily()
    {
        // Act
        var model = _context.Model;
        var defaultSchema = model.GetDefaultSchema();

        // Assert
        defaultSchema.Should().Be("family");
    }

    [Fact]
    public void OnModelCreating_AppliesConfigurationsFromAssembly()
    {
        // Act
        var model = _context.Model;
        var familyEntityType = model.FindEntityType(typeof(Modules.Family.Domain.Aggregates.Family));
        var invitationEntityType = model.FindEntityType(typeof(FamilyMemberInvitation));

        // Assert - Entity types should be configured via auto-discovery
        familyEntityType.Should().NotBeNull("Family entity should be configured");
        invitationEntityType.Should().NotBeNull("FamilyMemberInvitation entity should be configured");

        // Verify table names are set (proves configurations were applied)
        familyEntityType!.GetTableName().Should().Be("families");
        invitationEntityType!.GetTableName().Should().Be("family_member_invitations");
    }

    #endregion
}
