using FluentAssertions;
using Npgsql;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Integration tests for PostgreSQL Row-Level Security middleware behavior.
/// Tests that session variables are correctly set and used by RLS policies.
/// </summary>
[Collection("FamilyDatabase")]
public sealed class PostgresRlsMiddlewareIntegrationTests(FamilyPostgreSqlContainerFixture fixture)
{
    /// <summary>
    /// Creates a new Npgsql connection to the test database.
    /// </summary>
    private NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(fixture.ConnectionString);
    }

    #region Session Variable Tests

    [Fact]
    public async Task SetSessionVariable_WithValidUserId_SetsVariable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        // Act - Set the session variable (simulating middleware behavior)
        // NOTE: PostgreSQL SET command doesn't support parameters, so we use set_config() function
        await using (var setCmd = new NpgsqlCommand(
            "SELECT set_config('app.current_user_id', @userId, false)",
            connection))
        {
            setCmd.Parameters.AddWithValue("@userId", userId.ToString());
            await setCmd.ExecuteNonQueryAsync();
        }

        // Assert - Verify we can read it back
        await using var getCmd = new NpgsqlCommand(
            "SELECT current_setting('app.current_user_id', true)",
            connection);
        var result = await getCmd.ExecuteScalarAsync();

        result.Should().NotBeNull();
        result.ToString().Should().Be(userId.ToString());
    }

    [Fact]
    public async Task SetSessionVariable_InNewConnection_StartsEmpty()
    {
        // Arrange - Open a new connection
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        // Act - Try to read the variable (should be empty/null in new connection)
        await using var getCmd = new NpgsqlCommand(
            "SELECT current_setting('app.current_user_id', true)",
            connection);
        var result = await getCmd.ExecuteScalarAsync();

        // Assert - Should be empty or null in a fresh connection
        (result?.ToString() ?? string.Empty).Should().BeEmpty();
    }

    [Fact]
    public async Task SetSessionVariable_MultipleConnections_AreIsolated()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        await using var connection1 = CreateConnection();
        await using var connection2 = CreateConnection();

        await connection1.OpenAsync();
        await connection2.OpenAsync();

        // Act - Set different user IDs on each connection
        await using (var cmd1 = new NpgsqlCommand(
            "SELECT set_config('app.current_user_id', @userId, false)",
            connection1))
        {
            cmd1.Parameters.AddWithValue("@userId", userId1.ToString());
            await cmd1.ExecuteNonQueryAsync();
        }

        await using (var cmd2 = new NpgsqlCommand(
            "SELECT set_config('app.current_user_id', @userId, false)",
            connection2))
        {
            cmd2.Parameters.AddWithValue("@userId", userId2.ToString());
            await cmd2.ExecuteNonQueryAsync();
        }

        // Assert - Each connection should have its own value
        await using var getCmd1 = new NpgsqlCommand(
            "SELECT current_setting('app.current_user_id', true)",
            connection1);
        var result1 = await getCmd1.ExecuteScalarAsync();

        await using var getCmd2 = new NpgsqlCommand(
            "SELECT current_setting('app.current_user_id', true)",
            connection2);
        var result2 = await getCmd2.ExecuteScalarAsync();

        result1!.ToString().Should().Be(userId1.ToString());
        result2!.ToString().Should().Be(userId2.ToString());
    }

    #endregion

    #region RLS Policy Simulation Tests

    [Fact]
    public async Task SessionVariable_CanBeUsedInQuery()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        // Create a test table
        await using (var createCmd = new NpgsqlCommand(@"
            CREATE TABLE IF NOT EXISTS rls_test (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                owner_id UUID NOT NULL,
                data TEXT
            )", connection))
        {
            await createCmd.ExecuteNonQueryAsync();
        }

        // Insert test data
        await using (var insertCmd = new NpgsqlCommand(@"
            INSERT INTO rls_test (owner_id, data) VALUES
            (@userId, 'user data'),
            (@otherId, 'other data')",
            connection))
        {
            insertCmd.Parameters.AddWithValue("@userId", userId);
            insertCmd.Parameters.AddWithValue("@otherId", Guid.NewGuid());
            await insertCmd.ExecuteNonQueryAsync();
        }

        // Set session variable
        await using (var setCmd = new NpgsqlCommand(
            "SELECT set_config('app.current_user_id', @userId, false)",
            connection))
        {
            setCmd.Parameters.AddWithValue("@userId", userId.ToString());
            await setCmd.ExecuteNonQueryAsync();
        }

        // Act - Query using the session variable (simulating RLS policy behavior)
        await using var queryCmd = new NpgsqlCommand(@"
            SELECT data FROM rls_test
            WHERE owner_id = current_setting('app.current_user_id')::uuid",
            connection);
        var result = await queryCmd.ExecuteScalarAsync();

        // Assert
        result.Should().Be("user data");

        // Cleanup
        await using var dropCmd = new NpgsqlCommand("DROP TABLE rls_test", connection);
        await dropCmd.ExecuteNonQueryAsync();
    }

    [Fact]
    public async Task SessionVariable_FiltersDifferentOwners()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        // Create and populate test table
        await using (var cmd = new NpgsqlCommand(@"
            CREATE TABLE IF NOT EXISTS rls_filter_test (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                owner_id UUID NOT NULL,
                data TEXT
            );
            INSERT INTO rls_filter_test (owner_id, data) VALUES
            (@userId, 'my data 1'),
            (@userId, 'my data 2'),
            (@otherId, 'other data 1'),
            (@otherId, 'other data 2');",
            connection))
        {
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@otherId", otherUserId);
            await cmd.ExecuteNonQueryAsync();
        }

        // Set session variable to first user
        await using (var setCmd = new NpgsqlCommand(
            "SELECT set_config('app.current_user_id', @userId, false)",
            connection))
        {
            setCmd.Parameters.AddWithValue("@userId", userId.ToString());
            await setCmd.ExecuteNonQueryAsync();
        }

        // Act
        await using var countCmd = new NpgsqlCommand(@"
            SELECT COUNT(*) FROM rls_filter_test
            WHERE owner_id = current_setting('app.current_user_id')::uuid",
            connection);
        var count = (long)(await countCmd.ExecuteScalarAsync())!;

        // Assert - Should only see 2 rows (this user's data)
        count.Should().Be(2);

        // Cleanup
        await using var dropCmd = new NpgsqlCommand("DROP TABLE rls_filter_test", connection);
        await dropCmd.ExecuteNonQueryAsync();
    }

    #endregion

    #region Connection State Tests

    [Fact]
    public async Task SetSessionVariable_OnOpenConnection_Succeeds()
    {
        // Arrange
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        // Act
        await using var cmd = new NpgsqlCommand(
            "SELECT set_config('app.current_user_id', @userId, false)",
            connection);
        cmd.Parameters.AddWithValue("@userId", Guid.NewGuid().ToString());

        var act = async () => await cmd.ExecuteNonQueryAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SetSessionVariable_OnClosedConnection_ThrowsException()
    {
        // Arrange
        await using var connection = CreateConnection();
        // Note: Don't open the connection

        // Act
        await using var cmd = new NpgsqlCommand(
            "SELECT set_config('app.current_user_id', @userId, false)",
            connection);
        cmd.Parameters.AddWithValue("@userId", Guid.NewGuid().ToString());

        var act = async () => await cmd.ExecuteNonQueryAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion
}
