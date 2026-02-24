using System.Net;
using System.Text.Json;
using FamilyHub.Api.Common.Infrastructure.Configuration.Infisical;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace FamilyHub.Infisical.Tests;

public class InfisicalConfigurationProviderTests
{
    private static InfisicalOptions CreateOptions() => new()
    {
        Url = "http://infisical.test",
        ProjectId = "proj-123",
        Environment = "dev",
        SecretPath = "/",
        ClientId = "client-id",
        ClientSecret = "client-secret",
    };

    private static HttpClient CreateMockHttpClient(
        Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        return new HttpClient(new MockHttpHandler(handler))
        {
            BaseAddress = new Uri("http://infisical.test"),
        };
    }

    [Fact]
    public void Load_WithValidSecrets_PopulatesConfiguration()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(request =>
        {
            if (request.RequestUri!.PathAndQuery.Contains("/api/v1/auth/universal-auth/login"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { accessToken = "test-token" }),
                        System.Text.Encoding.UTF8,
                        "application/json"),
                };
            }

            if (request.RequestUri.PathAndQuery.Contains("/api/v4/secrets"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(new
                        {
                            secrets = new[]
                            {
                                new { secretKey = "GoogleIntegration__OAuth__ClientId", secretValue = "google-id" },
                                new { secretKey = "GoogleIntegration__OAuth__ClientSecret", secretValue = "google-secret" },
                                new { secretKey = "GoogleIntegration__EncryptionKey", secretValue = "enc-key-123" },
                            },
                        }),
                        System.Text.Encoding.UTF8,
                        "application/json"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var provider = new InfisicalConfigurationProvider(CreateOptions(), httpClient);

        // Act
        provider.Load();

        // Assert
        provider.TryGet("GoogleIntegration:OAuth:ClientId", out var clientId).Should().BeTrue();
        clientId.Should().Be("google-id");

        provider.TryGet("GoogleIntegration:OAuth:ClientSecret", out var clientSecret).Should().BeTrue();
        clientSecret.Should().Be("google-secret");

        provider.TryGet("GoogleIntegration:EncryptionKey", out var encKey).Should().BeTrue();
        encKey.Should().Be("enc-key-123");
    }

    [Fact]
    public void Load_WithAuthFailure_ReturnsEmptyConfiguration()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(_ =>
            new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var provider = new InfisicalConfigurationProvider(CreateOptions(), httpClient);

        // Act
        provider.Load();

        // Assert — no exception thrown, empty configuration
        provider.TryGet("GoogleIntegration:OAuth:ClientId", out _).Should().BeFalse();
    }

    [Fact]
    public void Load_WithNetworkError_ReturnsEmptyConfiguration()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(_ =>
            throw new HttpRequestException("Connection refused"));

        var provider = new InfisicalConfigurationProvider(CreateOptions(), httpClient);

        // Act
        provider.Load();

        // Assert — graceful degradation, no exception
        provider.TryGet("GoogleIntegration:OAuth:ClientId", out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("Foo__Bar__Baz", "Foo:Bar:Baz")]
    [InlineData("GoogleIntegration__OAuth__ClientId", "GoogleIntegration:OAuth:ClientId")]
    [InlineData("SimpleKey", "SimpleKey")]
    [InlineData("Email__Host", "Email:Host")]
    public void MapSecretKeyToConfigKey_ConvertsCorrectly(string input, string expected)
    {
        var result = InfisicalConfigurationProvider.MapSecretKeyToConfigKey(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void AddInfisical_WithoutEnvVars_SkipsSilently()
    {
        // Arrange — ensure env vars are not set
        System.Environment.SetEnvironmentVariable("INFISICAL_CLIENT_ID", null);
        System.Environment.SetEnvironmentVariable("INFISICAL_CLIENT_SECRET", null);

        var configBuilder = new ConfigurationBuilder();

        // Act
        configBuilder.AddInfisical();

        // Assert — no Infisical source added
        configBuilder.Sources.Should().BeEmpty();
    }

    [Fact]
    public void AddInfisical_WithEnvVars_AddsSource()
    {
        // Arrange
        System.Environment.SetEnvironmentVariable("INFISICAL_CLIENT_ID", "test-id");
        System.Environment.SetEnvironmentVariable("INFISICAL_CLIENT_SECRET", "test-secret");
        System.Environment.SetEnvironmentVariable("INFISICAL_URL", "http://vault.test:8180");
        System.Environment.SetEnvironmentVariable("INFISICAL_PROJECT_ID", "proj-456");

        try
        {
            var configBuilder = new ConfigurationBuilder();

            // Act
            configBuilder.AddInfisical();

            // Assert
            configBuilder.Sources.Should().ContainSingle()
                .Which.Should().BeOfType<InfisicalConfigurationSource>();

            var source = (InfisicalConfigurationSource)configBuilder.Sources[0];
            source.Options.Url.Should().Be("http://vault.test:8180");
            source.Options.ProjectId.Should().Be("proj-456");
            source.Options.ClientId.Should().Be("test-id");
            source.Options.ClientSecret.Should().Be("test-secret");
        }
        finally
        {
            // Cleanup
            System.Environment.SetEnvironmentVariable("INFISICAL_CLIENT_ID", null);
            System.Environment.SetEnvironmentVariable("INFISICAL_CLIENT_SECRET", null);
            System.Environment.SetEnvironmentVariable("INFISICAL_URL", null);
            System.Environment.SetEnvironmentVariable("INFISICAL_PROJECT_ID", null);
        }
    }

    [Fact]
    public void Load_SendsCorrectAuthPayload()
    {
        // Arrange
        string? capturedBody = null;
        var httpClient = CreateMockHttpClient(request =>
        {
            if (request.RequestUri!.PathAndQuery.Contains("/api/v1/auth/universal-auth/login"))
            {
                capturedBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { accessToken = "token" }),
                        System.Text.Encoding.UTF8,
                        "application/json"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { secrets = Array.Empty<object>() }),
                    System.Text.Encoding.UTF8,
                    "application/json"),
            };
        });

        var provider = new InfisicalConfigurationProvider(CreateOptions(), httpClient);

        // Act
        provider.Load();

        // Assert
        capturedBody.Should().NotBeNull();
        var doc = JsonDocument.Parse(capturedBody!);
        doc.RootElement.GetProperty("clientId").GetString().Should().Be("client-id");
        doc.RootElement.GetProperty("clientSecret").GetString().Should().Be("client-secret");
    }

    [Fact]
    public void Load_SendsBearerTokenForSecretsFetch()
    {
        // Arrange
        string? capturedAuth = null;
        var httpClient = CreateMockHttpClient(request =>
        {
            if (request.RequestUri!.PathAndQuery.Contains("/api/v1/auth/universal-auth/login"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { accessToken = "my-bearer-token" }),
                        System.Text.Encoding.UTF8,
                        "application/json"),
                };
            }

            if (request.RequestUri.PathAndQuery.Contains("/api/v4/secrets"))
            {
                capturedAuth = request.Headers.Authorization?.ToString();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { secrets = Array.Empty<object>() }),
                        System.Text.Encoding.UTF8,
                        "application/json"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var provider = new InfisicalConfigurationProvider(CreateOptions(), httpClient);

        // Act
        provider.Load();

        // Assert
        capturedAuth.Should().Be("Bearer my-bearer-token");
    }

    /// <summary>
    /// Delegating handler that routes requests through a custom function for testing.
    /// </summary>
    private sealed class MockHttpHandler(
        Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(handler(request));
    }
}
