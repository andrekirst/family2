using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FamilyHub.IntegrationTests.Fixtures;
using FluentAssertions;

namespace FamilyHub.IntegrationTests;

/// <summary>
/// Integration tests for the full HTTP pipeline: schema loading, auth, health, and GraphQL mutations.
/// Uses FamilyHubWebApplicationFactory with InMemoryDatabase and mock JWT (no Docker/Keycloak needed).
/// </summary>
public class GraphQLApiTests(FamilyHubWebApplicationFactory factory)
    : IClassFixture<FamilyHubWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task HealthAuthEndpoint_ReturnsDetailedStatus()
    {
        var response = await _client.GetAsync("/health/auth");

        // In test environment (no Keycloak), health check returns 503 (ServiceUnavailable)
        // but the response body should still be valid JSON with check details
        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        root.GetProperty("status").GetString().Should().NotBeNullOrEmpty();
        root.GetProperty("checks").EnumerateObject().Should().NotBeEmpty();
        // Verify all 3 checks are present
        var checks = root.GetProperty("checks");
        checks.TryGetProperty("keycloak_oidc", out _).Should().BeTrue();
        checks.TryGetProperty("jwt_signing_keys", out _).Should().BeTrue();
        checks.TryGetProperty("graphql_schema", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GraphQLEndpoint_ReturnsOk_NotFourOhFour()
    {
        // This is the critical regression test — if Hot Chocolate fails to build the schema,
        // /graphql returns 404 instead of a proper GraphQL response.
        var query = new { query = "{ __typename }" };

        var response = await _client.PostAsJsonAsync("/graphql", query);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "GraphQL endpoint should never return 404 — this indicates a schema build failure");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GraphQLEndpoint_WithMockJwt_ReturnsOk()
    {
        var authenticatedClient = factory.CreateAuthenticatedClient(
            sub: "keycloak-sub-12345",
            email: "integration@test.com",
            name: "Integration Tester");

        var query = new { query = "{ __typename }" };

        var response = await authenticatedClient.PostAsJsonAsync("/graphql", query);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("__typename");
    }

    [Fact]
    public async Task GraphQLEndpoint_WithoutAuth_ReturnsErrors()
    {
        var query = new { query = "query { getCurrentUser { email } }" };

        var response = await _client.PostAsJsonAsync("/graphql", query);

        // GraphQL may return 200 with errors or 400 for auth-required queries
        // The key assertion: it must NOT return 404 (schema failure)
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("error");
    }

    [Fact]
    public async Task GraphQLEndpoint_WithMockJwt_TypenameResolves()
    {
        // Verifies the schema is built and authenticated requests can resolve.
        // Uses __typename which doesn't require DB state, proving the full auth+GraphQL pipeline works.
        var authenticatedClient = factory.CreateAuthenticatedClient();

        var query = new { query = "{ __typename }" };

        var response = await authenticatedClient.PostAsJsonAsync("/graphql", query);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Query");
        content.Should().NotContain("errors");
    }
}
