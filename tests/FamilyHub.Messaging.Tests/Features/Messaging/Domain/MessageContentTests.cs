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
    public void From_WithEmptyString_ShouldSucceed()
    {
        // Empty content is valid for attachment-only messages
        var content = MessageContent.From("");
        content.Value.Should().BeEmpty();
    }

    [Fact]
    public void From_WithWhitespace_ShouldSucceed()
    {
        // Whitespace content is valid (trimmed to empty by mutation layer)
        var content = MessageContent.From("   ");
        content.Value.Should().Be("   ");
    }

    [Fact]
    public void Empty_ShouldReturnEmptyContent()
    {
        // MessageContent.Empty is a convenience for attachment-only messages
        var content = MessageContent.Empty;
        content.Value.Should().BeEmpty();
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
