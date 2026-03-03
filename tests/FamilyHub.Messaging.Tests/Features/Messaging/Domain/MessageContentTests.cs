using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using FluentAssertions;
using Vogen;

namespace FamilyHub.Messaging.Tests.Features.Messaging.Domain;

public class MessageContentTests
{
    [Fact]
    public void From_WithValidContent_ShouldCreateMessageContent()
    {
        // Act
        var content = MessageContent.From("Hello, family!");

        // Assert
        content.Value.Should().Be("Hello, family!");
    }

    [Fact]
    public void From_WithEmptyString_ShouldThrowValidationException()
    {
        // Act & Assert
        var act = () => MessageContent.From("");
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_WithWhitespace_ShouldThrowValidationException()
    {
        // Act & Assert
        var act = () => MessageContent.From("   ");
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_WithContentExceedingMaxLength_ShouldThrowValidationException()
    {
        // Arrange
        var longContent = new string('a', MessageContent.MaxLength + 1);

        // Act & Assert
        var act = () => MessageContent.From(longContent);
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_WithContentAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var maxContent = new string('a', MessageContent.MaxLength);

        // Act
        var content = MessageContent.From(maxContent);

        // Assert
        content.Value.Should().HaveLength(MessageContent.MaxLength);
    }
}
