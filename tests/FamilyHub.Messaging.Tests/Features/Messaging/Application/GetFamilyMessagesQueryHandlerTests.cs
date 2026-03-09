using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Application.Queries.GetFamilyMessages;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Messaging.Tests.Features.Messaging.Application;

public class GetFamilyMessagesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnMessagesForFamily()
    {
        // Arrange
        var familyId = FamilyId.New();
        var senderId = UserId.New();
        var messages = new List<Message>
        {
            Message.Create(familyId, senderId, MessageContent.From("Message 1"), utcNow: DateTimeOffset.UtcNow),
            Message.Create(familyId, senderId, MessageContent.From("Message 2"), utcNow: DateTimeOffset.UtcNow)
        };
        var user = CreateTestUser(senderId);
        var (handler, messageRepo, userRepo) = CreateHandler();

        // Configure: return messages in DESC order (matching real repo behavior)
        var orderedMessages = messages.OrderByDescending(m => m.SentAt).ToList();
        messageRepo.GetByFamilyAsync(familyId, 50, null, Arg.Any<CancellationToken>())
            .Returns(orderedMessages);
        userRepo.GetByIdAsync(senderId, Arg.Any<CancellationToken>())
            .Returns(user);

        var query = new GetFamilyMessagesQuery() { FamilyId = familyId, UserId = senderId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Content.Should().Be("Message 2"); // DESC order
        result[1].Content.Should().Be("Message 1");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoMessages()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, messageRepo, _) = CreateHandler();

        messageRepo.GetByFamilyAsync(familyId, 50, null, Arg.Any<CancellationToken>())
            .Returns(new List<Message>());

        var query = new GetFamilyMessagesQuery() { FamilyId = familyId, UserId = userId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldResolverSenderName()
    {
        // Arrange
        var familyId = FamilyId.New();
        var senderId = UserId.New();
        var messages = new List<Message>
        {
            Message.Create(familyId, senderId, MessageContent.From("Hello!"), utcNow: DateTimeOffset.UtcNow)
        };
        var user = CreateTestUser(senderId);
        var (handler, messageRepo, userRepo) = CreateHandler();

        messageRepo.GetByFamilyAsync(familyId, 50, null, Arg.Any<CancellationToken>())
            .Returns(messages);
        userRepo.GetByIdAsync(senderId, Arg.Any<CancellationToken>())
            .Returns(user);

        var query = new GetFamilyMessagesQuery() { FamilyId = familyId, UserId = senderId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].SenderName.Should().Be("Test User");
    }

    [Fact]
    public async Task Handle_ShouldRespectLimit()
    {
        // Arrange
        var familyId = FamilyId.New();
        var senderId = UserId.New();
        var messages = Enumerable.Range(1, 10)
            .Select(i => Message.Create(familyId, senderId, MessageContent.From($"Message {i}"), utcNow: DateTimeOffset.UtcNow))
            .ToList();
        var user = CreateTestUser(senderId);
        var (handler, messageRepo, userRepo) = CreateHandler();

        // Return only 3 messages (simulating limit applied by repo)
        var limitedMessages = messages.OrderByDescending(m => m.SentAt).Take(3).ToList();
        messageRepo.GetByFamilyAsync(familyId, 3, null, Arg.Any<CancellationToken>())
            .Returns(limitedMessages);
        userRepo.GetByIdAsync(senderId, Arg.Any<CancellationToken>())
            .Returns(user);

        var query = new GetFamilyMessagesQuery(Limit: 3) { FamilyId = familyId, UserId = senderId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    // --- Helpers ---

    private static User CreateTestUser(UserId? id = null)
    {
        var user = User.Register(
            Email.From("test@example.com"),
            UserName.From("Test User"),
            ExternalUserId.From("test-external-id"),
            emailVerified: true);
        user.ClearDomainEvents();
        return user;
    }

    private static (GetFamilyMessagesQueryHandler Handler, IMessageRepository MessageRepo, IUserRepository UserRepo) CreateHandler()
    {
        var messageRepo = Substitute.For<IMessageRepository>();
        var userRepo = Substitute.For<IUserRepository>();
        var handler = new GetFamilyMessagesQueryHandler(messageRepo, userRepo);
        return (handler, messageRepo, userRepo);
    }
}
