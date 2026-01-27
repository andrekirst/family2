using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FluentAssertions;

namespace FamilyHub.Tests.Unit.Auth.Infrastructure.Configuration;

/// <summary>
/// Unit tests for JwtSettings.
/// Tests validation logic and default values.
/// </summary>
public sealed class JwtSettingsTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultSecretKey_IsEmpty()
    {
        // Arrange & Act
        var settings = new JwtSettings();

        // Assert
        settings.SecretKey.Should().BeEmpty();
    }

    [Fact]
    public void DefaultIssuer_IsFamilyHubApp()
    {
        // Arrange & Act
        var settings = new JwtSettings();

        // Assert
        settings.Issuer.Should().Be("https://familyhub.app");
    }

    [Fact]
    public void DefaultAudience_IsFamilyHubApp()
    {
        // Arrange & Act
        var settings = new JwtSettings();

        // Assert
        settings.Audience.Should().Be("https://familyhub.app");
    }

    [Fact]
    public void DefaultAccessTokenExpirationMinutes_Is15()
    {
        // Arrange & Act
        var settings = new JwtSettings();

        // Assert
        settings.AccessTokenExpirationMinutes.Should().Be(15);
    }

    [Fact]
    public void DefaultRefreshTokenExpirationDays_Is7()
    {
        // Arrange & Act
        var settings = new JwtSettings();

        // Assert
        settings.RefreshTokenExpirationDays.Should().Be(7);
    }

    [Fact]
    public void SectionName_IsAuthenticationJwtSettings()
    {
        // Assert
        JwtSettings.SectionName.Should().Be("Authentication:JwtSettings");
    }

    #endregion

    #region IsValid Tests - Valid Settings

    [Fact]
    public void IsValid_WithValidSettings_ReturnsTrue()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "this-is-a-valid-secret-key-32-chars!",
            Issuer = "https://familyhub.app",
            Audience = "https://familyhub.app"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithMinimum32CharSecretKey_ReturnsTrue()
    {
        // Arrange - Exactly 32 characters
        var settings = new JwtSettings
        {
            SecretKey = "12345678901234567890123456789012",
            Issuer = "https://example.com",
            Audience = "https://example.com"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithLongSecretKey_ReturnsTrue()
    {
        // Arrange - Very long secret key
        var settings = new JwtSettings
        {
            SecretKey = new string('x', 256),
            Issuer = "https://example.com",
            Audience = "https://example.com"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithCustomIssuerAndAudience_ReturnsTrue()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "this-is-a-valid-secret-key-32-chars!",
            Issuer = "https://custom-issuer.example.com",
            Audience = "https://custom-audience.example.com"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    #endregion

    #region IsValid Tests - Invalid SecretKey

    [Fact]
    public void IsValid_WithDefaultSettings_ReturnsFalse()
    {
        // Arrange - Default SecretKey is empty
        var settings = new JwtSettings();

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithEmptySecretKey_ReturnsFalse()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = string.Empty,
            Issuer = "https://example.com",
            Audience = "https://example.com"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithWhitespaceSecretKey_ReturnsFalse()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "   ",
            Issuer = "https://example.com",
            Audience = "https://example.com"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithTooShortSecretKey_ReturnsFalse()
    {
        // Arrange - Only 31 characters
        var settings = new JwtSettings
        {
            SecretKey = "1234567890123456789012345678901",
            Issuer = "https://example.com",
            Audience = "https://example.com"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(31)]
    public void IsValid_WithSecretKeyUnder32Chars_ReturnsFalse(int length)
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = new string('x', length),
            Issuer = "https://example.com",
            Audience = "https://example.com"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion

    #region IsValid Tests - Invalid Issuer

    [Fact]
    public void IsValid_WithEmptyIssuer_ReturnsFalse()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "this-is-a-valid-secret-key-32-chars!",
            Issuer = string.Empty,
            Audience = "https://example.com"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithWhitespaceIssuer_ReturnsFalse()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "this-is-a-valid-secret-key-32-chars!",
            Issuer = "   ",
            Audience = "https://example.com"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNullIssuer_ReturnsFalse()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "this-is-a-valid-secret-key-32-chars!",
            Issuer = null!,
            Audience = "https://example.com"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion

    #region IsValid Tests - Invalid Audience

    [Fact]
    public void IsValid_WithEmptyAudience_ReturnsFalse()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "this-is-a-valid-secret-key-32-chars!",
            Issuer = "https://example.com",
            Audience = string.Empty
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithWhitespaceAudience_ReturnsFalse()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "this-is-a-valid-secret-key-32-chars!",
            Issuer = "https://example.com",
            Audience = "   "
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNullAudience_ReturnsFalse()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "this-is-a-valid-secret-key-32-chars!",
            Issuer = "https://example.com",
            Audience = null!
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void IsValid_WithMultipleInvalidSettings_ReturnsFalse()
    {
        // Arrange - All invalid
        var settings = new JwtSettings
        {
            SecretKey = "short",
            Issuer = "",
            Audience = ""
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_DoesNotValidateTokenExpirations()
    {
        // Arrange - Zero or negative expirations are not validated by IsValid()
        var settings = new JwtSettings
        {
            SecretKey = "this-is-a-valid-secret-key-32-chars!",
            Issuer = "https://example.com",
            Audience = "https://example.com",
            AccessTokenExpirationMinutes = 0,
            RefreshTokenExpirationDays = -1
        };

        // Act
        var isValid = settings.IsValid();

        // Assert - IsValid only checks SecretKey, Issuer, Audience
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("non-url-issuer")]
    [InlineData("localhost")]
    [InlineData("example.com")]
    public void IsValid_WithNonUrlIssuer_ReturnsTrue(string issuer)
    {
        // Arrange - Issuer doesn't need to be a URL
        var settings = new JwtSettings
        {
            SecretKey = "this-is-a-valid-secret-key-32-chars!",
            Issuer = issuer,
            Audience = "https://example.com"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    #endregion
}
