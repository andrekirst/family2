using FamilyHub.Modules.Auth.Infrastructure.Services;
using FluentAssertions;

namespace FamilyHub.Tests.Unit.Auth.Infrastructure.Configuration;

/// <summary>
/// Unit tests for PasswordPolicyOptions.
/// Tests default values and configuration options.
/// </summary>
public sealed class PasswordPolicyOptionsTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultMinimumLength_Is12()
    {
        // Arrange & Act
        var options = new PasswordPolicyOptions();

        // Assert
        options.MinimumLength.Should().Be(12);
    }

    [Fact]
    public void DefaultRequireUppercase_IsTrue()
    {
        // Arrange & Act
        var options = new PasswordPolicyOptions();

        // Assert
        options.RequireUppercase.Should().BeTrue();
    }

    [Fact]
    public void DefaultRequireLowercase_IsTrue()
    {
        // Arrange & Act
        var options = new PasswordPolicyOptions();

        // Assert
        options.RequireLowercase.Should().BeTrue();
    }

    [Fact]
    public void DefaultRequireDigit_IsTrue()
    {
        // Arrange & Act
        var options = new PasswordPolicyOptions();

        // Assert
        options.RequireDigit.Should().BeTrue();
    }

    [Fact]
    public void DefaultRequireSpecialCharacter_IsTrue()
    {
        // Arrange & Act
        var options = new PasswordPolicyOptions();

        // Assert
        options.RequireSpecialCharacter.Should().BeTrue();
    }

    [Fact]
    public void SectionName_IsAuthenticationPasswordPolicy()
    {
        // Assert
        PasswordPolicyOptions.SectionName.Should().Be("Authentication:PasswordPolicy");
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void CanSetMinimumLength()
    {
        // Arrange & Act
        var options = new PasswordPolicyOptions { MinimumLength = 16 };

        // Assert
        options.MinimumLength.Should().Be(16);
    }

    [Fact]
    public void CanDisableUppercaseRequirement()
    {
        // Arrange & Act
        var options = new PasswordPolicyOptions { RequireUppercase = false };

        // Assert
        options.RequireUppercase.Should().BeFalse();
    }

    [Fact]
    public void CanDisableLowercaseRequirement()
    {
        // Arrange & Act
        var options = new PasswordPolicyOptions { RequireLowercase = false };

        // Assert
        options.RequireLowercase.Should().BeFalse();
    }

    [Fact]
    public void CanDisableDigitRequirement()
    {
        // Arrange & Act
        var options = new PasswordPolicyOptions { RequireDigit = false };

        // Assert
        options.RequireDigit.Should().BeFalse();
    }

    [Fact]
    public void CanDisableSpecialCharacterRequirement()
    {
        // Arrange & Act
        var options = new PasswordPolicyOptions { RequireSpecialCharacter = false };

        // Assert
        options.RequireSpecialCharacter.Should().BeFalse();
    }

    #endregion

    #region Policy Combination Tests

    [Fact]
    public void CanCreateRelaxedPolicy()
    {
        // Arrange & Act - Minimal security policy
        var options = new PasswordPolicyOptions
        {
            MinimumLength = 6,
            RequireUppercase = false,
            RequireLowercase = false,
            RequireDigit = false,
            RequireSpecialCharacter = false
        };

        // Assert
        options.MinimumLength.Should().Be(6);
        options.RequireUppercase.Should().BeFalse();
        options.RequireLowercase.Should().BeFalse();
        options.RequireDigit.Should().BeFalse();
        options.RequireSpecialCharacter.Should().BeFalse();
    }

    [Fact]
    public void CanCreateStrictPolicy()
    {
        // Arrange & Act - High security policy
        var options = new PasswordPolicyOptions
        {
            MinimumLength = 20,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireDigit = true,
            RequireSpecialCharacter = true
        };

        // Assert
        options.MinimumLength.Should().Be(20);
        options.RequireUppercase.Should().BeTrue();
        options.RequireLowercase.Should().BeTrue();
        options.RequireDigit.Should().BeTrue();
        options.RequireSpecialCharacter.Should().BeTrue();
    }

    [Fact]
    public void CanCreatePartialPolicy()
    {
        // Arrange & Act - Only require letters
        var options = new PasswordPolicyOptions
        {
            MinimumLength = 10,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireDigit = false,
            RequireSpecialCharacter = false
        };

        // Assert
        options.MinimumLength.Should().Be(10);
        options.RequireUppercase.Should().BeTrue();
        options.RequireLowercase.Should().BeTrue();
        options.RequireDigit.Should().BeFalse();
        options.RequireSpecialCharacter.Should().BeFalse();
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void MinimumLength_AcceptsAnyPositiveValue(int length)
    {
        // Arrange & Act
        var options = new PasswordPolicyOptions { MinimumLength = length };

        // Assert - No validation at options level
        options.MinimumLength.Should().Be(length);
    }

    [Fact]
    public void MinimumLength_AcceptsNegativeValue()
    {
        // Arrange & Act - Options don't validate, service does
        var options = new PasswordPolicyOptions { MinimumLength = -1 };

        // Assert
        options.MinimumLength.Should().Be(-1);
    }

    #endregion
}
