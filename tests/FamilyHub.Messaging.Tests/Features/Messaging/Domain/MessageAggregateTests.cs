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
        var message = Message.Create(familyId, senderId, content, utcNow: DateTimeOffset.UtcNow);

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
        var message = Message.Create(familyId, senderId, content, utcNow: DateTimeOffset.UtcNow);

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
        var message1 = Message.Create(familyId, senderId, content, utcNow: DateTimeOffset.UtcNow);
        var message2 = Message.Create(familyId, senderId, content, utcNow: DateTimeOffset.UtcNow);

        // Assert
        message1.Id.Should().NotBe(message2.Id);
    }

    [Fact]
    public void Create_WithAttachments_ShouldIncludeAttachments()
    {
        // Arrange
        var familyId = FamilyId.New();
        var senderId = UserId.New();
        var content = MessageContent.From("Check out these files!");
        var attachments = new[]
        {
            MessageAttachment.Create(FileId.New(), "photo.jpg", "image/jpeg", 2048, "uploads/photo.jpg", DateTimeOffset.UtcNow),
            MessageAttachment.Create(FileId.New(), "doc.pdf", "application/pdf", 4096, "uploads/doc.pdf", DateTimeOffset.UtcNow),
        };

        // Act
        var message = Message.Create(familyId, senderId, content, attachments, utcNow: DateTimeOffset.UtcNow);

        // Assert
        message.Attachments.Should().HaveCount(2);
        message.Attachments[0].FileName.Should().Be("photo.jpg");
        message.Attachments[1].FileName.Should().Be("doc.pdf");
    }

    [Fact]
    public void Create_WithAttachments_ShouldRaiseAttachmentAddedEvents()
    {
        // Arrange
        var familyId = FamilyId.New();
        var senderId = UserId.New();
        var content = MessageContent.From("Files attached");
        var fileId1 = FileId.New();
        var fileId2 = FileId.New();
        var attachments = new[]
        {
            MessageAttachment.Create(fileId1, "a.txt", "text/plain", 100, "uploads/a.txt", DateTimeOffset.UtcNow),
            MessageAttachment.Create(fileId2, "b.txt", "text/plain", 200, "uploads/b.txt", DateTimeOffset.UtcNow),
        };

        // Act
        var message = Message.Create(familyId, senderId, content, attachments, utcNow: DateTimeOffset.UtcNow);

        // Assert — 1 MessageSentEvent + 2 MessageAttachmentAddedEvent
        message.DomainEvents.Should().HaveCount(3);
        message.DomainEvents.OfType<MessageSentEvent>().Should().HaveCount(1);
        message.DomainEvents.OfType<MessageAttachmentAddedEvent>().Should().HaveCount(2);

        var attachmentEvents = message.DomainEvents.OfType<MessageAttachmentAddedEvent>().ToList();
        attachmentEvents[0].FileId.Should().Be(fileId1);
        attachmentEvents[1].FileId.Should().Be(fileId2);
        attachmentEvents.Should().AllSatisfy(e =>
        {
            e.MessageId.Should().Be(message.Id);
            e.FamilyId.Should().Be(familyId);
            e.SenderId.Should().Be(senderId);
        });
    }

    [Fact]
    public void Create_WithoutAttachments_ShouldHaveEmptyCollection()
    {
        // Arrange & Act
        var message = Message.Create(
            FamilyId.New(),
            UserId.New(),
            MessageContent.From("No attachments"), utcNow: DateTimeOffset.UtcNow);

        // Assert
        message.Attachments.Should().BeEmpty();
    }
}
