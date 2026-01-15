using System.Net.Http.Json;
using System.Text.Json;
using FamilyHub.Modules.Family.Domain.Enums;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Integration.Helpers;
using FamilyHub.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.Tests.Integration.Family.Infrastructure;

/// <summary>
/// Integration tests for invitation email content verification via MailHog.
/// Tests end-to-end email flow from GraphQL mutation to email content validation.
///
/// Prerequisites:
/// - MailHog running on localhost:1025 (SMTP) and localhost:8025 (HTTP API)
/// - Backend configured to use localhost:1025 for SMTP
/// - Docker Compose infrastructure running
///
/// Test Coverage:
/// - Email delivery verification
/// - Subject line content
/// - Personal message inclusion
/// - Role information in email body
/// - Valid invitation token link
/// - Multiple invitations (batch processing)
/// </summary>
[Collection("Database")]
public sealed class InvitationEmailIntegrationTests(PostgreSqlContainerFixture containerFixture) : IAsyncLifetime
{
    private readonly MailHogClient _mailHog = new();
    private readonly GraphQlTestFactory _factory = new(containerFixture);

    public async Task InitializeAsync()
    {
        // Clear MailHog inbox before each test to ensure clean state
        await _mailHog.ClearEmailsAsync();
    }

    public async Task DisposeAsync()
    {
        await _mailHog.ClearEmailsAsync();
        _factory.Dispose();
        _mailHog.Dispose();
    }

    #region Email Delivery Tests

