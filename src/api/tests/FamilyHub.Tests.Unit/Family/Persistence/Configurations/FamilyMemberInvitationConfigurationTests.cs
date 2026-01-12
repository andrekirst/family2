using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FamilyHub.Tests.Unit.Family.Persistence.Configurations;

/// <summary>
/// Unit tests for FamilyMemberInvitationConfiguration.
/// Verifies EF Core entity configuration including table name, schema, columns,
/// indexes, foreign keys, and Vogen value converters.
/// </summary>
public sealed class FamilyMemberInvitationConfigurationTests : IDisposable
{
    private readonly FamilyDbContext _context;
    private readonly IEntityType _invitationEntityType;

    public FamilyMemberInvitationConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FamilyDbContext(options);
        _invitationEntityType = _context.Model.FindEntityType(typeof(FamilyMemberInvitation))!;
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Table Configuration Tests

    [Fact]
    public void Configure_SetsTableName_ToFamilyMemberInvitations()
    {
        // Act
        var tableName = _invitationEntityType.GetTableName();

        // Assert
        tableName.Should().Be("family_member_invitations");
    }

    [Fact]
    public void Configure_SetsSchema_ToFamily()
    {
        // Act
        var schema = _invitationEntityType.GetSchema();

        // Assert
        schema.Should().Be("family");
    }

    #endregion

    #region Primary Key Tests

    [Fact]
    public void Configure_ConfiguresIdAsPrimaryKey()
    {
        // Act
        var primaryKey = _invitationEntityType.FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties[0].Name.Should().Be("Id");
    }

    [Fact]
    public void Configure_ConfiguresIdWithVogenConverter()
    {
        // Act
        var idProperty = _invitationEntityType.FindProperty("Id");

        // Assert
        idProperty.Should().NotBeNull();
        idProperty!.GetValueConverter().Should().NotBeNull("Id should have a Vogen value converter");
        idProperty.GetColumnName().Should().Be("invitation_id");
        idProperty.IsNullable.Should().BeFalse();
    }

    #endregion

    #region Vogen Value Converter Tests

    [Fact]
    public void Configure_ConfiguresDisplayCodeWithMaxLength8()
    {
        // Act
        var displayCodeProperty = _invitationEntityType.FindProperty("DisplayCode");

        // Assert
        displayCodeProperty.Should().NotBeNull();
        displayCodeProperty!.GetMaxLength().Should().Be(8);
        displayCodeProperty.GetColumnName().Should().Be("display_code");
        displayCodeProperty.IsNullable.Should().BeFalse();
        displayCodeProperty.GetValueConverter().Should().NotBeNull("DisplayCode should have a Vogen value converter");
    }

    [Fact]
    public void Configure_ConfiguresFamilyIdWithForeignKey()
    {
        // Act
        var familyIdProperty = _invitationEntityType.FindProperty("FamilyId");
        var foreignKeys = _invitationEntityType.GetForeignKeys().ToList();

        // Assert
        familyIdProperty.Should().NotBeNull();
        familyIdProperty!.GetColumnName().Should().Be("family_id");
        familyIdProperty.IsNullable.Should().BeFalse();
        familyIdProperty.GetValueConverter().Should().NotBeNull("FamilyId should have a Vogen value converter");

        // Verify FK exists
        var familyFk = foreignKeys.FirstOrDefault(fk => fk.Properties.Any(p => p.Name == "FamilyId"));
        familyFk.Should().NotBeNull("Foreign key to Family should exist");
        familyFk!.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
    }

    [Fact]
    public void Configure_ConfiguresEmailWithMaxLength255()
    {
        // Act
        var emailProperty = _invitationEntityType.FindProperty("Email");

        // Assert
        emailProperty.Should().NotBeNull();
        emailProperty!.GetMaxLength().Should().Be(255);
        emailProperty.GetColumnName().Should().Be("email");
        emailProperty.IsNullable.Should().BeFalse();
        emailProperty.GetValueConverter().Should().NotBeNull("Email should have a Vogen value converter");
    }

    [Fact]
    public void Configure_ConfiguresRoleWithCustomConverter()
    {
        // Act
        var roleProperty = _invitationEntityType.FindProperty("Role");

        // Assert
        roleProperty.Should().NotBeNull();
        roleProperty!.GetMaxLength().Should().Be(20);
        roleProperty.GetColumnName().Should().Be("role");
        roleProperty.IsNullable.Should().BeFalse();
        roleProperty.GetValueConverter().Should().NotBeNull("Role should have a custom value converter");
    }

    [Fact]
    public void Configure_ConfiguresTokenWithVogenConverter()
    {
        // Act
        var tokenProperty = _invitationEntityType.FindProperty("Token");

        // Assert
        tokenProperty.Should().NotBeNull();
        tokenProperty!.GetMaxLength().Should().Be(64);
        tokenProperty.GetColumnName().Should().Be("token");
        tokenProperty.IsNullable.Should().BeFalse();
        tokenProperty.GetValueConverter().Should().NotBeNull("Token should have a Vogen value converter");
    }

