namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// xUnit collection definition for sharing the dual-schema PostgreSQL container
/// across cross-DbContext consistency tests.
/// All test classes marked with [Collection("DualSchema")] will share the same container instance.
/// This amortizes the container startup cost and ensures both schemas are initialized.
/// </summary>
[CollectionDefinition("DualSchema")]
public class DualSchemaCollection : ICollectionFixture<DualSchemaPostgreSqlContainerFixture>
{
    // This class is intentionally empty.
    // xUnit uses it to create the fixture instance and share it across test classes.
}
