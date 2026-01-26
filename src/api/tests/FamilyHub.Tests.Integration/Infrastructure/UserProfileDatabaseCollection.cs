namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Test collection for UserProfile database integration tests.
/// Shares a single PostgreSQL container across all tests in the collection.
/// </summary>
[CollectionDefinition("UserProfileDatabase")]
public sealed class UserProfileDatabaseCollection : ICollectionFixture<UserProfilePostgreSqlContainerFixture>
{
    // This class has no code, it's just used to define the collection
}
