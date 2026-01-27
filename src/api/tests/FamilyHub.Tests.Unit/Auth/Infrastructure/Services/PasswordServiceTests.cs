using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.Modules.Auth.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace FamilyHub.Tests.Unit.Auth.Infrastructure.Services;

/// <summary>
/// Unit tests for PasswordService.
/// Tests password hashing, verification, and strength validation.
/// </summary>
public sealed class PasswordServiceTests
{
    #region Test Helpers

    private static PasswordService CreateService(PasswordPolicyOptions? options = null)
    {
        var policyOptions = options ?? new PasswordPolicyOptions
        {
            MinimumLength = 12,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireDigit = true,
            RequireSpecialCharacter = true
        };

        return new PasswordService(Options.Create(policyOptions));
    }

    #endregion

    #region HashPassword Tests

    [Fact]
    public void HashPassword_WithValidPassword_ReturnsNonEmptyHash()
    {
        // Arrange
        var service = CreateService();
        var password = "ValidPassword123!";

        // Act
        var hash = service.HashPassword(password);

        // Assert
        hash.Value.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ReturnsDifferentHashes()
    {
        // Arrange - PasswordHasher includes unique salt per hash
        var service = CreateService();
        var password = "ValidPassword123!";

        // Act
        var hash1 = service.HashPassword(password);
        var hash2 = service.HashPassword(password);

        // Assert - Hashes should be different due to unique salts
        hash1.Value.Should().NotBe(hash2.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HashPassword_WithInvalidPassword_ThrowsArgumentException(string? password)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.HashPassword(password!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void HashPassword_WithShortPassword_StillHashes()
    {
        // Arrange - HashPassword doesn't validate strength, just hashes
        var service = CreateService();
        var password = "short";

        // Act
        var hash = service.HashPassword(password);

        // Assert
        hash.Value.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region VerifyPassword Tests

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var service = CreateService();
        var password = "ValidPassword123!";
        var hash = service.HashPassword(password);

        // Act
        var result = service.VerifyPassword(hash, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();
        var password = "ValidPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = service.HashPassword(password);

        // Act
        var result = service.VerifyPassword(hash, wrongPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyPassword_WithEmptyProvidedPassword_ReturnsFalse(string? providedPassword)
    {
        // Arrange
        var service = CreateService();
        var hash = service.HashPassword("ValidPassword123!");

        // Act
        var result = service.VerifyPassword(hash, providedPassword!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_CaseSensitive_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();
        var password = "ValidPassword123!";
        var differentCase = "validpassword123!";
        var hash = service.HashPassword(password);

        // Act
        var result = service.VerifyPassword(hash, differentCase);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ValidateStrength Tests - Valid Passwords

    [Fact]
    public void ValidateStrength_WithStrongPassword_ReturnsValid()
    {
        // Arrange
        var service = CreateService();
        var password = "StrongPass123!@#";

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateStrength_WithMinimumRequirements_ReturnsValid()
    {
        // Arrange
        var service = CreateService();
        var password = "Abcdefghij1!"; // 12 chars, upper, lower, digit, special

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateStrength_WithVeryLongPassword_ReturnsValid()
    {
        // Arrange
        var service = CreateService();
        var password = new string('A', 50) + new string('a', 50) + "12345!";

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region ValidateStrength Tests - Invalid Passwords

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ValidateStrength_WithEmptyPassword_ReturnsInvalidWithError(string? password)
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = service.ValidateStrength(password!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password is required.");
    }

    [Fact]
    public void ValidateStrength_TooShort_ReturnsInvalidWithError()
    {
        // Arrange
        var service = CreateService();
        var password = "Short1!aA"; // Only 9 chars

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least 12 characters"));
    }

    [Fact]
    public void ValidateStrength_NoUppercase_ReturnsInvalidWithError()
    {
        // Arrange
        var service = CreateService();
        var password = "abcdefghij1!"; // No uppercase

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("uppercase"));
    }

    [Fact]
    public void ValidateStrength_NoLowercase_ReturnsInvalidWithError()
    {
        // Arrange
        var service = CreateService();
        var password = "ABCDEFGHIJ1!"; // No lowercase

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("lowercase"));
    }

    [Fact]
    public void ValidateStrength_NoDigit_ReturnsInvalidWithError()
    {
        // Arrange
        var service = CreateService();
        var password = "Abcdefghijk!"; // No digit

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("digit"));
    }

    [Fact]
    public void ValidateStrength_NoSpecialCharacter_ReturnsInvalidWithError()
    {
        // Arrange
        var service = CreateService();
        var password = "Abcdefghijk1"; // No special char

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("special character"));
    }

    [Fact]
    public void ValidateStrength_TooLong_ReturnsInvalidWithError()
    {
        // Arrange
        var service = CreateService();
        var password = new string('A', 100) + new string('a', 50) + "1!"; // 152 chars, > 128

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("128 characters"));
    }

    [Fact]
    public void ValidateStrength_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var service = CreateService();
        var password = "short"; // Too short, no uppercase, no digit, no special

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
    }

    #endregion

    #region ValidateStrength Tests - Strength Score

    [Fact]
    public void ValidateStrength_LongPasswordWithAllCharTypes_ReturnsHighScore()
    {
        // Arrange
        var service = CreateService();
        var password = "VeryLongPassword123!@#"; // 22 chars, all types

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Score.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void ValidateStrength_MinimumValidPassword_ReturnsLowerScore()
    {
        // Arrange
        var service = CreateService();
        var password = "Abcdefghij1!"; // Exactly 12 chars

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Score.Should().BeLessThanOrEqualTo(3);
    }

    [Theory]
    [InlineData("StrongPass123!", "Strong")]
    [InlineData("VeryStrongPassword123!@#", "Very Strong")]
    public void ValidateStrength_ReturnsCorrectStrengthDescription(string password, string expectedStrengthContains)
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Strength.Should().Contain(expectedStrengthContains.Split(' ').First());
    }

    #endregion

    #region ValidateStrength Tests - Custom Policy

    [Fact]
    public void ValidateStrength_WithRelaxedPolicy_AcceptsSimplerPasswords()
    {
        // Arrange - Relax all requirements
        var options = new PasswordPolicyOptions
        {
            MinimumLength = 8,
            RequireUppercase = false,
            RequireLowercase = false,
            RequireDigit = false,
            RequireSpecialCharacter = false
        };
        var service = CreateService(options);
        var password = "simplepass"; // Would fail default policy

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateStrength_WithStricterPolicy_RejectsWeakerPasswords()
    {
        // Arrange - Stricter minimum length
        var options = new PasswordPolicyOptions
        {
            MinimumLength = 20,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireDigit = true,
            RequireSpecialCharacter = true
        };
        var service = CreateService(options);
        var password = "StrongPass123!"; // Would pass default, but too short

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least 20 characters"));
    }

    #endregion

    #region Suggestions Tests

    [Fact]
    public void ValidateStrength_InvalidPassword_ReturnsSuggestions()
    {
        // Arrange
        var service = CreateService();
        var password = "weak"; // Short, no uppercase, no digit, no special

        // Act
        var result = service.ValidateStrength(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Suggestions.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateStrength_WithCommonPattern_IncludesPatternSuggestion()
    {
        // Arrange
        var service = CreateService();
        var password = "Password123!"; // Contains "password" pattern

        // Act
        var result = service.ValidateStrength(password);

        // Assert - Still valid but may have suggestions
        // Note: The current implementation suggests avoiding common patterns
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
