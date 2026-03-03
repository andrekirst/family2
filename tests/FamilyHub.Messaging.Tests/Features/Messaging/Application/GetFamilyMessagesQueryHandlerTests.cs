using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Application.Queries.GetFamilyMessages;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

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
            Message.Create(familyId, senderId, MessageContent.From("Message 1")),
            Message.Create(familyId, senderId, MessageContent.From("Message 2"))
        };
        var user = CreateTestUser(senderId);
        var (handler, _, _) = CreateHandler(messages, user);
        var query = new GetFamilyMessagesQuery(familyId);

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
        var user = CreateTestUser();
        var (handler, _, _) = CreateHandler([], user);
        var query = new GetFamilyMessagesQuery(FamilyId.New());

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
            Message.Create(familyId, senderId, MessageContent.From("Hello!"))
        };
        var user = CreateTestUser(senderId);
        var (handler, _, _) = CreateHandler(messages, user);
        var query = new GetFamilyMessagesQuery(familyId);

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
            .Select(i => Message.Create(familyId, senderId, MessageContent.From($"Message {i}")))
            .ToList();
        var user = CreateTestUser(senderId);
        var (handler, _, _) = CreateHandler(messages, user);
        var query = new GetFamilyMessagesQuery(familyId, Limit: 3);

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

    private static (GetFamilyMessagesQueryHandler Handler, FakeMessageRepository MessageRepo, FakeUserRepository UserRepo) CreateHandler(
        List<Message> messages,
        User? user)
    {
        var messageRepo = new FakeMessageRepository(messages);
        var userRepo = new FakeUserRepository(user);
        var handler = new GetFamilyMessagesQueryHandler(messageRepo, userRepo);
        return (handler, messageRepo, userRepo);
    }
}
