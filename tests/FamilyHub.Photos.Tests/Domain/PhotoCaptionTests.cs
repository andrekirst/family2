using FluentAssertions;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;
using Vogen;

namespace FamilyHub.Photos.Tests.Domain;

public class PhotoCaptionTests
{
    [Fact]
    public void From_WithValidCaption_ShouldCreate()
    {
        // Act
        var caption = PhotoCaption.From("Family dinner");

        // Assert
        caption.Value.Should().Be("Family dinner");
    }

    [Fact]
    public void From_WithEmptyString_ShouldThrow()
    {
        // Act & Assert
        var act = () => PhotoCaption.From("");
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_WithWhitespace_ShouldThrow()
    {
        // Act & Assert
        var act = () => PhotoCaption.From("   ");
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_WithExceeding500Characters_ShouldThrow()
    {
        // Arrange
        var longCaption = new string('a', 501);

        // Act & Assert
        var act = () => PhotoCaption.From(longCaption);
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_WithExactly500Characters_ShouldCreate()
    {
        // Arrange
        var caption = new string('a', 500);

        // Act
        var result = PhotoCaption.From(caption);

        // Assert
        result.Value.Should().HaveLength(500);
    }
}
