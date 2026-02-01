using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FamilyHub.IntegrationTests;

/// <summary>
/// Integration tests for GraphQL API
/// Tests the full HTTP pipeline including authentication, GraphQL, and database
/// </summary>
public class GraphQLApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GraphQLApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("healthy", content);
    }

    [Fact]
    public async Task GraphQLEndpoint_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var query = new
        {
            query = "query { getCurrentUser { email } }"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/graphql", query);

        // Assert
        // GraphQL returns 200 with errors for unauthorized queries
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("errors", content);
    }
}
