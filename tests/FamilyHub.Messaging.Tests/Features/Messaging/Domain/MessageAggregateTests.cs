using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.Events;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.Messaging.Tests.Features.Messaging.Domain;

public class MessageAggregateTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange
        var familyId = FamilyId.New();
        var senderId = UserId.New();
        var content = MessageContent.From("Hello, family!");

        // Act
        var message = Message.Create(familyId, senderId, content);

        // Assert
        message.Should().NotBeNull();
        message.Id.Value.Should().NotBe(Guid.Empty);
        message.FamilyId.Should().Be(familyId);
        message.SenderId.Should().Be(senderId);
        message.Content.Should().Be(content);
        message.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldRaiseMessageSentEvent()
    {
        // Arrange
        var familyId = FamilyId.New();
        var senderId = UserId.New();
        var content = MessageContent.From("Hello, family!");

        // Act
        var message = Message.Create(familyId, senderId, content);

        // Assert
        message.DomainEvents.Should().HaveCount(1);
        var domainEvent = message.DomainEvents.First();
        domainEvent.Should().BeOfType<MessageSentEvent>();

        var messageSentEvent = (MessageSentEvent)domainEvent;
        messageSentEvent.MessageId.Should().Be(message.Id);
        messageSentEvent.FamilyId.Should().Be(familyId);
        messageSentEvent.SenderId.Should().Be(senderId);
        messageSentEvent.Content.Should().Be(content);
        messageSentEvent.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange
        var familyId = FamilyId.New();
        var senderId = UserId.New();
        var content = MessageContent.From("Hello!");

        // Act
        var message1 = Message.Create(familyId, senderId, content);
        var message2 = Message.Create(familyId, senderId, content);

        // Assert
        message1.Id.Should().NotBe(message2.Id);
    }
}
