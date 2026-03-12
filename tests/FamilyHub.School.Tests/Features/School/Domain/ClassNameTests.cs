using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FluentAssertions;
using Vogen;

namespace FamilyHub.School.Tests.Features.School.Domain;

public class ClassNameTests
{
    [Fact]
    public void From_ShouldCreateClassName_WithValidValue()
    {
        // Arrange & Act
        var className = ClassName.From("1a");

        // Assert
        className.Value.Should().Be("1a");
    }

    [Fact]
    public void From_ShouldThrow_WhenValueIsEmpty()
    {
        // Arrange & Act
        var act = () => ClassName.From("");

        // Assert
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_ShouldThrow_WhenValueIsWhitespace()
    {
        // Arrange & Act
        var act = () => ClassName.From("   ");

        // Assert
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_ShouldThrow_WhenValueIsTooLong()
    {
        // Arrange
        var longName = new string('A', 21);

        // Act
        var act = () => ClassName.From(longName);

        // Assert
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_ShouldAcceptMaxLengthValue()
    {
        // Arrange
        var maxName = new string('A', 20);

        // Act
        var className = ClassName.From(maxName);

        // Assert
        className.Value.Should().HaveLength(20);
    }

    [Fact]
    public void From_ShouldAcceptTypicalClassNames()
    {
        // Arrange & Act & Assert
        ClassName.From("1a").Value.Should().Be("1a");
        ClassName.From("10b").Value.Should().Be("10b");
        ClassName.From("5").Value.Should().Be("5");
    }
}
