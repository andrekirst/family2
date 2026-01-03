using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Tests.Unit.Auth;

/// <summary>
/// Integration tests for OAuth authentication flow.
/// Tests the complete end-to-end OAuth user creation.
/// </summary>
public class OAuthIntegrationTests
{
    [Fact]
    public void User_CreateFromOAuth_CreatesValidUserWithZitadelProvider()
    {
        // Arrange
        var email = Email.From("admin@familyhub.localhost");
        var externalUserId = "352299881513222146";
        var externalProvider = "zitadel";

        // Act
        var user = User.CreateFromOAuth(email, externalUserId, externalProvider);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(email, user.Email);
        Assert.Equal(externalUserId, user.ExternalUserId);
        Assert.Equal(externalProvider, user.ExternalProvider);
        Assert.True(user.EmailVerified);
        Assert.NotNull(user.EmailVerifiedAt);
        Assert.Null(user.DeletedAt);
        Assert.NotEqual(Guid.Empty, user.Id.Value);
    }

    [Fact]
    public void User_CreateFromOAuth_EmailIsAutomaticallyVerified()
    {
        // Arrange
        var email = Email.From("test@example.com");
        var externalUserId = "test-user-id";
        var externalProvider = "zitadel";

        // Act
        var user = User.CreateFromOAuth(email, externalUserId, externalProvider);

        // Assert - Email should be verified when created from OAuth
        Assert.True(user.EmailVerified);
        Assert.NotNull(user.EmailVerifiedAt);
        var timeSinceCreation = DateTime.UtcNow - user.EmailVerifiedAt.Value;
        Assert.True(timeSinceCreation.TotalSeconds < 5); // Verified within last 5 seconds
    }

    [Theory]
    [InlineData("user1@example.com", "user-id-1")]
    [InlineData("user2@example.com", "user-id-2")]
    [InlineData("admin@familyhub.localhost", "352299881513222146")]
    public void User_CreateFromOAuth_WorksWithDifferentEmails(string emailAddress, string userId)
    {
        // Arrange
        var email = Email.From(emailAddress);
        var externalProvider = "zitadel";

        // Act
        var user = User.CreateFromOAuth(email, userId, externalProvider);

        // Assert
        Assert.Equal(emailAddress, user.Email.Value);
        Assert.Equal(userId, user.ExternalUserId);
    }

    [Fact]
    public void User_CreateFromOAuth_GeneratesUniqueUserIds()
    {
        // Arrange
        var email = Email.From("test@example.com");
        var externalUserId = "test-user-id";
        var externalProvider = "zitadel";

        // Act - Create multiple users
        var user1 = User.CreateFromOAuth(email, externalUserId, externalProvider);
        var user2 = User.CreateFromOAuth(email, externalUserId, externalProvider);
        var user3 = User.CreateFromOAuth(email, externalUserId, externalProvider);

        // Assert - All have different internal IDs
        Assert.NotEqual(user1.Id.Value, user2.Id.Value);
        Assert.NotEqual(user2.Id.Value, user3.Id.Value);
        Assert.NotEqual(user1.Id.Value, user3.Id.Value);
    }
}
