using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FamilyHub.Modules.Family.Domain.Abstractions;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Integration.Helpers;
using FamilyHub.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using FamilyMemberInvitationAggregate = FamilyHub.Modules.Family.Domain.Aggregates.FamilyMemberInvitation;

namespace FamilyHub.Tests.Integration.Auth.GraphQL;

/// <summary>
/// Integration tests for invitations.pending GraphQL query.
/// Tests cross-family isolation, automatic family filtering, and security.
/// </summary>
[Collection("Database")]
public sealed class InvitationsPendingQueryTests(PostgreSqlContainerFixture containerFixture) : IDisposable
{
    private readonly GraphQlTestFactory _factory = new(containerFixture);

    #region Helper Methods

    /// <summary>
    /// Parses GraphQL response and returns the invitations.pending result.
    /// </summary>
    private static async Task<JsonElement> ParsePendingInvitationsResponse(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseContent);

        // Check for GraphQL errors first
        if (jsonDocument.RootElement.TryGetProperty("errors", out _))
        {
            throw new InvalidOperationException(
                $"GraphQL query returned errors: {responseContent}");
        }

        if (jsonDocument.RootElement.TryGetProperty("data", out var data) &&
            data.ValueKind != JsonValueKind.Null)
        {
            return data.GetProperty("invitations").GetProperty("pending");
        }

