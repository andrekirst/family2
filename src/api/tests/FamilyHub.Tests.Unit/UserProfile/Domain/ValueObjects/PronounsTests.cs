using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.Tests.Unit.UserProfile.Domain.ValueObjects;

/// <summary>
/// Unit tests for Pronouns value object.
/// </summary>
public class PronounsTests
{
    [Theory]
    [InlineData("he/him")]
    [InlineData("she/her")]
    [InlineData("they/them")]
    [InlineData("he/they")]
    [InlineData("she/they")]
    [InlineData("any")]
    public void From_WithValidPronouns_ShouldSucceed(string pronouns)
    {
        // Act
        var result = Pronouns.From(pronouns);

        // Assert
        result.Value.Should().Be(pronouns);
    }

    [Fact]
    public void From_WithWhitespace_ShouldTrim()
    {
        // Act
        var pronouns = Pronouns.From("  he/him  ");

        // Assert
        pronouns.Value.Should().Be("he/him");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void From_WithEmptyOrWhitespace_ShouldAllowAsOptional(string optionalValue)
    {
        // Pronouns is an optional field, so empty/whitespace values are allowed
        // and normalized to empty string

        // Act
        var pronouns = Pronouns.From(optionalValue);

        // Assert
        pronouns.Value.Should().BeEmpty();
    }

    [Fact]
    public void From_WithNull_ShouldThrow()
    {
        // Act
        var act = () => Pronouns.From(null!);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>();
    }

    [Fact]
    public void From_WithExactly50Characters_ShouldSucceed()
    {
        // Arrange
        var maxLengthPronouns = new string('a', 50);

        // Act
        var pronouns = Pronouns.From(maxLengthPronouns);

        // Assert
        pronouns.Value.Should().Be(maxLengthPronouns);
    }

    [Fact]
    public void From_WithMoreThan50Characters_ShouldThrow()
    {
        // Arrange
        var longPronouns = new string('a', 51);

        // Act
        var act = () => Pronouns.From(longPronouns);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Pronouns cannot exceed 50 characters*");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var pronouns1 = Pronouns.From("they/them");
        var pronouns2 = Pronouns.From("they/them");

        // Act & Assert
        pronouns1.Should().Be(pronouns2);
        (pronouns1 == pronouns2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var pronouns1 = Pronouns.From("he/him");
        var pronouns2 = Pronouns.From("she/her");

        // Act & Assert
        pronouns1.Should().NotBe(pronouns2);
        (pronouns1 != pronouns2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var pronouns1 = Pronouns.From("they/them");
        var pronouns2 = Pronouns.From("they/them");

        // Act & Assert
        pronouns1.GetHashCode().Should().Be(pronouns2.GetHashCode());
    }
}
