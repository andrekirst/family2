using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Integration.Helpers;
using FamilyHub.Tests.Integration.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Tests.Integration.Auth;

/// <summary>
/// Integration tests for Zitadel OAuth 2.0 authorization code flow.
/// Tests the complete flow from authorization code exchange to user creation.
/// </summary>
[Collection("Database")]
public sealed class ZitadelOAuthFlowTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly TestHttpMessageHandler _testHttpMessageHandler;

    public ZitadelOAuthFlowTests(PostgreSqlContainerFixture containerFixture)
    {
        _testHttpMessageHandler = new TestHttpMessageHandler();

        var baseFactory = new TestApplicationFactory(containerFixture);
        _factory = baseFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace HttpClient with test version
                var httpClientDescriptor = services.FirstOrDefault(d =>
                    d.ServiceType == typeof(IHttpClientFactory));
                if (httpClientDescriptor != null)
                {
                    services.Remove(httpClientDescriptor);
                }

                services.AddSingleton<IHttpClientFactory>(_ =>
                {
                    var mockFactory = Substitute.For<IHttpClientFactory>();
                    var httpClient = new HttpClient(_testHttpMessageHandler);
                    mockFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
                    return mockFactory;
                });

                // Configure Zitadel settings for testing
                services.Configure<ZitadelSettings>(settings =>
                {
                    settings.GetType().GetProperty(nameof(ZitadelSettings.Authority))!
                        .SetValue(settings, "http://localhost:8080");
                    settings.GetType().GetProperty(nameof(ZitadelSettings.ClientId))!
                        .SetValue(settings, "test-client-id");
                    settings.GetType().GetProperty(nameof(ZitadelSettings.ClientSecret))!
                        .SetValue(settings, "test-client-secret");
                    settings.GetType().GetProperty(nameof(ZitadelSettings.RedirectUri))!
                        .SetValue(settings, "http://localhost:4200/auth/callback");
                    settings.GetType().GetProperty(nameof(ZitadelSettings.Audience))!
                        .SetValue(settings, "family-hub-api");
                });
            });
        });
    }

    /// <summary>
    /// Test HTTP message handler that allows configuring responses.
    /// </summary>
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        public HttpResponseMessage? Response { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Response ?? new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    [Fact]
    public async Task CompleteZitadelLogin_NewUser_CreatesUserViaCreateFromOAuth()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (mediator, userRepository) = TestServices.ResolveOAuthServices(scope);

        var testId = TestDataFactory.GenerateTestId();
        var zitadelUserId = $"zitadel-user-{testId}";
        var email = $"newuser-{testId}@example.com";

        // Mock Zitadel token endpoint response
        SetupMockHttpResponse(zitadelUserId, email);

        var command = new CompleteZitadelLoginCommand(
            AuthorizationCode: "mock_authorization_code",
            CodeVerifier: "mock_code_verifier"
        );

        // Act
        var result = await mediator.Send<CompleteZitadelLoginResult>(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email.Value);
        Assert.True(result.EmailVerified);

        // Verify user was created in database
        var user = await userRepository.GetByIdAsync(result.UserId);
        Assert.NotNull(user);
        Assert.Equal("zitadel", user.ExternalProvider);
        Assert.Equal(zitadelUserId, user.ExternalUserId);
        Assert.True(user.EmailVerified);
        Assert.Equal(email, user.Email.Value);
    }

    [Fact]
    public async Task CompleteZitadelLogin_ExistingUser_ReturnsExistingUser()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (mediator, userRepository) = TestServices.ResolveOAuthServices(scope);
        var familyRepository = scope.ServiceProvider.GetRequiredService<IFamilyRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<SharedKernel.Interfaces.IUnitOfWork>();

        var testId = TestDataFactory.GenerateTestId();
        var zitadelUserId = $"zitadel-user-existing-{testId}";
        var email = $"existing-{testId}@example.com";

        // Create family first (required by foreign key constraint)
        var family = FamilyAggregate.Create(FamilyName.From($"Existing Family {testId}"), UserId.New());
        await familyRepository.AddAsync(family);
        await unitOfWork.SaveChangesAsync();

        // Create existing user
        var existingUser = User.CreateFromOAuth(
            Email.From(email),
            zitadelUserId,
            "zitadel",
            family.Id
        );
        await userRepository.AddAsync(existingUser);

        // Save changes to persist the existing user
        await unitOfWork.SaveChangesAsync();

        // Mock Zitadel token endpoint response
        SetupMockHttpResponse(zitadelUserId, email);

        var command = new CompleteZitadelLoginCommand(
            AuthorizationCode: "mock_authorization_code",
            CodeVerifier: "mock_code_verifier"
        );

        // Act
        var result = await mediator.Send<CompleteZitadelLoginResult>(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingUser.Id, result.UserId);
        Assert.Equal(email, result.Email.Value);
        Assert.True(result.EmailVerified);
    }

    [Fact]
    public async Task CompleteZitadelLogin_InvalidAuthorizationCode_ThrowsException()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Mock failed token exchange
        _testHttpMessageHandler.Response = new()
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = JsonContent.Create(new { error = "invalid_grant" })
        };

        var command = new CompleteZitadelLoginCommand(
            AuthorizationCode: "invalid_code",
            CodeVerifier: "mock_code_verifier"
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await mediator.Send<CompleteZitadelLoginResult>(command)
        );
    }

    [Fact]
    public async Task CompleteZitadelLogin_ReturnsValidAccessToken()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var testId = Guid.NewGuid().ToString("N")[..8];
        var zitadelUserId = $"zitadel-user-token-{testId}";
        var email = $"tokentest-{testId}@example.com";

        // Mock Zitadel token endpoint response
        SetupMockHttpResponse(zitadelUserId, email);

        var command = new CompleteZitadelLoginCommand(
            AuthorizationCode: "mock_authorization_code",
            CodeVerifier: "mock_code_verifier"
        );

        // Act
        var result = await mediator.Send<CompleteZitadelLoginResult>(command);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AccessToken); // AccessToken contains the ID token (JWT)
        Assert.StartsWith("eyJ", result.AccessToken); // JWT tokens start with "eyJ"
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    /// <summary>
    /// Creates a mock ID token (JWT) for testing.
    /// </summary>
    private string CreateMockIdToken(string zitadelUserId, string email)
    {
        var claims = new List<Claim>
        {
            new("sub", zitadelUserId),
            new("email", email),
            new("email_verified", "true"),
            new("name", "Test User"), // Added for family name generation
            new("iss", "http://localhost:8080"),
            new("aud", "family-hub-api"),
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString())
        };

        var handler = new JwtSecurityTokenHandler();
        var token = new JwtSecurityToken(
            issuer: "http://localhost:8080",
            audience: "family-hub-api",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: null // No signature for mock
        );

        return handler.WriteToken(token);
    }

    /// <summary>
    /// Sets up the test HTTP handler to return a token response.
    /// </summary>
    private void SetupMockHttpResponse(
        string zitadelUserId,
        string email,
        string accessToken = "mock_access_token")
    {
        var idToken = CreateMockIdToken(zitadelUserId, email);

        _testHttpMessageHandler.Response = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(new
            {
                access_token = accessToken,
                id_token = idToken,
                expires_in = 3600,
                token_type = "Bearer"
            })
        };
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