        throw new InvalidOperationException(
            $"Unexpected GraphQL response format: {responseContent}");
    }

    /// <summary>
    /// Creates an authenticated HTTP client with mocked ICurrentUserService.
    /// </summary>
    private HttpClient CreateAuthenticatedClient(string email, UserId userId, string? externalUserId = null)
    {
        _factory.SetAuthenticatedUser(Email.From(email), userId, externalUserId);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "mock_token");
        return client;
    }

    #endregion

    [Fact]
    public async Task Pending_ShouldReturnOnlyUserFamilyInvitations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
        var familyRepo = scope.ServiceProvider.GetRequiredService<IFamilyRepository>();
        var invitationRepo = scope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();
        var familyUnitOfWork = scope.ServiceProvider.GetRequiredService<IFamilyUnitOfWork>();

        // Create two separate families with owners
        var userA = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "owner-a");
        var userB = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "owner-b");

        // Reload families to get updated state
        var familyA = await familyRepo.GetByIdAsync(userA.FamilyId);
        var familyB = await familyRepo.GetByIdAsync(userB.FamilyId);

        familyA.Should().NotBeNull();
        familyB.Should().NotBeNull();

        // Create invitations for both families
        var invitationA = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyA.Id,
            Email.From("member-a@test.com"),
            FamilyRole.Member,
            userA.Id);

        var invitationB = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyB.Id,
            Email.From("member-b@test.com"),
            FamilyRole.Member,
            userB.Id);

        await invitationRepo.AddAsync(invitationA);
        await invitationRepo.AddAsync(invitationB);
        await familyUnitOfWork.SaveChangesAsync();

        // Setup authentication for User A
        var client = CreateAuthenticatedClient(userA.Email.Value, userA.Id);

        var query = """
        query {
          invitations {
            pending {
              id
              email
              role
              status
            }
          }
        }
        """;

        var request = new { query };

        // Act - User A queries pending invitations
        var response = await client.PostAsJsonAsync("/graphql", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pending = await ParsePendingInvitationsResponse(response);
        var invitations = pending.EnumerateArray().ToList();

        // User A should ONLY see Family A's invitation
        invitations.Should().HaveCount(1);
        invitations[0].GetProperty("email").GetString().Should().Be("member-a@test.com");

        // Verify the ID matches invitation A
        var returnedId = invitations[0].GetProperty("id").GetGuid();
        returnedId.Should().Be(invitationA.Id.Value);
    }

    [Fact]
    public async Task Pending_ShouldNotAllowCrossFamilyAccess()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
        var familyRepo = scope.ServiceProvider.GetRequiredService<IFamilyRepository>();
        var invitationRepo = scope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();
        var familyUnitOfWork = scope.ServiceProvider.GetRequiredService<IFamilyUnitOfWork>();

        // Create two families
        var userA = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "isolated-a");
        var userB = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "isolated-b");

        var familyB = await familyRepo.GetByIdAsync(userB.FamilyId);
        familyB.Should().NotBeNull();

        // Create invitation ONLY for Family B
        var invitationB = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyB.Id,
            Email.From("secret@test.com"),
            FamilyRole.Admin,
            userB.Id);

        await invitationRepo.AddAsync(invitationB);
        await familyUnitOfWork.SaveChangesAsync();

        // Setup authentication for User A (who belongs to Family A)
        var client = CreateAuthenticatedClient(userA.Email.Value, userA.Id);

        var query = """
        query {
          invitations {
            pending {
              id
              email
              role
            }
          }
        }
        """;

        var request = new { query };

        // Act - User A tries to query invitations (should NOT see Family B's data)
        var response = await client.PostAsJsonAsync("/graphql", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pending = await ParsePendingInvitationsResponse(response);
        var invitations = pending.EnumerateArray().ToList();

        // User A should see EMPTY list (Family A has no invitations)
        invitations.Should().BeEmpty();

        // Verify Family B's invitation still exists in database (not accessible via query)
        var dbInvitation = await invitationRepo.GetByIdAsync(invitationB.Id);
        dbInvitation.Should().NotBeNull();
        dbInvitation.Email.Should().Be(Email.From("secret@test.com"));
    }

    [Fact]
    public async Task Pending_WithMultipleInvitations_ShouldReturnAllForUserFamily()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
        var familyRepo = scope.ServiceProvider.GetRequiredService<IFamilyRepository>();
        var invitationRepo = scope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();
        var familyUnitOfWork = scope.ServiceProvider.GetRequiredService<IFamilyUnitOfWork>();

        var user = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "multi");
        var family = await familyRepo.GetByIdAsync(user.FamilyId);
        family.Should().NotBeNull();

        // Create multiple invitations for the same family
        var invitation1 = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            family.Id,
            Email.From("member1@test.com"),
            FamilyRole.Member,
            user.Id);

        var invitation2 = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            family.Id,
            Email.From("member2@test.com"),
            FamilyRole.Admin,
            user.Id);

        var invitation3 = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            family.Id,
            Email.From("member3@test.com"),
            FamilyRole.Member,
            user.Id);

        await invitationRepo.AddAsync(invitation1);
        await invitationRepo.AddAsync(invitation2);
        await invitationRepo.AddAsync(invitation3);
        await familyUnitOfWork.SaveChangesAsync();

        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        var query = """
        query {
          invitations {
            pending {
              id
              email
              role
            }
          }
        }
        """;

        var request = new { query };

        // Act
        var response = await client.PostAsJsonAsync("/graphql", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pending = await ParsePendingInvitationsResponse(response);
        var invitations = pending.EnumerateArray().ToList();

        // Should return all 3 invitations
        invitations.Should().HaveCount(3);

        var emails = invitations.Select(i => i.GetProperty("email").GetString()).ToList();
        emails.Should().Contain("member1@test.com");
        emails.Should().Contain("member2@test.com");
        emails.Should().Contain("member3@test.com");
    }

    [Fact]
    public async Task Pending_WithNoInvitations_ShouldReturnEmptyArray()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var user = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "empty");
        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        var query = """
        query {
          invitations {
            pending {
              id
              email
            }
          }
        }
        """;

        var request = new { query };

        // Act
        var response = await client.PostAsJsonAsync("/graphql", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pending = await ParsePendingInvitationsResponse(response);
        var invitations = pending.EnumerateArray().ToList();

        // Should return empty array (not null)
        invitations.Should().BeEmpty();
    }

    [Fact]
    public async Task Pending_WithoutAuthentication_ShouldReturnUnauthorizedError()
    {
        // Arrange
        _factory.ClearAuthenticatedUser();
        var client = _factory.CreateClient();

        var query = """
        query {
          invitations {
            pending {
              id
              email
            }
          }
        }
        """;

        var request = new { query };

        // Act
        var response = await client.PostAsJsonAsync("/graphql", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseContent);

        // GraphQL returns errors for unauthenticated requests
        jsonDocument.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);

        var firstError = errors[0];
        var message = firstError.GetProperty("message").GetString();
        message.Should().Contain("not authenticated");
    }

    [Fact]
    public async Task Pending_ShouldReturnAllInvitationFields()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
        var familyRepo = scope.ServiceProvider.GetRequiredService<IFamilyRepository>();
        var invitationRepo = scope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();
        var familyUnitOfWork = scope.ServiceProvider.GetRequiredService<IFamilyUnitOfWork>();

        var user = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "fields");
        var family = await familyRepo.GetByIdAsync(user.FamilyId);
        family.Should().NotBeNull();

        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            family.Id,
            Email.From("test@example.com"),
            FamilyRole.Admin,
            user.Id,
            "Welcome to our family!");

        await invitationRepo.AddAsync(invitation);
        await familyUnitOfWork.SaveChangesAsync();

        var client = CreateAuthenticatedClient(user.Email.Value, user.Id);

        var query = """
        query {
          invitations {
            pending {
              id
              email
              role
              status
              invitedAt
              expiresAt
              displayCode
              message
            }
          }
        }
        """;

        var request = new { query };

        // Act
        var response = await client.PostAsJsonAsync("/graphql", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pending = await ParsePendingInvitationsResponse(response);
        var invitations = pending.EnumerateArray().ToList();

        invitations.Should().HaveCount(1);

        var inv = invitations[0];
        inv.GetProperty("id").GetGuid().Should().Be(invitation.Id.Value);
        inv.GetProperty("email").GetString().Should().Be("test@example.com");
        inv.GetProperty("role").GetString().Should().Be("ADMIN");
        inv.GetProperty("status").GetString().Should().Be("PENDING");
        inv.GetProperty("message").GetString().Should().Be("Welcome to our family!");

        // Verify dates are returned
        inv.TryGetProperty("invitedAt", out _).Should().BeTrue();
        inv.TryGetProperty("expiresAt", out _).Should().BeTrue();
        inv.TryGetProperty("displayCode", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Pending_AcrossDifferentUsers_ShouldMaintainIsolation()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
        var familyRepo = scope.ServiceProvider.GetRequiredService<IFamilyRepository>();
        var invitationRepo = scope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();
        var familyUnitOfWork = scope.ServiceProvider.GetRequiredService<IFamilyUnitOfWork>();

        // Create 3 separate families
        var user1 = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "user1");
        var user2 = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "user2");
        var user3 = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "user3");

        var family1 = await familyRepo.GetByIdAsync(user1.FamilyId);
        var family2 = await familyRepo.GetByIdAsync(user2.FamilyId);
        var family3 = await familyRepo.GetByIdAsync(user3.FamilyId);

        // Create invitations for each family
        var inv1 = FamilyMemberInvitationAggregate.CreateEmailInvitation(family1!.Id, Email.From("f1@test.com"), FamilyRole.Member, user1.Id);
        var inv2A = FamilyMemberInvitationAggregate.CreateEmailInvitation(family2!.Id, Email.From("f2a@test.com"), FamilyRole.Member, user2.Id);
        var inv2B = FamilyMemberInvitationAggregate.CreateEmailInvitation(family2.Id, Email.From("f2b@test.com"), FamilyRole.Admin, user2.Id);
        var inv3 = FamilyMemberInvitationAggregate.CreateEmailInvitation(family3!.Id, Email.From("f3@test.com"), FamilyRole.Member, user3.Id);

        await invitationRepo.AddAsync(inv1);
        await invitationRepo.AddAsync(inv2A);
        await invitationRepo.AddAsync(inv2B);
        await invitationRepo.AddAsync(inv3);
        await familyUnitOfWork.SaveChangesAsync();

        var query = """
        query {
          invitations {
            pending {
              email
            }
          }
        }
        """;

        // Act & Assert - User 1 sees only Family 1's invitation
        var client1 = CreateAuthenticatedClient(user1.Email.Value, user1.Id);
        var response1 = await client1.PostAsJsonAsync("/graphql", new { query });
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        var pending1 = await ParsePendingInvitationsResponse(response1);
        var emails1 = pending1.EnumerateArray().Select(i => i.GetProperty("email").GetString()).ToList();
        emails1.Should().ContainSingle().Which.Should().Be("f1@test.com");

        // Act & Assert - User 2 sees only Family 2's invitations (2 of them)
        var client2 = CreateAuthenticatedClient(user2.Email.Value, user2.Id);
        var response2 = await client2.PostAsJsonAsync("/graphql", new { query });
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        var pending2 = await ParsePendingInvitationsResponse(response2);
        var emails2 = pending2.EnumerateArray().Select(i => i.GetProperty("email").GetString()).ToList();
        emails2.Should().HaveCount(2);
        emails2.Should().Contain("f2a@test.com");
        emails2.Should().Contain("f2b@test.com");

        // Act & Assert - User 3 sees only Family 3's invitation
        var client3 = CreateAuthenticatedClient(user3.Email.Value, user3.Id);
        var response3 = await client3.PostAsJsonAsync("/graphql", new { query });
        response3.StatusCode.Should().Be(HttpStatusCode.OK);
        var pending3 = await ParsePendingInvitationsResponse(response3);
        var emails3 = pending3.EnumerateArray().Select(i => i.GetProperty("email").GetString()).ToList();
        emails3.Should().ContainSingle().Which.Should().Be("f3@test.com");
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
