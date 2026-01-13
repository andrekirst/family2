using FamilyHub.Modules.Family.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Tests.Unit.Family.Persistence.Configurations;

/// <summary>
/// Unit tests for FamilyConfiguration.
/// Verifies EF Core entity configuration including table name, schema, columns,
/// indexes, query filters, and Vogen value converters.
/// </summary>
public sealed class FamilyConfigurationTests : IDisposable
{
    private readonly FamilyDbContext _context;
    private readonly IEntityType _familyEntityType;

    public FamilyConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FamilyDbContext(options);
        _familyEntityType = _context.Model.FindEntityType(typeof(FamilyAggregate))!;
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Table Configuration Tests

    [Fact]
    public void Configure_SetsTableName_ToFamilies()
    {
        // Act
        var tableName = _familyEntityType.GetTableName();

        // Assert
        tableName.Should().Be("families");
    }

    [Fact]
    public void Configure_SetsSchema_ToFamily()
    {
        // Act
        var schema = _familyEntityType.GetSchema();

        // Assert
        schema.Should().Be("family");
    }

    #endregion

    #region Primary Key Tests

    [Fact]
    public void Configure_ConfiguresIdAsPrimaryKey()
    {
        // Act
        var primaryKey = _familyEntityType.FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey.Properties.Should().HaveCount(1);
        primaryKey.Properties[0].Name.Should().Be("Id");
    }

    [Fact]
    public void Configure_ConfiguresIdWithVogenConverter()
    {
        // Act
        var idProperty = _familyEntityType.FindProperty("Id");

        // Assert
        idProperty.Should().NotBeNull();
        idProperty.GetValueConverter().Should().NotBeNull("Id should have a Vogen value converter");
        idProperty.GetColumnName().Should().Be("id");
        idProperty.IsNullable.Should().BeFalse();
    }

    #endregion

    #region Property Configuration Tests

    [Fact]
    public void Configure_ConfiguresNameWithMaxLength100()
    {
        // Act
        var nameProperty = _familyEntityType.FindProperty("Name");

        // Assert
        nameProperty.Should().NotBeNull();
        nameProperty.GetMaxLength().Should().Be(100);
        nameProperty.IsNullable.Should().BeFalse();
        nameProperty.GetColumnName().Should().Be("name");
        nameProperty.GetValueConverter().Should().NotBeNull("Name should have a Vogen value converter");
    }

    [Fact]
    public void Configure_ConfiguresOwnerIdAsRequired()
    {
        // Act
        var ownerIdProperty = _familyEntityType.FindProperty("OwnerId");

        // Assert
        ownerIdProperty.Should().NotBeNull();
        ownerIdProperty.IsNullable.Should().BeFalse();
        ownerIdProperty.GetColumnName().Should().Be("owner_id");
        ownerIdProperty.GetValueConverter().Should().NotBeNull("OwnerId should have a Vogen value converter");
    }

    #endregion

    #region Index Tests

    [Fact]
    public void Configure_CreatesIndexOnOwnerId()
    {
        // Act
        var ownerIdProperty = _familyEntityType.FindProperty("OwnerId");
        var indexes = _familyEntityType.GetIndexes().ToList();

        // Assert
        ownerIdProperty.Should().NotBeNull();
        var ownerIdIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == "OwnerId"));
        ownerIdIndex.Should().NotBeNull("Index on OwnerId should exist");
        ownerIdIndex.GetDatabaseName().Should().Be("ix_families_owner_id");
    }

    #endregion

    #region Query Filter Tests

    [Fact]
    public void Configure_ConfiguresSoftDeleteQueryFilter()
    {
        // Act
        var queryFilter = _familyEntityType.GetQueryFilter();

        // Assert
        queryFilter.Should().NotBeNull("Query filter for soft delete should be configured");

        // The query filter expression should reference DeletedAt
        var filterExpression = queryFilter.ToString();
        filterExpression.Should().Contain("DeletedAt");
    }

    #endregion

    #region Audit Field Tests

    [Fact]
    public void Configure_ConfiguresAuditFieldsWithDefaults()
    {
        // Act
        var createdAtProperty = _familyEntityType.FindProperty("CreatedAt");
        var updatedAtProperty = _familyEntityType.FindProperty("UpdatedAt");

        // Assert
        createdAtProperty.Should().NotBeNull();
        createdAtProperty.GetColumnName().Should().Be("created_at");
        createdAtProperty.IsNullable.Should().BeFalse();
        createdAtProperty.GetDefaultValueSql().Should().Be("CURRENT_TIMESTAMP");

        updatedAtProperty.Should().NotBeNull();
        updatedAtProperty.GetColumnName().Should().Be("updated_at");
        updatedAtProperty.IsNullable.Should().BeFalse();
        updatedAtProperty.GetDefaultValueSql().Should().Be("CURRENT_TIMESTAMP");
    }

    #endregion

    #region Ignored Properties Tests

    [Fact]
    public void Configure_IgnoresDomainEventsCollection()
    {
        // Act
        var domainEventsProperty = _familyEntityType.FindProperty("DomainEvents");

        // Assert
        domainEventsProperty.Should().BeNull("DomainEvents should be ignored by EF Core");
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void Configure_ConfiguresDeletedAtAsNullable()
    {
        // Act
        var deletedAtProperty = _familyEntityType.FindProperty("DeletedAt");

        // Assert
        deletedAtProperty.Should().NotBeNull();
        deletedAtProperty.IsNullable.Should().BeTrue("DeletedAt should be nullable for soft delete");
        deletedAtProperty.GetColumnName().Should().Be("deleted_at");
    }

    #endregion
}
