using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Application.Commands.SendMessage;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

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
        await messageRepo.Received(1).AddAsync(
            Arg.Is<Message>(m =>
                m.FamilyId == familyId &&
                m.SenderId == senderId &&
                m.Content == content),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRaiseMessageSentEvent()
    {
        // Arrange
        Message? capturedMessage = null;
        var (handler, messageRepo) = CreateHandler();
        messageRepo.AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(callInfo => capturedMessage = callInfo.ArgAt<Message>(0));

        var command = new SendMessageCommand(
            MessageContent.From("Event test"))
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedMessage.Should().NotBeNull();
        capturedMessage!.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithAttachments_ShouldCreateMessageWithAttachments()
    {
        // Arrange
        var familyId = FamilyId.New();
        var senderId = UserId.New();
        Message? capturedMessage = null;
        var (handler, messageRepo) = CreateHandler(familyId, senderId);
        messageRepo.AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(callInfo => capturedMessage = callInfo.ArgAt<Message>(0));

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
        capturedMessage.Should().NotBeNull();
        capturedMessage!.Attachments.Should().HaveCount(2);
        capturedMessage.Attachments[0].FileName.Should().Be("photo.jpg");
        capturedMessage.Attachments[0].MimeType.Should().Be("image/jpeg");
        capturedMessage.Attachments[0].FileSize.Should().Be(1024);
        capturedMessage.Attachments[1].FileName.Should().Be("doc.pdf");
    }

    [Fact]
    public async Task Handle_WithNoAttachments_ShouldCreateMessageWithoutAttachments()
    {
        // Arrange
        Message? capturedMessage = null;
        var (handler, messageRepo) = CreateHandler();
        messageRepo.AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(callInfo => capturedMessage = callInfo.ArgAt<Message>(0));

        var command = new SendMessageCommand(
            MessageContent.From("No files here"))
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedMessage.Should().NotBeNull();
        capturedMessage!.Attachments.Should().BeEmpty();
    }

    // --- Helpers ---

    private static (SendMessageCommandHandler Handler, IMessageRepository MessageRepo) CreateHandler(
        FamilyId? familyId = null, UserId? userId = null)
    {
        var messageRepo = Substitute.For<IMessageRepository>();
        var storedFileRepo = Substitute.For<IStoredFileRepository>();
        var folderRepo = Substitute.For<IFolderRepository>();
        var conversationRepo = Substitute.For<IConversationRepository>();

        // Seed a root folder when familyId is provided (needed for attachment resolution)
        if (familyId is not null)
        {
            var rootFolder = Folder.CreateRoot(familyId.Value, userId ?? UserId.New(), DateTimeOffset.UtcNow);
            folderRepo.GetRootFolderAsync(familyId.Value, Arg.Any<CancellationToken>())
                .Returns(rootFolder);
        }

        var handler = new SendMessageCommandHandler(messageRepo, storedFileRepo, folderRepo, conversationRepo, TimeProvider.System);
        return (handler, messageRepo);
    }
}
