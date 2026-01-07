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
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FamilyHub.Tests.Integration.Auth.GraphQL;

/// <summary>
/// Integration tests for CreateFamily GraphQL mutation.
/// Tests GraphQL API layer, authentication, validation, and error handling.
/// </summary>
[Collection("Database")]
public sealed class CreateFamilyMutationTests : IDisposable
{
    private readonly GraphQlTestFactory _factory;

    public CreateFamilyMutationTests(PostgreSqlContainerFixture containerFixture)
    {
        _factory = new GraphQlTestFactory(containerFixture);
    }

    #region Helper Methods

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

    #endregion

    [Fact]
    public async Task CreateFamily_WithValidInput_ReturnsSuccessPayload()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, familyRepo, unitOfWork, "valid");
        var testId = TestDataFactory.GenerateTestId();
        var familyName = $"GraphQL Test Family {testId}";

        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        var mutation = $$"""
        mutation {
          createFamily(input: { name: "{{familyName}}" }) {
            createdFamilyDto {
              id
              name
              ownerId
              createdAt
            }
            errors {
              __typename
              ... on ValidationError {
                message
                field
              }
              ... on BusinessError {
                message
                code
              }
              ... on ValueObjectError {
                message
              }
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

        // Check for errors - should be null/empty on success
        var errors = createFamily.GetProperty("errors");
        (errors.ValueKind == JsonValueKind.Null || errors.GetArrayLength() == 0).Should().BeTrue();

        // Check family data
        var familyDto = createFamily.GetProperty("createdFamilyDto");
        familyDto.GetProperty("name").GetString().Should().Be(familyName);
        familyDto.GetProperty("ownerId").GetGuid().Should().Be(user.Id.Value);

        var familyId = familyDto.GetProperty("id").GetGuid();
        familyId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateFamily_WithEmptyName_ReturnsValidationError()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, familyRepo, unitOfWork, "empty");
        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        var mutation = """
        mutation {
          createFamily(input: { name: "" }) {
            createdFamilyDto {
              id
              name
            }
            errors {
              __typename
              ... on ValidationError {
                message
                field
              }
              ... on BusinessError {
                message
                code
              }
              ... on ValueObjectError {
                message
              }
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

        // Check for errors array
        var errors = createFamily.GetProperty("errors");
        errors.GetArrayLength().Should().BeGreaterThan(0);

        var firstError = errors[0];
        // Empty string fails at ValueObject level (Vogen FamilyName validation), not FluentValidation
        firstError.GetProperty("__typename").GetString().Should().Be("ValueObjectError");
        firstError.GetProperty("message").GetString().Should().Contain("Family name");

        // createdFamilyDto should be null
        var familyDto = createFamily.GetProperty("createdFamilyDto");
        familyDto.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task CreateFamily_WithNameTooLong_ReturnsValidationError()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, familyRepo, unitOfWork, "long");
        var longName = new string('A', 101); // Exceeds 100 character limit
        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        var mutation = $$"""
        mutation {
          createFamily(input: { name: "{{longName}}" }) {
            createdFamilyDto {
              id
              name
            }
            errors {
              __typename
              ... on ValidationError {
                message
                field
              }
              ... on BusinessError {
                message
                code
              }
              ... on ValueObjectError {
                message
              }
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

        // Check for errors array
        var errors = createFamily.GetProperty("errors");
        errors.GetArrayLength().Should().BeGreaterThan(0);

        var firstError = errors[0];
        // Too long name fails at ValueObject level (Vogen FamilyName validation), not FluentValidation
        firstError.GetProperty("__typename").GetString().Should().Be("ValueObjectError");
        firstError.GetProperty("message").GetString().Should().Contain("100 characters");

        // createdFamilyDto should be null
        var familyDto = createFamily.GetProperty("createdFamilyDto");
        familyDto.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact(Skip = "Authentication scoping issue with TestCurrentUserService in GraphQL tests - needs investigation")]
    public async Task CreateFamily_WhenUserHasFamily_ReturnsGraphQLErrorWithCorrectCode()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Create user and family using direct command (no GraphQL)
        var user = await TestDataFactory.CreateUserAsync(userRepo, familyRepo, unitOfWork, "existing");

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
            createdFamilyDto {
              id
              name
            }
            errors {
              __typename
              ... on ValidationError {
                message
                field
              }
              ... on BusinessError {
                message
                code
              }
              ... on ValueObjectError {
                message
              }
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

        // Check for errors array
        var errors = createFamily.GetProperty("errors");
        errors.GetArrayLength().Should().BeGreaterThan(0);

        var firstError = errors[0];
        firstError.GetProperty("__typename").GetString().Should().Be("BusinessError");
        firstError.GetProperty("code").GetString().Should().Be("FAMILY_ALREADY_EXISTS");
        firstError.GetProperty("message").GetString().Should().Contain("already");

        // createdFamilyDto should be null
        var familyDto = createFamily.GetProperty("createdFamilyDto");
        familyDto.ValueKind.Should().Be(JsonValueKind.Null);

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
            createdFamilyDto {
              id
              name
            }
            errors {
              __typename
              ... on ValidationError {
                message
                field
              }
              ... on BusinessError {
                message
                code
              }
              ... on UnauthorizedError {
                message
              }
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

        // Check for errors array
        var errors = createFamily.GetProperty("errors");
        errors.GetArrayLength().Should().BeGreaterThan(0);

        var firstError = errors[0];
        firstError.GetProperty("__typename").GetString().Should().Be("UnauthorizedError");
        firstError.GetProperty("message").GetString().Should().Contain("authenticated");
    }

    [Fact]
    public async Task CreateFamily_WithMalformedInput_ReturnsBadRequest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, familyRepo, unitOfWork, "malformed");
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
        var (userRepo, familyRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, familyRepo, unitOfWork, "null");
        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        var mutation = """
        mutation {
          createFamily(input: { name: null }) {
            createdFamilyDto {
              id
              name
            }
            errors {
              __typename
              ... on ValidationError {
                message
                field
              }
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

    public void Dispose()
    {
        _factory.Dispose();
    }
}
