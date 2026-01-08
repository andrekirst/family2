using FamilyDomain = FamilyHub.Modules.Family.Domain;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using System.Net.Http.Headers;
using System.Text.Json;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Integration.Helpers;
using FamilyHub.Tests.Integration.Infrastructure;

namespace FamilyHub.Tests.Integration.Auth.GraphQL;

/// <summary>
/// Integration tests for AcceptInvitation GraphQL mutation.
/// Tests validation, authentication, and database state changes.
/// </summary>
[Collection("Database")]
public sealed class AcceptInvitationMutationTests : IDisposable
{
    private readonly GraphQlTestFactory _factory;

    public AcceptInvitationMutationTests(PostgreSqlContainerFixture containerFixture)
    {
        _factory = new GraphQlTestFactory(containerFixture);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    #region Happy Path

    [Fact]
    public async Task AcceptInvitation_WithValidToken_ReturnsSuccessPayload()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
        var invitationRepo = scope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();

        // Create family owner
        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyRepo, unitOfWork, "owner");

        // Create invitation
        var testId = TestDataFactory.GenerateTestId();
        var inviteeEmail = Email.From($"invitee-{testId}@example.com");
        var invitation = FamilyDomain.FamilyMemberInvitation.CreateEmailInvitation(
            owner.FamilyId!,
            inviteeEmail,
            FamilyRole.Member,
            owner.Id,
            "Welcome to the family!");

        await invitationRepo.AddAsync(invitation, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        // Create invitee user (with temporary family, AcceptInvitation will update)
        var tempFamily = FamilyDomain.Family.Create(FamilyName.From("Temp Family"), UserId.New());
        await familyRepo.AddAsync(tempFamily, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var invitee = User.CreateFromOAuth(inviteeEmail, $"ext-invitee-{testId}", "zitadel", tempFamily.Id);
        await userRepo.AddAsync(invitee, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var client = CreateAuthenticatedClient(inviteeEmail.Value, invitee.Id);

        var mutation = $$"""
        mutation {
          acceptInvitation(input: { token: "{{invitation.Token.Value}}" }) {
            family { id name }
            role
            errors { message code }
          }
        }
        """;

        // Act
        var response = await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await ParseAcceptInvitationResponse(response);
        var errors = result.GetProperty("errors");
        errors.ValueKind.Should().Be(JsonValueKind.Null);

        var family = result.GetProperty("family");
        family.GetProperty("id").GetGuid().Should().Be(owner.FamilyId!.Value);

        var role = result.GetProperty("role").GetString();
        role.Should().Be("MEMBER");
    }

    [Fact]
    public async Task AcceptInvitation_WithValidToken_UpdatesDatabaseState()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
        var invitationRepo = scope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyRepo, unitOfWork, "owner");

        var testId = TestDataFactory.GenerateTestId();
        var inviteeEmail = Email.From($"invitee-{testId}@example.com");
        var invitation = FamilyDomain.FamilyMemberInvitation.CreateEmailInvitation(
            owner.FamilyId!,
            inviteeEmail,
            FamilyRole.Admin,
            owner.Id);

        await invitationRepo.AddAsync(invitation, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var tempFamily = FamilyDomain.Family.Create(FamilyName.From("Temp Family"), UserId.New());
        await familyRepo.AddAsync(tempFamily, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var invitee = User.CreateFromOAuth(inviteeEmail, $"ext-invitee-{testId}", "zitadel", tempFamily.Id);
        await userRepo.AddAsync(invitee, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var client = CreateAuthenticatedClient(inviteeEmail.Value, invitee.Id);

        var mutation = $$"""
        mutation {
          acceptInvitation(input: { token: "{{invitation.Token.Value}}" }) {
            family { id }
            role
            errors { message }
          }
        }
        """;

        // Act
        var response = await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Assert - HTTP response
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Database state (use new scope to avoid stale data)
        using var verifyScope = _factory.Services.CreateScope();
        var (verifyUserRepo, _, verifyUnitOfWork) = TestServices.ResolveRepositoryServices(verifyScope);
        var verifyInvitationRepo = verifyScope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();

        var updatedInvitation = await verifyInvitationRepo.GetByTokenAsync(invitation.Token, CancellationToken.None);
        updatedInvitation.Should().NotBeNull();
        updatedInvitation!.Status.Should().Be(InvitationStatus.Accepted);
        updatedInvitation.AcceptedAt.Should().NotBeNull();
        updatedInvitation.AcceptedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var updatedUser = await verifyUserRepo.GetByIdAsync(invitee.Id, CancellationToken.None);
        updatedUser.Should().NotBeNull();
        updatedUser!.FamilyId.Should().Be(owner.FamilyId);
        updatedUser.Role.Should().Be(FamilyRole.Admin);
    }

    #endregion

    #region Validation - Invalid Token

    [Fact]
    public async Task AcceptInvitation_WithNonExistentToken_ReturnsValidationError()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, familyRepo, unitOfWork, "user");
        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        var invalidToken = InvitationToken.Generate();
        var mutation = $$"""
        mutation {
          acceptInvitation(input: { token: "{{invalidToken.Value}}" }) {
            family { id }
            role
            errors { message code }
          }
        }
        """;

        // Act
        var response = await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await ParseAcceptInvitationResponse(response);
        var errors = result.GetProperty("errors");
        errors.ValueKind.Should().Be(JsonValueKind.Array);
        errors.GetArrayLength().Should().BeGreaterThan(0);

        var firstError = errors[0];
        firstError.GetProperty("message").GetString()
            .Should().Be("Invalid or expired invitation token.");
    }

    #endregion

    #region Validation - Expired Invitation

    [Fact]
    public async Task AcceptInvitation_WithExpiredToken_ReturnsValidationError()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
        var invitationRepo = scope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyRepo, unitOfWork, "owner");