    [Fact]
    public void Configure_ConfiguresStatusWithVogenConverter()
    {
        // Act
        var statusProperty = _invitationEntityType.FindProperty("Status");

        // Assert
        statusProperty.Should().NotBeNull();
        statusProperty!.GetMaxLength().Should().Be(20);
        statusProperty.GetColumnName().Should().Be("status");
        statusProperty.IsNullable.Should().BeFalse();
        statusProperty.GetValueConverter().Should().NotBeNull("Status should have a Vogen value converter");
    }

    [Fact]
    public void Configure_ConfiguresInvitedByUserIdWithVogenConverter()
    {
        // Act
        var invitedByProperty = _invitationEntityType.FindProperty("InvitedByUserId");

        // Assert
        invitedByProperty.Should().NotBeNull();
        invitedByProperty!.GetColumnName().Should().Be("invited_by_user_id");
        invitedByProperty.IsNullable.Should().BeFalse();
        invitedByProperty.GetValueConverter().Should().NotBeNull("InvitedByUserId should have a Vogen value converter");
    }

    #endregion

    #region Index Tests

    [Fact]
    public void Configure_ConfiguresTokenAsUniqueIndex()
    {
        // Act
        var indexes = _invitationEntityType.GetIndexes().ToList();
        var tokenIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == "Token"));

        // Assert
        tokenIndex.Should().NotBeNull("Index on Token should exist");
        tokenIndex!.IsUnique.Should().BeTrue("Token index should be unique");
        tokenIndex.GetDatabaseName().Should().Be("ix_family_member_invitations_token");
    }

    [Fact]
    public void Configure_CreatesCompositeIndexOnFamilyIdStatus()
    {
        // Act
        var indexes = _invitationEntityType.GetIndexes().ToList();
        var compositeIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "FamilyId") &&
            i.Properties.Any(p => p.Name == "Status"));

        // Assert
        compositeIndex.Should().NotBeNull("Composite index on (FamilyId, Status) should exist");
        compositeIndex!.GetDatabaseName().Should().Be("ix_family_member_invitations_family_id_status");
    }

    [Fact]
    public void Configure_CreatesIndexOnExpiresAt()
    {
        // Act
        var indexes = _invitationEntityType.GetIndexes().ToList();
        var expiresAtIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 &&
            i.Properties.Any(p => p.Name == "ExpiresAt"));

        // Assert
        expiresAtIndex.Should().NotBeNull("Index on ExpiresAt should exist");
        expiresAtIndex!.GetDatabaseName().Should().Be("ix_family_member_invitations_expires_at");
    }

    [Fact]
    public void Configure_CreatesIndexOnInvitedByUserId()
    {
        // Act
        var indexes = _invitationEntityType.GetIndexes().ToList();
        var invitedByIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 &&
            i.Properties.Any(p => p.Name == "InvitedByUserId"));

        // Assert
        invitedByIndex.Should().NotBeNull("Index on InvitedByUserId should exist");
        invitedByIndex!.GetDatabaseName().Should().Be("ix_family_member_invitations_invited_by_user_id");
    }

    [Fact]
    public void Configure_CreatesIndexOnFamilyId()
    {
        // Act
        var indexes = _invitationEntityType.GetIndexes().ToList();
        var familyIdIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 &&
            i.Properties.Any(p => p.Name == "FamilyId"));

        // Assert
        familyIdIndex.Should().NotBeNull("Index on FamilyId should exist");
        familyIdIndex!.GetDatabaseName().Should().Be("ix_family_member_invitations_family_id");
    }

    #endregion

    #region Optional Property Tests

    [Fact]
    public void Configure_ConfiguresMessageAsOptionalText()
    {
        // Act
        var messageProperty = _invitationEntityType.FindProperty("Message");

        // Assert
        messageProperty.Should().NotBeNull();
        messageProperty!.IsNullable.Should().BeTrue("Message should be optional");
        messageProperty.GetColumnName().Should().Be("message");
        // Note: GetColumnType() doesn't work with InMemory database - verified via schema configuration
    }

    [Fact]
    public void Configure_ConfiguresAcceptedAtAsOptional()
    {
        // Act
        var acceptedAtProperty = _invitationEntityType.FindProperty("AcceptedAt");

        // Assert
        acceptedAtProperty.Should().NotBeNull();
        acceptedAtProperty!.IsNullable.Should().BeTrue("AcceptedAt should be optional");
        acceptedAtProperty.GetColumnName().Should().Be("accepted_at");
    }

    #endregion

    #region Ignored Properties Tests

    [Fact]
    public void Configure_IgnoresDomainEventsCollection()
    {
        // Act
        var domainEventsProperty = _invitationEntityType.FindProperty("DomainEvents");

        // Assert
        domainEventsProperty.Should().BeNull("DomainEvents should be ignored by EF Core");
    }

    #endregion
}
