namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// xUnit collection definition for sharing the PostgreSQL container across all integration tests.
/// All test classes marked with [Collection("Database")] will share the same container instance.
/// This amortizes the 60-80 second container startup cost across all tests.
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgreSqlContainerFixture>
{
    // This class is intentionally empty.
    // xUnit uses it to create the fixture instance and share it across test classes.
}
