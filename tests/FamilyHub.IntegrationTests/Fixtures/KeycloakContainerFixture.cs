using System.Net.Http.Json;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using Testcontainers.Keycloak;

namespace FamilyHub.IntegrationTests.Fixtures;

/// <summary>
/// Starts a real Keycloak container, imports the FamilyHub realm, creates a test user,
/// and provides methods to obtain access tokens for integration tests.
/// </summary>
public class KeycloakContainerFixture : IAsyncLifetime
{
    private KeycloakContainer _container = null!;
    private string _baseUrl = null!;

    public string BaseUrl => _baseUrl;
    public string Authority => $"{_baseUrl}/realms/FamilyHub";
    public string Issuer => Authority;

    public const string TestUserEmail = "e2e-test@familyhub.local";
    public const string TestUserPassword = "TestPassword123!";
    public const string TestUserName = "E2E Test User";
    public const string RealmName = "FamilyHub";
    public const string ClientId = "familyhub-web";

    public async ValueTask InitializeAsync()
    {
        var realmPath = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..",
                "infrastructure", "keycloak", "realm-base.json"));

        var containerBuilder = new KeycloakBuilder()
            .WithImage("quay.io/keycloak/keycloak:26.5.4")
            .WithResourceMapping(realmPath, "/opt/keycloak/data/import/realm-base.json")
            .WithCommand("--import-realm")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(r => r.ForPath("/health/ready").ForPort(8080)));

        _container = containerBuilder.Build();
        await _container.StartAsync();

        _baseUrl = _container.GetBaseAddress().TrimEnd('/');

        // Create test user via Keycloak Admin REST API
        await CreateTestUser();
    }

    public async Task<string> GetAccessTokenAsync()
    {
        using var httpClient = new HttpClient();
        var tokenEndpoint = $"{Authority}/protocol/openid-connect/token";

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = ClientId,
            ["username"] = TestUserEmail,
            ["password"] = TestUserPassword,
            ["scope"] = "openid profile email",
        });

        var response = await httpClient.PostAsync(tokenEndpoint, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("access_token").GetString()!;
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    private async Task CreateTestUser()
    {
        using var httpClient = new HttpClient();

        // Get admin token
        var adminTokenEndpoint = $"{_baseUrl}/realms/master/protocol/openid-connect/token";
        var adminContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = KeycloakBuilder.DefaultUsername,
            ["password"] = KeycloakBuilder.DefaultPassword,
        });

        var adminResponse = await httpClient.PostAsync(adminTokenEndpoint, adminContent);
        adminResponse.EnsureSuccessStatusCode();
        var adminJson = await adminResponse.Content.ReadFromJsonAsync<JsonElement>();
        var adminToken = adminJson.GetProperty("access_token").GetString()!;

        // Create user
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var userPayload = new
        {
            username = TestUserEmail,
            email = TestUserEmail,
            firstName = "E2E Test",
            lastName = "User",
            emailVerified = true,
            enabled = true,
            credentials = new[]
            {
                new { type = "password", value = TestUserPassword, temporary = false }
            }
        };

        var createResponse = await httpClient.PostAsJsonAsync(
            $"{_baseUrl}/admin/realms/{RealmName}/users", userPayload);

        // 201 Created or 409 Conflict (user already exists from realm import)
        if (!createResponse.IsSuccessStatusCode && createResponse.StatusCode != System.Net.HttpStatusCode.Conflict)
        {
            var error = await createResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create test user: {createResponse.StatusCode} - {error}");
        }
    }
}
