using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
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
            MessageContent.From("Hello, family!"))
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MessageId.Value.Should().NotBe(Guid.Empty);
        result.SentMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldAddMessageToRepository()
    {
        // Arrange
        var familyId = FamilyId.New();
        var senderId = UserId.New();
        var content = MessageContent.From("Test message");
        var (handler, messageRepo) = CreateHandler();
        var command = new SendMessageCommand(content)
        {
            FamilyId = familyId,
            UserId = senderId
        };

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
            MessageContent.From("Event test"))
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

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
        var familyId = FamilyId.New();
        var senderId = UserId.New();
        var (handler, messageRepo) = CreateHandler(familyId, senderId);
        var command = new SendMessageCommand(
            MessageContent.From("See attached"),
            [
                new AttachmentData("uploads/photo.jpg", "photo.jpg", "image/jpeg", 1024, "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2"),
                new AttachmentData("uploads/doc.pdf", "doc.pdf", "application/pdf", 2048, "b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3")
            ])
        {
            FamilyId = familyId,
            UserId = senderId
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var message = messageRepo.AddedMessages[0];
        message.Attachments.Should().HaveCount(2);
        message.Attachments[0].FileName.Should().Be("photo.jpg");
        message.Attachments[0].MimeType.Should().Be("image/jpeg");
        message.Attachments[0].FileSize.Should().Be(1024);
        message.Attachments[1].FileName.Should().Be("doc.pdf");
    }

    [Fact]
    public async Task Handle_WithNoAttachments_ShouldCreateMessageWithoutAttachments()
    {
        // Arrange
        var (handler, messageRepo) = CreateHandler();
        var command = new SendMessageCommand(
            MessageContent.From("No files here"))
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var message = messageRepo.AddedMessages[0];
        message.Attachments.Should().BeEmpty();
    }

    // --- Helpers ---

    private static (SendMessageCommandHandler Handler, FakeMessageRepository MessageRepo) CreateHandler(
        FamilyId? familyId = null, UserId? userId = null)
    {
        var messageRepo = new FakeMessageRepository();
        var storedFileRepo = new FakeStoredFileRepository();
        var folderRepo = new FakeFolderRepository();
        var conversationRepo = new FakeConversationRepository();

        // Seed a root folder when familyId is provided (needed for attachment resolution)
        if (familyId is not null)
        {
            var rootFolder = Folder.CreateRoot(familyId.Value, userId ?? UserId.New());
            folderRepo.Folders.Add(rootFolder);
        }

        var handler = new SendMessageCommandHandler(messageRepo, storedFileRepo, folderRepo, conversationRepo);
        return (handler, messageRepo);
    }
}
