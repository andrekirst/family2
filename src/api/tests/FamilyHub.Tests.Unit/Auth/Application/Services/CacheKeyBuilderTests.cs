using FamilyHub.Modules.Auth.Application.Services;
using FluentAssertions;

namespace FamilyHub.Tests.Unit.Auth.Application.Services;

public sealed class CacheKeyBuilderTests
{
    [Fact]
    public void FamilyMemberInvitation_WithValidToken_ReturnsCorrectKey()
    {
        // Arrange
        const string token = "abc123xyz";

        // Act
        var key = CacheKeyBuilder.FamilyMemberInvitation(token);

        // Assert
        key.Should().Be("FamilyMemberInvitation:abc123xyz");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void FamilyMemberInvitation_WithInvalidToken_ThrowsArgumentException(string? invalidToken)
    {
        // Act
        Action act = () => CacheKeyBuilder.FamilyMemberInvitation(invalidToken!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("token");
    }

    [Fact]
    public void Family_WithValidGuid_ReturnsCorrectKey()
    {
        // Arrange
        var familyId = Guid.Parse("12345678-1234-1234-1234-123456789abc");

        // Act
        var key = CacheKeyBuilder.Family(familyId);

        // Assert
        key.Should().Be("Family:12345678-1234-1234-1234-123456789abc");
    }

    [Fact]
    public void Family_WithEmptyGuid_ThrowsArgumentException()
    {
        // Act
        Action act = () => CacheKeyBuilder.Family(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("familyId");
    }

    [Fact]
    public void FamilyMemberInvitation_MatchesInterfaceDocumentedPattern()
    {
        // This test verifies that the builder produces keys matching
        // the pattern documented in IValidationCache interface

        // Arrange
        const string token = "test-token-123";

        // Act
        var key = CacheKeyBuilder.FamilyMemberInvitation(token);

        // Assert
        key.Should().StartWith("FamilyMemberInvitation:");
        key.Should().EndWith(token);
        key.Should().MatchRegex(@"^[A-Za-z]+:[A-Za-z0-9\-]+$");
    }
}
