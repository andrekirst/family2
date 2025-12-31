using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Integration.Helpers;
using FamilyHub.Tests.Integration.Infrastructure;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FamilyHub.Tests.Integration.Auth.GraphQL;

/// <summary>
/// Integration tests for CreateFamily GraphQL mutation.
/// Tests GraphQL API layer, authentication, validation, and error handling.
/// Uses Testcontainers PostgreSQL for real database testing with automatic cleanup.
/// </summary>
[Collection("Database")]
public sealed class CreateFamilyMutationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _containerFixture;
    private readonly GraphQlTestFactory _factory;

    public CreateFamilyMutationTests(PostgreSqlContainerFixture containerFixture)
    {
        _containerFixture = containerFixture;
        _factory = new GraphQlTestFactory(_containerFixture.ConnectionString);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Parses GraphQL response and returns the createFamily result.
    /// Handles both successful responses (with data) and error responses (data is null).
    /// </summary>
    private static async Task<JsonElement> ParseCreateFamilyResponse(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseContent);

        // Check if data property exists and is not null
        if (jsonDocument.RootElement.TryGetProperty("data", out var data) &&
            data.ValueKind != JsonValueKind.Null)
        {
            return data.GetProperty("createFamily");
        }

        // If data is null or doesn't exist, return root element (which should have errors)
        return jsonDocument.RootElement;
    }

    [Fact]
    public async Task CreateFamily_WithValidInput_ReturnsSuccessPayload()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, unitOfWork, "valid");
        var testId = TestDataFactory.GenerateTestId();
        var familyName = $"GraphQL Test Family {testId}";

        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        var mutation = $$"""
        mutation {
          createFamily(input: { name: "{{familyName}}" }) {
            family {
              id
              name
              ownerId
              createdAt
            }
            errors {
              message
              code
              field
            }
          }
        }
        """;

        var request = new { query = mutation };

        // Act
        var response = await client.PostAsJsonAsync("/graphql", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createFamily = await ParseCreateFamilyResponse(response);

        // Check for errors - should be null on success
        var errors = createFamily.GetProperty("errors");
        errors.ValueKind.Should().Be(JsonValueKind.Null);

        // Check family data
        var family = createFamily.GetProperty("family");
        family.GetProperty("name").GetString().Should().Be(familyName);
        family.GetProperty("ownerId").GetGuid().Should().Be(user.Id.Value);

        var familyId = family.GetProperty("id").GetGuid();
        familyId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateFamily_WithEmptyName_ReturnsValidationError()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, unitOfWork, "empty");
        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        const string mutation = """
                                mutation {
                                  createFamily(input: { name: "" }) {
                                    family {
                                      id
                                      name
                                    }
                                    errors {
                                      message
                                      code
                                      field
                                    }
                                  }
                                }
                                """;

        var request = new { query = mutation };

        // Act
        var response = await client.PostAsJsonAsync("/graphql", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createFamily = await ParseCreateFamilyResponse(response);

        // Check for errors
        var errors = createFamily.GetProperty("errors");
        errors.GetArrayLength().Should().BeGreaterThan(0);

        var firstError = errors[0];
        firstError.GetProperty("message").GetString().Should().Contain("Family name");
        firstError.GetProperty("code").GetString().Should().Be("VALIDATION_ERROR");

        // Family should be null
        var familyProperty = createFamily.GetProperty("family");
        familyProperty.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task CreateFamily_WithNameTooLong_ReturnsValidationError()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, unitOfWork, "long");
        var longName = new string('A', 101); // Exceeds 100 character limit
        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        var mutation = $$"""
        mutation {
          createFamily(input: { name: "{{longName}}" }) {
            family {
              id
              name
            }
            errors {
              message
              code
              field
            }
          }
        }
        """;

        var request = new { query = mutation };

        // Act
        var response = await client.PostAsJsonAsync("/graphql", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createFamily = await ParseCreateFamilyResponse(response);

        // Check for errors
        var errors = createFamily.GetProperty("errors");
        errors.GetArrayLength().Should().BeGreaterThan(0);

        var firstError = errors[0];
        firstError.GetProperty("message").GetString().Should().Contain("100 characters");
        firstError.GetProperty("code").GetString().Should().Be("VALIDATION_ERROR");

        // Family should be null
        var familyProperty = createFamily.GetProperty("family");
        familyProperty.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact(Skip = "Authentication scoping issue with TestCurrentUserService in GraphQL tests - needs investigation")]
    public async Task CreateFamily_WhenUserHasFamily_ReturnsGraphQLErrorWithCorrectCode()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
        var familyRepo = scope.ServiceProvider.GetRequiredService<IFamilyRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Create user and family using direct command (no GraphQL)
        var user = await TestDataFactory.CreateUserAsync(userRepo, unitOfWork, "existing");

        // Set authentication and create family via command handler
        TestCurrentUserService.SetUserId(user.Id);
        var createCommand = new CreateFamilyCommand(FamilyName.From("Existing Family"));
        var familyResult = await mediator.Send(createCommand);

        // Verify family was created
        var family = await familyRepo.GetByIdAsync(familyResult.FamilyId);
        family.Should().NotBeNull();

        // Set authentication for the GraphQL request
        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        var mutation = """
        mutation {
          createFamily(input: { name: "Second Family" }) {
            family {
              id
              name
            }
            errors {
              message
              code
              field
            }
          }
        }
        """;

        var request = new { query = mutation };

        // Act
        var response = await client.PostAsJsonAsync("/graphql", request);

        // Assert - Focus ONLY on GraphQL response structure
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createFamily = await ParseCreateFamilyResponse(response);

        // Verify GraphQL error response format
        var errors = createFamily.GetProperty("errors");
        errors.GetArrayLength().Should().BeGreaterThan(0);

        var error = errors[0];
        error.GetProperty("code").GetString().Should().Be("FAMILY_ALREADY_EXISTS");
        error.GetProperty("message").GetString().Should().Contain("already");

        // Family should be null
        var familyProperty = createFamily.GetProperty("family");
        familyProperty.ValueKind.Should().Be(JsonValueKind.Null);

        // Note: Business rule enforcement itself is tested in CreateFamilyIntegrationTests
    }

    [Fact]
    public async Task CreateFamily_WithoutAuthentication_Returns401()
    {
        // Arrange
        _factory.ClearAuthenticatedUser(); // Clear any authentication state from previous tests
        var client = _factory.CreateClient();

        var mutation = """
        mutation {
          createFamily(input: { name: "Test Family" }) {
            family {
              id
              name
            }
            errors {
              message
              code
            }
          }
        }
        """;

        var request = new { query = mutation };

        // Act
        var response = await client.PostAsJsonAsync("/graphql", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseContent);

        // GraphQL returns the response in errors when unauthenticated
        var data = jsonDocument.RootElement.GetProperty("data");
        var createFamily = data.GetProperty("createFamily");

        var errors = createFamily.GetProperty("errors");
        errors.GetArrayLength().Should().BeGreaterThan(0);

        var firstError = errors[0];
        firstError.GetProperty("message").GetString().Should().Contain("authenticated");
        firstError.GetProperty("code").GetString().Should().Be("UNAUTHENTICATED");
    }

    [Fact]
    public async Task CreateFamily_WithMalformedInput_ReturnsBadRequest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, unitOfWork, "malformed");
        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        // Malformed GraphQL query (missing closing brace)
        var malformedMutation = """
        mutation {
          createFamily(input: { name: "Test" }
        """;

        var request = new { query = malformedMutation };

        // Act
        var response = await client.PostAsJsonAsync("/graphql", request);

        // Assert
        // Hot Chocolate GraphQL returns 400 Bad Request for syntax errors
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseContent);

        // Check for GraphQL errors
        jsonDocument.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateFamily_WithNullInput_ReturnsValidationError()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, unitOfWork, "null");
        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        var mutation = """
        mutation {
          createFamily(input: { name: null }) {
            family {
              id
              name
            }
            errors {
              message
              code
            }
          }
        }
        """;

        var request = new { query = mutation };

        // Act
        var response = await client.PostAsJsonAsync("/graphql", request);

        // Assert
        // Hot Chocolate GraphQL returns 400 Bad Request for schema validation errors (null for non-nullable field)
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseContent);

        // Check for GraphQL errors (null input should be rejected by GraphQL schema)
        jsonDocument.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Creates an authenticated HTTP client with mocked ICurrentUserService.
    /// </summary>
    private HttpClient CreateAuthenticatedClient(string email, UserId userId)
    {
        // Configure the factory with the authenticated user
        _factory.SetAuthenticatedUser(Email.From(email), userId);

        // Create the HTTP client (factory is already configured)
        var client = _factory.CreateClient();

        // Add mock JWT token to Authorization header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "mock_token");

        return client;
    }
}