        var testId = TestDataFactory.GenerateTestId();
        var inviteeEmail = Email.From($"invitee-{testId}@example.com");
        var invitation = FamilyDomain.FamilyMemberInvitation.CreateEmailInvitation(
            owner.FamilyId!,
            inviteeEmail,
            FamilyRole.Member,
            owner.Id);

        // Use reflection to set expiration date in the past
        var expiresAtProperty = typeof(FamilyDomain.FamilyMemberInvitation).GetProperty("ExpiresAt")!;
        expiresAtProperty.SetValue(invitation, DateTime.UtcNow.AddDays(-1));

        await invitationRepo.AddAsync(invitation, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var tempFamily = FamilyDomain.Family.Create(FamilyName.From("Temp Family"), UserId.New());
        await familyRepo.AddAsync(tempFamily, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var invitee = User.CreateFromOAuth(inviteeEmail, $"ext-invitee-{testId}", "zitadel", tempFamily.Id);
        await userRepo.AddAsync(invitee, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var client = CreateAuthenticatedClient(inviteeEmail.Value, invitee.Id);

        var mutation = $$"""
        mutation {
          acceptInvitation(input: { token: "{{invitation.Token.Value}}" }) {
            family { id }
            role
            errors { message code }
          }
        }
        """;

        // Act
        var response = await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await ParseAcceptInvitationResponse(response);
        var errors = result.GetProperty("errors");
        errors.ValueKind.Should().Be(JsonValueKind.Array);

        var firstError = errors[0];
        firstError.GetProperty("message").GetString()
            .Should().Be("Invitation has expired and cannot be accepted.");
    }

    #endregion

    #region Validation - Email Mismatch

    [Fact]
    public async Task AcceptInvitation_WithEmailMismatch_ReturnsValidationError()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
        var invitationRepo = scope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyRepo, unitOfWork, "owner");

        // Create invitation for different email
        var invitationEmail = Email.From("invited@example.com");
        var invitation = FamilyDomain.FamilyMemberInvitation.CreateEmailInvitation(
            owner.FamilyId!,
            invitationEmail,
            FamilyRole.Member,
            owner.Id);

        await invitationRepo.AddAsync(invitation, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        // Authenticate as user with different email
        var differentEmail = Email.From("different@example.com");
        var tempFamily = FamilyDomain.Family.Create(FamilyName.From("Wrong User Family"), UserId.New());
        await familyRepo.AddAsync(tempFamily, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var wrongUser = User.CreateFromOAuth(differentEmail, "ext-wrong", "zitadel", tempFamily.Id);
        await userRepo.AddAsync(wrongUser, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var client = CreateAuthenticatedClient(differentEmail.Value, wrongUser.Id);

        var mutation = $$"""
        mutation {
          acceptInvitation(input: { token: "{{invitation.Token.Value}}" }) {
            family { id }
            role
            errors { message code }
          }
        }
        """;

        // Act
        var response = await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await ParseAcceptInvitationResponse(response);
        var errors = result.GetProperty("errors");
        errors.ValueKind.Should().Be(JsonValueKind.Array);

        var firstError = errors[0];
        firstError.GetProperty("message").GetString()
            .Should().Be("Invitation email does not match authenticated user.");
    }

    #endregion

    #region Validation - Wrong Status

    [Fact]
    public async Task AcceptInvitation_WithAcceptedInvitation_ReturnsValidationError()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
        var invitationRepo = scope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyRepo, unitOfWork, "owner");

        var testId = TestDataFactory.GenerateTestId();
        var inviteeEmail = Email.From($"invitee-{testId}@example.com");
        var invitation = FamilyDomain.FamilyMemberInvitation.CreateEmailInvitation(
            owner.FamilyId!,
            inviteeEmail,
            FamilyRole.Member,
            owner.Id);

        // Accept invitation first
        var invitee = User.CreateFromOAuth(inviteeEmail, $"ext-invitee-{testId}", "zitadel", owner.FamilyId!);
        await userRepo.AddAsync(invitee, CancellationToken.None);
        invitation.Accept(invitee.Id);

        await invitationRepo.AddAsync(invitation, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var client = CreateAuthenticatedClient(inviteeEmail.Value, invitee.Id);

        var mutation = $$"""
        mutation {
          acceptInvitation(input: { token: "{{invitation.Token.Value}}" }) {
            family { id }
            role
            errors { message code }
          }
        }
        """;

        // Act
        var response = await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await ParseAcceptInvitationResponse(response);
        var errors = result.GetProperty("errors");
        errors.ValueKind.Should().Be(JsonValueKind.Array);

        var firstError = errors[0];
        firstError.GetProperty("message").GetString()
            .Should().Contain("Cannot accept invitation in accepted status");
    }

    [Fact]
    public async Task AcceptInvitation_WithCanceledInvitation_ReturnsValidationError()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
        var invitationRepo = scope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyRepo, unitOfWork, "owner");

        var testId = TestDataFactory.GenerateTestId();
        var inviteeEmail = Email.From($"invitee-{testId}@example.com");
        var invitation = FamilyDomain.FamilyMemberInvitation.CreateEmailInvitation(
            owner.FamilyId!,
            inviteeEmail,
            FamilyRole.Member,
            owner.Id);

        // Cancel invitation
        invitation.Cancel(owner.Id);

        await invitationRepo.AddAsync(invitation, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var tempFamily = FamilyDomain.Family.Create(FamilyName.From("Temp Family"), UserId.New());
        await familyRepo.AddAsync(tempFamily, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var invitee = User.CreateFromOAuth(inviteeEmail, $"ext-invitee-{testId}", "zitadel", tempFamily.Id);
        await userRepo.AddAsync(invitee, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var client = CreateAuthenticatedClient(inviteeEmail.Value, invitee.Id);

        var mutation = $$"""
        mutation {
          acceptInvitation(input: { token: "{{invitation.Token.Value}}" }) {
            family { id }
            role
            errors { message code }
          }
        }
        """;

        // Act
        var response = await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await ParseAcceptInvitationResponse(response);
        var errors = result.GetProperty("errors");
        errors.ValueKind.Should().Be(JsonValueKind.Array);

        var firstError = errors[0];
        firstError.GetProperty("message").GetString()
            .Should().Contain("Cannot accept invitation in canceled status");
    }

    #endregion

    // NOTE: "Non-Existent Family" scenario is skipped for integration tests
    // because it would require bypassing database foreign key constraints.
    // This scenario is already covered by unit tests (AcceptInvitationCommandValidatorTests).

    #region Helper Methods

    private HttpClient CreateAuthenticatedClient(string email, UserId userId)
    {
        _factory.SetAuthenticatedUser(Email.From(email), userId);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "mock_token");
        return client;
    }

    private static async Task<JsonElement> ParseAcceptInvitationResponse(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseContent);

        if (jsonDocument.RootElement.TryGetProperty("data", out var data) &&
            data.ValueKind != JsonValueKind.Null)
        {
            return data.GetProperty("acceptInvitation");
        }

        return jsonDocument.RootElement;
    }

    #endregion
}
