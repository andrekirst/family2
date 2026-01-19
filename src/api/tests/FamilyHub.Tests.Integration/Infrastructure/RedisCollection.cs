namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// xUnit collection definition for Redis integration tests.
/// All test classes decorated with [Collection("Redis")] share the same RedisContainerFixture.
/// </summary>
[CollectionDefinition("Redis")]
public sealed class RedisCollection : ICollectionFixture<RedisContainerFixture>
{
    // xUnit automatically creates fixture once per collection
}
