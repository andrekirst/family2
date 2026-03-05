using FamilyHub.IntegrationTests.Fixtures;

namespace FamilyHub.IntegrationTests.E2E;

/// <summary>
/// Shared collection fixture for E2E tests. Starts Keycloak and PostgreSQL containers once
/// and shares them across all test classes in the collection.
/// </summary>
[CollectionDefinition("E2E")]
public class E2ETestCollection
    : ICollectionFixture<KeycloakContainerFixture>,
      ICollectionFixture<PostgresContainerFixture>;
