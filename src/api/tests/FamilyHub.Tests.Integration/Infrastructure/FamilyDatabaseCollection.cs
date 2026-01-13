namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// xUnit collection definition for sharing the PostgreSQL container across Family module integration tests.
/// All test classes marked with [Collection("FamilyDatabase")] will share the same container instance.
/// This collection uses a fixture that only applies FamilyDbContext migrations (not AuthDbContext).
/// </summary>
[CollectionDefinition("FamilyDatabase")]
public class FamilyDatabaseCollection : ICollectionFixture<FamilyPostgreSqlContainerFixture>
{
    // This class is intentionally empty.
    // xUnit uses it to create the fixture instance and share it across test classes.
}
