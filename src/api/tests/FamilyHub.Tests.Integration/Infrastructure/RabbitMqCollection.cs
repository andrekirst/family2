namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Collection definition for RabbitMQ integration tests.
/// Tests in this collection share a single RabbitMQ container instance.
/// </summary>
[CollectionDefinition("RabbitMQ")]
public class RabbitMqCollection : ICollectionFixture<RabbitMqContainerFixture>
{
    // This class is intentionally empty.
    // xUnit uses it to create the fixture instance and share it across test classes.
}
