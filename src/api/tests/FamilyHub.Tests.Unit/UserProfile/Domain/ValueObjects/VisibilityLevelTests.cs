using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.Tests.Unit.UserProfile.Domain.ValueObjects;

/// <summary>
/// Unit tests for VisibilityLevel value object.
/// </summary>
public class VisibilityLevelTests
{
    [Fact]
    public void Hidden_ShouldHaveCorrectValue()
    {
        // Act
        var level = VisibilityLevel.Hidden;

        // Assert
        level.Value.Should().Be("hidden");
    }

    [Fact]
    public void Family_ShouldHaveCorrectValue()
    {
        // Act
        var level = VisibilityLevel.Family;

        // Assert
        level.Value.Should().Be("family");
    }

    [Fact]
    public void Public_ShouldHaveCorrectValue()
    {
        // Act
        var level = VisibilityLevel.Public;

        // Assert
        level.Value.Should().Be("public");
    }

    [Theory]
    [InlineData("hidden")]
    [InlineData("family")]
    [InlineData("public")]
    public void From_WithValidLevel_ShouldCreateVisibilityLevel(string levelValue)
    {
        // Act
        var level = VisibilityLevel.From(levelValue);

        // Assert
        level.Value.Should().Be(levelValue);
    }

    [Theory]
    [InlineData("Hidden")]
    [InlineData("FAMILY")]
    [InlineData("Public")]
    public void From_WithMixedCase_ShouldNormalizeToLowerCase(string levelValue)
    {
        // Act
        var level = VisibilityLevel.From(levelValue);

        // Assert
        level.Value.Should().Be(levelValue.ToLowerInvariant());
    }

    [Theory]
    [InlineData("  hidden  ")]
    [InlineData("\tfamily\t")]
    [InlineData(" public ")]
    public void From_WithWhitespace_ShouldTrimAndNormalize(string levelValue)
    {
        // Act
        var level = VisibilityLevel.From(levelValue);

        // Assert
        level.Value.Should().NotContain(" ");
        level.Value.Should().NotContain("\t");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void From_WithEmptyOrWhitespace_ShouldThrowException(string levelValue)
    {
        // Act
        var act = () => VisibilityLevel.From(levelValue);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Visibility level cannot be empty*");
    }

    [Theory]
    [InlineData("private")]
    [InlineData("friends")]
    [InlineData("everyone")]
    [InlineData("invalid")]
    public void From_WithInvalidLevel_ShouldThrowException(string levelValue)
    {
        // Act
        var act = () => VisibilityLevel.From(levelValue);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Invalid visibility level*");
    }

    [Fact]
    public void Equals_WithSameLevel_ShouldBeEqual()
    {
        // Arrange
        var level1 = VisibilityLevel.From("hidden");
        var level2 = VisibilityLevel.From("hidden");

        // Act & Assert
        level1.Should().Be(level2);
        (level1 == level2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentLevels_ShouldNotBeEqual()
    {
        // Arrange
        var level1 = VisibilityLevel.From("hidden");
        var level2 = VisibilityLevel.From("family");

        // Act & Assert
        level1.Should().NotBe(level2);
        (level1 != level2).Should().BeTrue();
    }

    [Fact]
    public void Hidden_ShouldBeEqualToHiddenFromString()
    {
        // Arrange
        var hiddenStatic = VisibilityLevel.Hidden;
        var hiddenFromString = VisibilityLevel.From("hidden");

        // Act & Assert
        hiddenStatic.Should().Be(hiddenFromString);
    }

    [Fact]
    public void ToString_ShouldReturnLevelValue()
    {
        // Arrange
        var level = VisibilityLevel.Family;

        // Act
        var result = level.ToString();

        // Assert
        result.Should().Be("family");
    }

    [Fact]
    public void GetHashCode_WithSameLevel_ShouldReturnSameHashCode()
    {
        // Arrange
        var level1 = VisibilityLevel.From("family");
        var level2 = VisibilityLevel.From("family");

        // Act & Assert
        level1.GetHashCode().Should().Be(level2.GetHashCode());
    }
}
