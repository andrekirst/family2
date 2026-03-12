using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FluentAssertions;
using Vogen;

namespace FamilyHub.School.Tests.Features.School.Domain;

public class SchoolNameTests
{
    [Fact]
    public void From_ShouldCreateSchoolName_WithValidValue()
    {
        // Arrange & Act
        var schoolName = SchoolName.From("Grundschule am Park");

        // Assert
        schoolName.Value.Should().Be("Grundschule am Park");
    }

    [Fact]
    public void From_ShouldThrow_WhenValueIsEmpty()
    {
        // Arrange & Act
        var act = () => SchoolName.From("");

        // Assert
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_ShouldThrow_WhenValueIsWhitespace()
    {
        // Arrange & Act
        var act = () => SchoolName.From("   ");

        // Assert
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_ShouldThrow_WhenValueIsTooShort()
    {
        // Arrange & Act
        var act = () => SchoolName.From("A");

        // Assert
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_ShouldThrow_WhenValueIsTooLong()
    {
        // Arrange
        var longName = new string('A', 201);

        // Act
        var act = () => SchoolName.From(longName);

        // Assert
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_ShouldAcceptMaxLengthValue()
    {
        // Arrange
        var maxName = new string('A', 200);

        // Act
        var schoolName = SchoolName.From(maxName);

        // Assert
        schoolName.Value.Should().HaveLength(200);
    }

    [Fact]
    public void From_ShouldAcceptMinLengthValue()
    {
        // Arrange & Act
        var schoolName = SchoolName.From("AB");

        // Assert
        schoolName.Value.Should().Be("AB");
    }
}
