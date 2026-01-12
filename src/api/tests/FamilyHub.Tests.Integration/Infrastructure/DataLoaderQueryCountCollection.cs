namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// xUnit collection definition for DataLoader query count tests.
/// Uses DualSchemaPostgreSqlContainerFixture for both Auth and Family schemas.
/// This enables sharing the PostgreSQL container across all DataLoader test classes.
/// </summary>
[CollectionDefinition("DataLoaderQueryCount")]
public class DataLoaderQueryCountCollection : ICollectionFixture<DualSchemaPostgreSqlContainerFixture>
{
    // This class is intentionally empty.
    // xUnit uses it to create the fixture instance and share it across test classes
    // that are decorated with [Collection("DataLoaderQueryCount")].
}
