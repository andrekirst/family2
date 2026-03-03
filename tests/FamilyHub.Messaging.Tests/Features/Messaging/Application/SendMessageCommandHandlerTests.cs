using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Application.Commands.SendMessage;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Messaging.Tests.Features.Messaging.Application;

public class SendMessageCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateMessageAndReturnResult()
    {
        // Arrange
        var (handler, _) = CreateHandler();
        var command = new SendMessageCommand(
            FamilyId.New(),
            UserId.New(),
            MessageContent.From("Hello, family!"));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MessageId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldAddMessageToRepository()
    {
        // Arrange
        var familyId = FamilyId.New();
        var senderId = UserId.New();
        var content = MessageContent.From("Test message");
        var (handler, messageRepo) = CreateHandler();
        var command = new SendMessageCommand(familyId, senderId, content);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        messageRepo.AddedMessages.Should().HaveCount(1);
        var addedMessage = messageRepo.AddedMessages[0];
        addedMessage.FamilyId.Should().Be(familyId);
        addedMessage.SenderId.Should().Be(senderId);
        addedMessage.Content.Should().Be(content);
    }

    [Fact]
    public async Task Handle_ShouldRaiseMessageSentEvent()
    {
        // Arrange
        var (handler, messageRepo) = CreateHandler();
        var command = new SendMessageCommand(
            FamilyId.New(),
            UserId.New(),
            MessageContent.From("Event test"));

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var addedMessage = messageRepo.AddedMessages[0];
        addedMessage.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithAttachments_ShouldCreateMessageWithAttachments()
    {
        // Arrange
        var (handler, messageRepo) = CreateHandler();
        var fileId1 = FileId.New();
        var fileId2 = FileId.New();
        var command = new SendMessageCommand(
            FamilyId.New(),
            UserId.New(),
            MessageContent.From("See attached"),
            [
                new AttachmentData(fileId1, "photo.jpg", "image/jpeg", 1024),
                new AttachmentData(fileId2, "doc.pdf", "application/pdf", 2048)
            ]);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var message = messageRepo.AddedMessages[0];
        message.Attachments.Should().HaveCount(2);
        message.Attachments[0].FileId.Should().Be(fileId1);
        message.Attachments[0].FileName.Should().Be("photo.jpg");
        message.Attachments[0].MimeType.Should().Be("image/jpeg");
        message.Attachments[0].FileSize.Should().Be(1024);
        message.Attachments[1].FileId.Should().Be(fileId2);
        message.Attachments[1].FileName.Should().Be("doc.pdf");
    }

    [Fact]
    public async Task Handle_WithNoAttachments_ShouldCreateMessageWithoutAttachments()
    {
        // Arrange
        var (handler, messageRepo) = CreateHandler();
        var command = new SendMessageCommand(
            FamilyId.New(),
            UserId.New(),
            MessageContent.From("No files here"));

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var message = messageRepo.AddedMessages[0];
        message.Attachments.Should().BeEmpty();
    }

    // --- Helpers ---

    private static (SendMessageCommandHandler Handler, FakeMessageRepository MessageRepo) CreateHandler()
    {
        var messageRepo = new FakeMessageRepository();
        var handler = new SendMessageCommandHandler(messageRepo);
        return (handler, messageRepo);
    }
}
