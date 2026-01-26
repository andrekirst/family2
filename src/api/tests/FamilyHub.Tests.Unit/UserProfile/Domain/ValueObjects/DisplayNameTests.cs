using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.Tests.Unit.UserProfile.Domain.ValueObjects;

/// <summary>
/// Unit tests for DisplayName value object.
/// </summary>
public class DisplayNameTests
{
    [Fact]
    public void From_WithValidDisplayName_ShouldSucceed()
    {
        // Act
        var displayName = DisplayName.From("John Doe");

        // Assert
        displayName.Value.Should().Be("John Doe");
    }

    [Fact]
    public void From_WithWhitespace_ShouldTrim()
    {
        // Act
        var displayName = DisplayName.From("  John Doe  ");

        // Assert
        displayName.Value.Should().Be("John Doe");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void From_WithEmptyOrWhitespace_ShouldThrow(string invalidValue)
    {
        // Act
        var act = () => DisplayName.From(invalidValue);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Display name cannot be empty*");
    }

    [Fact]
    public void From_WithNull_ShouldThrow()
    {
        // Act
        var act = () => DisplayName.From(null!);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>();
    }

    [Fact]
    public void From_WithExactly100Characters_ShouldSucceed()
    {
        // Arrange
        var maxLengthName = new string('A', 100);

        // Act
        var displayName = DisplayName.From(maxLengthName);

        // Assert
        displayName.Value.Should().Be(maxLengthName);
    }

    [Fact]
    public void From_WithMoreThan100Characters_ShouldThrow()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var act = () => DisplayName.From(longName);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Display name cannot exceed 100 characters*");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var displayName1 = DisplayName.From("John Doe");
        var displayName2 = DisplayName.From("John Doe");

        // Act & Assert
        displayName1.Should().Be(displayName2);
        (displayName1 == displayName2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var displayName1 = DisplayName.From("John Doe");
        var displayName2 = DisplayName.From("Jane Doe");

        // Act & Assert
        displayName1.Should().NotBe(displayName2);
        (displayName1 != displayName2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var displayName1 = DisplayName.From("John Doe");
        var displayName2 = DisplayName.From("John Doe");

        // Act & Assert
        displayName1.GetHashCode().Should().Be(displayName2.GetHashCode());
    }
}
