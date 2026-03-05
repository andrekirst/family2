using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FamilyHub.IntegrationTests.Fixtures;
using FluentAssertions;

namespace FamilyHub.IntegrationTests.E2E;

/// <summary>
/// End-to-end tests using real Keycloak and PostgreSQL containers.
/// These tests validate the full auth flow with real OIDC tokens and RLS.
/// Requires Docker to run.
/// </summary>
[Collection("E2E")]
[Trait("Category", "E2E")]
public class AuthFlowE2ETests : IAsyncLifetime
{
    private readonly KeycloakContainerFixture _keycloak;
    private readonly PostgresContainerFixture _postgres;
    private FullStackWebApplicationFactory _factory = null!;

    public AuthFlowE2ETests(
        KeycloakContainerFixture keycloak,
        PostgresContainerFixture postgres)
    {
        _keycloak = keycloak;
        _postgres = postgres;
    }

    public ValueTask InitializeAsync()
    {
        _factory = new FullStackWebApplicationFactory(_keycloak, _postgres);
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task HealthAuthEndpoint_WithRealKeycloak_ReturnsHealthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/auth");

        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var checks = doc.RootElement.GetProperty("checks");

        // With real Keycloak, the OIDC and JWT checks should be healthy
        checks.GetProperty("keycloak_oidc").GetProperty("status").GetString()
            .Should().Be("Healthy");
        checks.GetProperty("jwt_signing_keys").GetProperty("status").GetString()
            .Should().Be("Healthy");
        checks.GetProperty("graphql_schema").GetProperty("status").GetString()
            .Should().Be("Healthy");
    }

    [Fact]
    public async Task RegisterUser_WithRealKeycloakToken_ReturnsUser()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var mutation = new
        {
            query = """
                mutation {
                    registerUser {
                        email
                        displayName
                    }
                }
                """
        };

        var response = await client.PostAsJsonAsync("/graphql", mutation);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(KeycloakContainerFixture.TestUserEmail);
    }

    [Fact]
    public async Task GraphQLEndpoint_WithRealToken_NotFourOhFour()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var query = new { query = "{ __typename }" };

        var response = await client.PostAsJsonAsync("/graphql", query);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "GraphQL endpoint must never return 404 with real Keycloak auth");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RlsMiddleware_WithRealToken_SetsSessionVariables()
    {
        // First register the user so RLS can find them
        var client = await _factory.CreateAuthenticatedClientAsync();

        var registerMutation = new
        {
            query = """
                mutation {
                    registerUser {
                        id
                        email
                    }
                }
                """
        };

        var registerResponse = await client.PostAsJsonAsync("/graphql", registerMutation);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Now make a subsequent request — RLS middleware should set session vars
        var query = new
        {
            query = """
                query {
                    getCurrentUser {
                        email
                    }
                }
                """
        };

        var response = await client.PostAsJsonAsync("/graphql", query);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(KeycloakContainerFixture.TestUserEmail);
    }
}