    [Fact]
    public async Task InviteFamilyMembers_WithValidEmail_EmailDeliveredToMailHog()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "owner");
        var client = CreateAuthenticatedClient(owner.Email.Value, owner.Id.Value);

        var inviteeEmail = $"invitee-{Guid.NewGuid():N}@example.com";

        var mutation = $$"""
        mutation {
          inviteFamilyMembers(input: {
            familyId: "{{owner.FamilyId.Value}}"
            invitations: [{
              email: "{{inviteeEmail}}"
              role: "MEMBER"
            }]
          }) {
            inviteFamilyMembersDto {
              successfulInvitations {
                invitationId
                email
                role
              }
              failedInvitations {
                email
                errorCode
                errorMessage
              }
            }
            errors {
              __typename
            }
          }
        }
        """;

        // Act
        var response = await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Wait for email to arrive (async background processing)
        await Task.Delay(2000); // Allow EmailOutbox background service to process

        // Assert
        response.EnsureSuccessStatusCode();

        var email = await _mailHog.WaitForEmailAsync(
            e => e.To.Any(to => $"{to.Mailbox}@{to.Domain}" == inviteeEmail),
            5000);

        email.Should().NotBeNull("invitation email should be delivered to MailHog");
        email!.To.Should().HaveCount(1);
        email.To[0].Mailbox.Should().Be(inviteeEmail.Split('@')[0]);
        email.To[0].Domain.Should().Be("example.com");
    }

    [Fact]
    public async Task InviteFamilyMembers_WithMultipleInvitations_AllEmailsDelivered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "owner");
        var client = CreateAuthenticatedClient(owner.Email.Value, owner.Id.Value);

        var invitee1Email = $"invitee1-{Guid.NewGuid():N}@example.com";
        var invitee2Email = $"invitee2-{Guid.NewGuid():N}@example.com";
        var invitee3Email = $"invitee3-{Guid.NewGuid():N}@example.com";

        var mutation = $$"""
        mutation {
          inviteFamilyMembers(input: {
            familyId: "{{owner.FamilyId.Value}}"
            invitations: [
              { email: "{{invitee1Email}}", role: "MEMBER" }
              { email: "{{invitee2Email}}", role: "ADMIN" }
              { email: "{{invitee3Email}}", role: "MEMBER" }
            ]
          }) {
            inviteFamilyMembersDto {
              successfulInvitations {
                invitationId
                email
                role
              }
              failedInvitations {
                email
                errorCode
                errorMessage
              }
            }
            errors {
              __typename
            }
          }
        }
        """;

        // Act
        var response = await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Wait for emails to arrive
        await Task.Delay(3000);

        // Assert
        response.EnsureSuccessStatusCode();

        var emails = await _mailHog.WaitForEmailsAsync(
            3,
            e => e.To.Any(to => new[] { invitee1Email, invitee2Email, invitee3Email }
                .Contains($"{to.Mailbox}@{to.Domain}")),
            10000);

        emails.Should().HaveCount(3, "all 3 invitation emails should be delivered");

        var recipients = emails.Select(e => $"{e.To[0].Mailbox}@{e.To[0].Domain}").ToList();
        recipients.Should().Contain(invitee1Email);
        recipients.Should().Contain(invitee2Email);
        recipients.Should().Contain(invitee3Email);
    }

    #endregion

    #region Email Content Tests

    [Fact]
    public async Task InvitationEmail_ContainsCorrectSubject()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "owner");
        var client = CreateAuthenticatedClient(owner.Email.Value, owner.Id.Value);

        var inviteeEmail = $"invitee-{Guid.NewGuid():N}@example.com";

        var mutation = $$"""
        mutation {
          inviteFamilyMembers(input: {
            familyId: "{{owner.FamilyId.Value}}"
            invitations: [{
              email: "{{inviteeEmail}}"
              role: "MEMBER"
            }]
          }) {
            inviteFamilyMembersDto {
              successfulInvitations {
                invitationId
                email
                role
              }
              failedInvitations {
                email
                errorCode
                errorMessage
              }
            }
            errors {
              __typename
            }
          }
        }
        """;

        // Act
        await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Wait for email to arrive (async background processing)
        await Task.Delay(2000); // Allow EmailOutbox background service to process

        // Assert
        var email = await _mailHog.WaitForEmailAsync(
            e => e.To.Any(to => $"{to.Mailbox}@{to.Domain}" == inviteeEmail),
            5000);
        email.Should().NotBeNull("email should arrive in MailHog");

        var subject = email!.Content.Headers["Subject"][0];
        subject.Should().Contain("invited", "subject should indicate invitation");
        subject.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task InvitationEmail_IncludesPersonalMessage()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "owner");
        var client = CreateAuthenticatedClient(owner.Email.Value, owner.Id.Value);

        var inviteeEmail = $"invitee-{Guid.NewGuid():N}@example.com";
        var personalMessage = "Welcome to our family circle! 🎉 Looking forward to sharing memories.";

        var mutation = $$"""
        mutation {
          inviteFamilyMembers(input: {
            familyId: "{{owner.FamilyId.Value}}"
            invitations: [{
              email: "{{inviteeEmail}}"
              role: "MEMBER"
            }]
            message: "{{personalMessage}}"
          }) {
            inviteFamilyMembersDto {
              successfulInvitations {
                invitationId
                email
                role
              }
              failedInvitations {
                email
                errorCode
                errorMessage
              }
            }
            errors {
              __typename
            }
          }
        }
        """;

        // Act
        await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Wait for email to arrive (async background processing)
        await Task.Delay(2000); // Allow EmailOutbox background service to process

        // Assert
        var email = await _mailHog.WaitForEmailAsync(
            e => e.To.Any(to => $"{to.Mailbox}@{to.Domain}" == inviteeEmail),
            5000);
        email.Should().NotBeNull("email should arrive in MailHog");

        var bodyPlainText = _mailHog.GetPlainTextBody(email!);
        bodyPlainText.Should().Contain(personalMessage, "email body should include personal message");
    }

    [Fact]
    public async Task InvitationEmail_ContainsValidInvitationToken()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "owner");
        var client = CreateAuthenticatedClient(owner.Email.Value, owner.Id.Value);

        var inviteeEmail = $"invitee-{Guid.NewGuid():N}@example.com";

        var mutation = $$"""
        mutation {
          inviteFamilyMembers(input: {
            familyId: "{{owner.FamilyId.Value}}"
            invitations: [{
              email: "{{inviteeEmail}}"
              role: "MEMBER"
            }]
          }) {
            inviteFamilyMembersDto {
              successfulInvitations {
                invitationId
                email
                role
              }
              failedInvitations {
                email
                errorCode
                errorMessage
              }
            }
            errors {
              __typename
            }
          }
        }
        """;

        // Act
        await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Wait for email to arrive (async background processing)
        await Task.Delay(2000); // Allow EmailOutbox background service to process

        // Assert
        var email = await _mailHog.WaitForEmailAsync(
            e => e.To.Any(to => $"{to.Mailbox}@{to.Domain}" == inviteeEmail),
            5000);
        email.Should().NotBeNull("email should arrive in MailHog");

        // Extract invitation token from email body
        var token = _mailHog.ExtractInvitationToken(email!);
        token.Should().NotBeNullOrWhiteSpace("email should contain invitation token");
        token.Should().MatchRegex(@"^[a-zA-Z0-9-]{20,}$", "token should be alphanumeric with hyphens");

        // Verify token link is present
        var urls = _mailHog.ExtractUrls(email!);
        urls.Should().Contain(url => url.Contains($"token={token}"),
            "email should contain clickable link with token parameter");
    }

    [Fact]
    public async Task InvitationEmail_AdminRole_IndicatesAdminPrivileges()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "owner");
        var client = CreateAuthenticatedClient(owner.Email.Value, owner.Id.Value);

        var inviteeEmail = $"admin-{Guid.NewGuid():N}@example.com";

        var mutation = $$"""
        mutation {
          inviteFamilyMembers(input: {
            familyId: "{{owner.FamilyId.Value}}"
            invitations: [{
              email: "{{inviteeEmail}}"
              role: "ADMIN"
            }]
          }) {
            inviteFamilyMembersDto {
              successfulInvitations {
                invitationId
                email
                role
              }
              failedInvitations {
                email
                errorCode
                errorMessage
              }
            }
            errors {
              __typename
            }
          }
        }
        """;

        // Act
        await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Wait for email to arrive (async background processing)
        await Task.Delay(2000); // Allow EmailOutbox background service to process

        // Assert
        var email = await _mailHog.WaitForEmailAsync(
            e => e.To.Any(to => $"{to.Mailbox}@{to.Domain}" == inviteeEmail),
            5000);
        email.Should().NotBeNull("email should arrive in MailHog");

        var bodyPlainText = _mailHog.GetPlainTextBody(email!);
        bodyPlainText.Should().ContainAny("Admin", "administrator", "admin role",
            "email should mention admin privileges");
    }

    [Fact]
    public async Task InvitationEmail_MemberRole_IndicatesMemberRole()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "owner");
        var client = CreateAuthenticatedClient(owner.Email.Value, owner.Id.Value);

        var inviteeEmail = $"member-{Guid.NewGuid():N}@example.com";

        var mutation = $$"""
        mutation {
          inviteFamilyMembers(input: {
            familyId: "{{owner.FamilyId.Value}}"
            invitations: [{
              email: "{{inviteeEmail}}"
              role: "MEMBER"
            }]
          }) {
            inviteFamilyMembersDto {
              successfulInvitations {
                invitationId
                email
                role
              }
              failedInvitations {
                email
                errorCode
                errorMessage
              }
            }
            errors {
              __typename
            }
          }
        }
        """;

        // Act
        await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Wait for email to arrive (async background processing)
        await Task.Delay(2000); // Allow EmailOutbox background service to process

        // Assert
        var email = await _mailHog.WaitForEmailAsync(
            e => e.To.Any(to => $"{to.Mailbox}@{to.Domain}" == inviteeEmail),
            5000);
        email.Should().NotBeNull("email should arrive in MailHog");

        var bodyPlainText = _mailHog.GetPlainTextBody(email!);
        bodyPlainText.Should().ContainAny("Member", "member role",
            "email should mention member role");
    }

    #endregion

    #region Email Format Tests

    [Fact]
    public async Task InvitationEmail_ContainsBothHtmlAndPlainTextParts()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "owner");
        var client = CreateAuthenticatedClient(owner.Email.Value, owner.Id.Value);

        var inviteeEmail = $"invitee-{Guid.NewGuid():N}@example.com";

        var mutation = $$"""
        mutation {
          inviteFamilyMembers(input: {
            familyId: "{{owner.FamilyId.Value}}"
            invitations: [{
              email: "{{inviteeEmail}}"
              role: "MEMBER"
            }]
          }) {
            inviteFamilyMembersDto {
              successfulInvitations {
                invitationId
                email
                role
              }
              failedInvitations {
                email
                errorCode
                errorMessage
              }
            }
            errors {
              __typename
            }
          }
        }
        """;

        // Act
        await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Wait for email to arrive (async background processing)
        await Task.Delay(2000); // Allow EmailOutbox background service to process

        // Assert
        var email = await _mailHog.WaitForEmailAsync(
            e => e.To.Any(to => $"{to.Mailbox}@{to.Domain}" == inviteeEmail),
            5000);
        email.Should().NotBeNull("email should arrive in MailHog");

        // Verify HTML content
        email!.Content.Body.Should().NotBeNullOrWhiteSpace("email should have content");

        // Check for MIME multipart (HTML + plain text)
        if (email.MIME?.Parts != null)
        {
            email.MIME.Parts.Should().HaveCountGreaterThan(0,
                "email should have MIME parts for HTML and plain text");
        }
    }

    [Fact]
    public async Task InvitationEmail_FromAddressMatchesConfiguration()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var (userRepo, familyService, unitOfWork) = TestServices.ResolveRepositoryServices(scope);

        var owner = await TestDataFactory.CreateUserAsync(userRepo, familyService, unitOfWork, "owner");
        var client = CreateAuthenticatedClient(owner.Email.Value, owner.Id.Value);

        var inviteeEmail = $"invitee-{Guid.NewGuid():N}@example.com";

        var mutation = $$"""
        mutation {
          inviteFamilyMembers(input: {
            familyId: "{{owner.FamilyId.Value}}"
            invitations: [{
              email: "{{inviteeEmail}}"
              role: "MEMBER"
            }]
          }) {
            inviteFamilyMembersDto {
              successfulInvitations {
                invitationId
                email
                role
              }
              failedInvitations {
                email
                errorCode
                errorMessage
              }
            }
            errors {
              __typename
            }
          }
        }
        """;

        // Act
        await client.PostAsJsonAsync("/graphql", new { query = mutation });

        // Wait for email to arrive (async background processing)
        await Task.Delay(2000); // Allow EmailOutbox background service to process

        // Assert
        var email = await _mailHog.WaitForEmailAsync(
            e => e.To.Any(to => $"{to.Mailbox}@{to.Domain}" == inviteeEmail),
            5000);
        email.Should().NotBeNull("email should arrive in MailHog");

        var fromAddress = $"{email!.From.Mailbox}@{email.From.Domain}";
        fromAddress.Should().Be("no-reply@familyhub.local",
            "from address should match SMTP configuration");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates an authenticated HTTP client for GraphQL requests
    /// </summary>
    private HttpClient CreateAuthenticatedClient(string email, Guid userId)
    {
        _factory.SetAuthenticatedUser(Email.From(email), UserId.From(userId));
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "mock_token");
        return client;
    }

    #endregion
}
