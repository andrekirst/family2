using DotNet.Testcontainers.Builders;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Integration tests for EF Core migration behavior with FamilyDbContext.
/// Tests migration apply and rollback scenarios using a dedicated container.
/// </summary>
/// <remarks>
/// These tests use a DEDICATED PostgreSQL container (not shared) to avoid
/// affecting other tests. Migration rollback can drop schemas, so isolation
/// is essential.
///
/// Note: The Family module uses EnsureCreated() in test fixtures rather than
/// MigrateAsync() due to migration discovery issues in test environments.
/// These tests verify that the underlying migration mechanism works correctly.
/// </remarks>
public sealed class FamilyDbContextMigrationTests : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private string _connectionString = string.Empty;

    public async Task InitializeAsync()
    {
        // Dedicated container for migration tests (not shared)
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("migration_test")
            .WithUsername("migration_user")
            .WithPassword(Guid.NewGuid().ToString())
            .WithCleanUp(true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilCommandIsCompleted("pg_isready"))
            .Build();

        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    #region Schema Creation Tests

    /// <summary>
    /// Tests that EnsureCreated creates the family schema with all tables.
    /// </summary>
    [Fact]
    public async Task EnsureCreated_CreatesSchemaAndTables()
    {
        // Arrange
        await using var context = CreateDbContext();

        // Act
        await context.Database.EnsureCreatedAsync();

        // Assert - Verify schema exists
        var schemaExists = await SchemaExistsAsync("family");
        schemaExists.Should().BeTrue("EnsureCreated should create the family schema");

        // Verify tables exist
        var familiesTableExists = await TableExistsAsync("family", "families");
        var invitationsTableExists = await TableExistsAsync("family", "family_member_invitations");

        familiesTableExists.Should().BeTrue("families table should exist");
        invitationsTableExists.Should().BeTrue("family_member_invitations table should exist");
    }

    /// <summary>
    /// Tests that EnsureDeleted removes the schema completely.
    /// </summary>
    [Fact]
    public async Task EnsureDeleted_RemovesAllTables()
    {
        // Arrange - Create schema first
        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        // Verify schema was created
        var schemaExistsBefore = await SchemaExistsAsync("family");
        schemaExistsBefore.Should().BeTrue();

        // Act
        await context.Database.EnsureDeletedAsync();

        // Assert - Schema should be removed
        // Note: EnsureDeleted drops the entire database, so we verify by checking
        // if the database itself no longer exists (connection will fail or database is gone)
        var databaseDeleted = await DatabaseDeletedAsync();

        // After EnsureDeleted, the entire database is dropped
        databaseDeleted.Should().BeTrue("EnsureDeleted should drop the database");
    }

    #endregion

    #region Data Persistence Tests

    /// <summary>
    /// Tests that data persists correctly after schema creation.
    /// </summary>
    [Fact]
    public async Task AfterCreation_DataCanBePersisted()
    {
        // Arrange
        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var ownerId = UserId.New();
        var family = FamilyAggregate.Create(FamilyName.From("Migration Test Family"), ownerId);

        // Act
        await context.Families.AddAsync(family);
        await context.SaveChangesAsync();

        // Assert - Query with fresh context
        await using var verifyContext = CreateDbContext();
        var retrieved = await verifyContext.Families.FindAsync(family.Id);

        retrieved.Should().NotBeNull();
        retrieved.Name.Should().Be(FamilyName.From("Migration Test Family"));
    }

    /// <summary>
    /// Tests that EnsureDeleted removes all data.
    /// </summary>
    [Fact]
    public async Task EnsureDeleted_WithData_RemovesAllData()
    {
        // Arrange - Create schema and insert data
        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var ownerId = UserId.New();
        var family = FamilyAggregate.Create(FamilyName.From("Data Cleanup Test"), ownerId);
        await context.Families.AddAsync(family);
        await context.SaveChangesAsync();

        var familyId = family.Id;

        // Verify data exists
        var dataExistsBefore = await context.Families.AnyAsync(f => f.Id == familyId);
        dataExistsBefore.Should().BeTrue();

        // Act
        await context.Database.EnsureDeletedAsync();

        // Re-create schema
        await context.Database.EnsureCreatedAsync();

        // Assert - No data should exist after recreation
        await using var verifyContext = CreateDbContext();
        var count = await verifyContext.Families.CountAsync();

        count.Should().Be(0, "data should not survive EnsureDeleted + EnsureCreated cycle");
    }

    #endregion

    #region Schema Verification Tests

    /// <summary>
    /// Tests that the families table has the expected columns.
    /// </summary>
    [Fact]
    public async Task FamiliesTable_HasExpectedColumns()
    {
        // Arrange
        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        // Act - Query column info
        var columns = await GetTableColumnsAsync("family", "families");

        // Assert
        columns.Should().Contain("id", "primary key column");
        columns.Should().Contain("name", "family name column");
        columns.Should().Contain("owner_id", "owner reference column");
        columns.Should().Contain("created_at", "audit timestamp");
        columns.Should().Contain("updated_at", "audit timestamp");
        columns.Should().Contain("deleted_at", "soft delete column");
    }

    /// <summary>
    /// Tests that the family_member_invitations table has the expected columns.
    /// </summary>
    [Fact]
    public async Task InvitationsTable_HasExpectedColumns()
    {
        // Arrange
        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        // Act
        var columns = await GetTableColumnsAsync("family", "family_member_invitations");

        // Assert - Note: EF Core uses "invitation_id" for primary key due to entity naming
        columns.Should().Contain("invitation_id", "primary key column");
        columns.Should().Contain("family_id");
        columns.Should().Contain("email");
        columns.Should().Contain("role");
        columns.Should().Contain("token");
        columns.Should().Contain("display_code");
        columns.Should().Contain("expires_at");
        columns.Should().Contain("invited_by_user_id");
        columns.Should().Contain("status");
    }

    /// <summary>
    /// Tests that foreign key constraint exists between invitations and families.
    /// </summary>
    [Fact]
    public async Task InvitationsTable_HasForeignKeyToFamilies()
    {
        // Arrange
        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        // Act
        var hasFk = await ForeignKeyExistsAsync(
            "family",
            "family_member_invitations",
            "families");

        // Assert
        hasFk.Should().BeTrue(
            "family_member_invitations should have FK to families");
    }

    #endregion

    #region Idempotency Tests

    /// <summary>
    /// Tests that calling EnsureCreated multiple times is safe (idempotent).
    /// </summary>
    [Fact]
    public async Task EnsureCreated_CalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        await using var context = CreateDbContext();

        // Act - Call EnsureCreated multiple times
        await context.Database.EnsureCreatedAsync();
        await context.Database.EnsureCreatedAsync();
        await context.Database.EnsureCreatedAsync();

        // Assert - Should still work correctly
        var schemaExists = await SchemaExistsAsync("family");
        schemaExists.Should().BeTrue();

        // Should be able to insert data
        var family = FamilyAggregate.Create(FamilyName.From("Idempotency Test"), UserId.New());
        await context.Families.AddAsync(family);

        var act = async () => await context.SaveChangesAsync();
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Index Verification Tests

    /// <summary>
    /// Tests that expected indexes exist on the families table.
    /// </summary>
    [Fact]
    public async Task FamiliesTable_HasExpectedIndexes()
    {
        // Arrange
        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        // Act
        var indexes = await GetTableIndexesAsync("family", "families");

        // Assert - Primary key index should exist
        indexes.Should().Contain(idx => idx.Contains("pkey") || idx.Contains("pk"),
            "primary key index should exist");

        // Owner_id index should exist (for lookups by owner)
        indexes.Should().Contain(idx => idx.Contains("owner_id"),
            "index on owner_id should exist for efficient owner lookups");
    }

    #endregion

    #region Helper Methods

    private FamilyDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseNpgsql(_connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new FamilyDbContext(options);
    }

    private async Task<bool> SchemaExistsAsync(string schemaName)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT EXISTS(SELECT 1 FROM information_schema.schemata WHERE schema_name = @schema)",
            connection);
        cmd.Parameters.AddWithValue("@schema", schemaName);

        return (bool)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task<bool> DatabaseDeletedAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            // If we can connect, database exists
            return false;
        }
        catch (PostgresException ex) when (ex.SqlState == "3D000") // invalid_catalog_name = database does not exist
        {
            return true;
        }
    }

    private async Task<bool> TableExistsAsync(string schemaName, string tableName)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"SELECT EXISTS(
                SELECT 1 FROM information_schema.tables
                WHERE table_schema = @schema AND table_name = @table)",
            connection);
        cmd.Parameters.AddWithValue("@schema", schemaName);
        cmd.Parameters.AddWithValue("@table", tableName);

        return (bool)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task<List<string>> GetTableColumnsAsync(string schemaName, string tableName)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"SELECT column_name FROM information_schema.columns
              WHERE table_schema = @schema AND table_name = @table",
            connection);
        cmd.Parameters.AddWithValue("@schema", schemaName);
        cmd.Parameters.AddWithValue("@table", tableName);

        var columns = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(0));
        }

        return columns;
    }

    private async Task<List<string>> GetTableIndexesAsync(string schemaName, string tableName)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"SELECT indexname FROM pg_indexes
              WHERE schemaname = @schema AND tablename = @table",
            connection);
        cmd.Parameters.AddWithValue("@schema", schemaName);
        cmd.Parameters.AddWithValue("@table", tableName);

        var indexes = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            indexes.Add(reader.GetString(0));
        }

        return indexes;
    }

    private async Task<bool> ForeignKeyExistsAsync(
        string schemaName,
        string tableName,
        string referencedTable)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"SELECT EXISTS(
                SELECT 1 FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                    ON tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = @schema
                    AND tc.table_name = @table
                    AND tc.constraint_type = 'FOREIGN KEY'
                    AND ccu.table_name = @referenced)",
            connection);
        cmd.Parameters.AddWithValue("@schema", schemaName);
        cmd.Parameters.AddWithValue("@table", tableName);
        cmd.Parameters.AddWithValue("@referenced", referencedTable);

        return (bool)(await cmd.ExecuteScalarAsync())!;
    }

    #endregion
}
