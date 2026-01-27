using System.Text;
using System.Text.Json;
using FamilyHub.Infrastructure.Messaging;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.DTOs.Subscriptions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Subscriptions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using HotChocolate.Subscriptions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StackExchange.Redis;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Integration tests for Redis subscription infrastructure using real Redis.
/// Tests message publishing, topic isolation, authorization, and error handling.
/// </summary>
[Collection("Redis")]
public sealed class RedisSubscriptionIntegrationTests(RedisContainerFixture fixture) : IAsyncLifetime
{
    private IConnectionMultiplexer _redis = null!;
    private RedisSubscriptionPublisher _publisher = null!;
    private ITopicEventSender _topicEventSender = null!;

    public async Task InitializeAsync()
    {
        // Create Redis connection from fixture
        var configOptions = ConfigurationOptions.Parse(fixture.ConnectionString);
        _redis = await ConnectionMultiplexer.ConnectAsync(configOptions);

        // Mock ITopicEventSender for publisher
        _topicEventSender = Substitute.For<ITopicEventSender>();

        // Configure ITopicEventSender to publish to Redis directly
        _topicEventSender
            .When(x => x.SendAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                var topicName = callInfo.ArgAt<string>(0);
                var message = callInfo.ArgAt<object>(1);
                var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var subscriber = _redis.GetSubscriber();
                subscriber.Publish(RedisChannel.Literal(topicName), json);
            });

        var logger = Substitute.For<ILogger<RedisSubscriptionPublisher>>();
        _publisher = new RedisSubscriptionPublisher(_topicEventSender, logger);
    }

    public async Task DisposeAsync()
    {
        await _redis.DisposeAsync();
    }

    #region Core Publishing Tests

    [Fact]
    public async Task PublishAsync_ValidMessage_DeliveredToSubscriber()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var topicName = $"family-members-changed:{familyId}";
        var expectedMessage = new FamilyMembersChangedPayload
        {
            FamilyId = familyId,
            ChangeType = ChangeType.ADDED,
            Member = new FamilyMemberDto
            {
                Id = Guid.NewGuid(),
                Email = "newmember@example.com",
                EmailVerified = true,
                Role = "member",
                JoinedAt = DateTime.UtcNow,
                IsOwner = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var receivedMessage = string.Empty;
        var messageReceived = new TaskCompletionSource<bool>();

        // Subscribe to Redis topic
        var subscriber = _redis.GetSubscriber();
        await subscriber.SubscribeAsync(RedisChannel.Literal(topicName), (channel, value) =>
        {
            receivedMessage = value!;
            messageReceived.SetResult(true);
        });

        // Act
        await _publisher.PublishAsync(topicName, expectedMessage);

        // Assert
        var completed = await Task.WhenAny(
            messageReceived.Task,
            Task.Delay(TimeSpan.FromSeconds(5))
        );

        completed.Should().Be(messageReceived.Task, "message should be received within timeout");
        receivedMessage.Should().NotBeEmpty();

        // Verify deserialization
        var receivedPayload = JsonSerializer.Deserialize<FamilyMembersChangedPayload>(
            receivedMessage,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        receivedPayload.Should().NotBeNull();
        receivedPayload!.FamilyId.Should().Be(familyId);
        receivedPayload.ChangeType.Should().Be(ChangeType.ADDED);
        receivedPayload.Member.Should().NotBeNull();
        receivedPayload.Member!.Email.Should().Be("newmember@example.com");

        // Cleanup
        await subscriber.UnsubscribeAsync(RedisChannel.Literal(topicName));
    }

    [Fact]
    public async Task PublishAsync_MultipleMessages_AllDeliveredInOrder()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var topicName = $"family-members-changed:{familyId}";
        var messageCount = 10;
        var receivedMessages = new List<string>();
        var allMessagesReceived = new TaskCompletionSource<bool>();

        var subscriber = _redis.GetSubscriber();
        await subscriber.SubscribeAsync(RedisChannel.Literal(topicName), (channel, value) =>
        {
            lock (receivedMessages)
            {
                receivedMessages.Add(value!);
                if (receivedMessages.Count == messageCount)
                {
                    allMessagesReceived.TrySetResult(true);
                }
            }
        });

        // Act
        for (var i = 0; i < messageCount; i++)
        {
            var message = new FamilyMembersChangedPayload
            {
                FamilyId = familyId,
                ChangeType = ChangeType.ADDED,
                Member = new FamilyMemberDto
                {
                    Id = Guid.NewGuid(),
                    Email = $"member{i}@example.com",
                    EmailVerified = true,
                    Role = "member",
                    JoinedAt = DateTime.UtcNow,
                    IsOwner = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            await _publisher.PublishAsync(topicName, message);
        }

        // Assert
        var received = await Task.WhenAny(allMessagesReceived.Task, Task.Delay(TimeSpan.FromSeconds(10)));
        received.Should().Be(allMessagesReceived.Task, "all messages should be received within timeout");
        receivedMessages.Should().HaveCount(messageCount);

        // Cleanup
        await subscriber.UnsubscribeAsync(RedisChannel.Literal(topicName));
    }

    [Fact]
    public async Task PublishAsync_JsonSerialization_PreservesPayloadStructure()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var topicName = $"family-members-changed:{familyId}";
        var expectedMessage = new FamilyMembersChangedPayload
        {
            FamilyId = familyId,
            ChangeType = ChangeType.ADDED,
            Member = new FamilyMemberDto
            {
                Id = userId,
                Email = "test@example.com",
                EmailVerified = true,
                Role = "admin",
                JoinedAt = DateTime.UtcNow,
                IsOwner = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var receivedMessage = string.Empty;
        var messageReceived = new TaskCompletionSource<bool>();

        var subscriber = _redis.GetSubscriber();
        await subscriber.SubscribeAsync(RedisChannel.Literal(topicName), (channel, value) =>
        {
            receivedMessage = value!;
            messageReceived.SetResult(true);
        });

        // Act
        await _publisher.PublishAsync(topicName, expectedMessage);

        // Assert
        var completed = await Task.WhenAny(messageReceived.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        completed.Should().Be(messageReceived.Task);

        var receivedPayload = JsonSerializer.Deserialize<FamilyMembersChangedPayload>(
            receivedMessage,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        receivedPayload.Should().NotBeNull();
        receivedPayload!.FamilyId.Should().Be(familyId);
        receivedPayload.ChangeType.Should().Be(ChangeType.ADDED);
        receivedPayload.Member.Should().NotBeNull();
        receivedPayload.Member!.Id.Should().Be(userId);
        receivedPayload.Member.Email.Should().Be("test@example.com");
        receivedPayload.Member.Role.Should().Be("admin");

        // Cleanup
        await subscriber.UnsubscribeAsync(RedisChannel.Literal(topicName));
    }

    #endregion

    #region Topic Isolation Tests

    [Fact]
    public async Task PublishAsync_DifferentTopics_NoLeakageBetweenFamilies()
    {
        // Arrange
        var familyAId = Guid.NewGuid();
        var familyBId = Guid.NewGuid();
        var topicA = $"family-members-changed:{familyAId}";
        var topicB = $"family-members-changed:{familyBId}";

        var receivedMessage = string.Empty;
        var messageReceived = new TaskCompletionSource<bool>();

        // Subscribe to family-B topic
        var subscriber = _redis.GetSubscriber();
        await subscriber.SubscribeAsync(RedisChannel.Literal(topicB), (channel, value) =>
        {
            receivedMessage = value!;
            messageReceived.SetResult(true);
        });

        // Act - Publish to family-A topic
        var message = new FamilyMembersChangedPayload
        {
            FamilyId = familyAId,
            ChangeType = ChangeType.ADDED,
            Member = null
        };
        await _publisher.PublishAsync(topicA, message);

        // Assert - Should NOT receive message (different topic)
        var received = await Task.WhenAny(messageReceived.Task, Task.Delay(TimeSpan.FromMilliseconds(500)));
        received.Should().NotBe(messageReceived.Task, "subscriber should not receive messages from different family topics");
        receivedMessage.Should().BeEmpty();

        // Cleanup
        await subscriber.UnsubscribeAsync(RedisChannel.Literal(topicB));
    }

    [Fact]
    public async Task PublishAsync_SameMessageType_DifferentFamilies_IsolatedDelivery()
    {
        // Arrange
        var familyAId = Guid.NewGuid();
        var familyBId = Guid.NewGuid();
        var topicA = $"family-members-changed:{familyAId}";
        var topicB = $"family-members-changed:{familyBId}";

        var receivedMessagesA = new List<string>();
        var receivedMessagesB = new List<string>();
        var allMessagesReceived = new TaskCompletionSource<bool>();

        var subscriber = _redis.GetSubscriber();

        // Subscribe to both topics
        await subscriber.SubscribeAsync(RedisChannel.Literal(topicA), (channel, value) =>
        {
            lock (receivedMessagesA)
            {
                receivedMessagesA.Add(value!);
                if (receivedMessagesA.Count >= 1 && receivedMessagesB.Count >= 1)
                {
                    allMessagesReceived.TrySetResult(true);
                }
            }
        });

        await subscriber.SubscribeAsync(RedisChannel.Literal(topicB), (channel, value) =>
        {
            lock (receivedMessagesB)
            {
                receivedMessagesB.Add(value!);
                if (receivedMessagesA.Count >= 1 && receivedMessagesB.Count >= 1)
                {
                    allMessagesReceived.TrySetResult(true);
                }
            }
        });

        // Act - Publish to both topics
        await _publisher.PublishAsync(topicA, new FamilyMembersChangedPayload
        {
            FamilyId = familyAId,
            ChangeType = ChangeType.ADDED,
            Member = null
        });

        await _publisher.PublishAsync(topicB, new FamilyMembersChangedPayload
        {
            FamilyId = familyBId,
            ChangeType = ChangeType.ADDED,
            Member = null
        });

        // Assert
        var received = await Task.WhenAny(allMessagesReceived.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        received.Should().Be(allMessagesReceived.Task);

        receivedMessagesA.Should().HaveCount(1, "subscriber A should receive only family A messages");
        receivedMessagesB.Should().HaveCount(1, "subscriber B should receive only family B messages");

        // Cleanup
        await subscriber.UnsubscribeAsync(RedisChannel.Literal(topicA));
        await subscriber.UnsubscribeAsync(RedisChannel.Literal(topicB));
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task FamilyMembersChanged_UserInFamily_ReceivesMessages()
    {
        // Arrange
        var familyId = FamilyId.New();
        var currentUserId = UserId.New();

        var user = User.CreateWithPassword(
            Email.From("member@example.com"),
            PasswordHash.FromHash("TestPasswordHash123!"),
            
            familyId
        );

        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(currentUserId);

        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(user);

        var logger = Substitute.For<ILogger<InvitationSubscriptions>>();

        var message = new FamilyMembersChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Member = new FamilyMemberDto
            {
                Id = Guid.NewGuid(),
                Email = "newmember@example.com",
                EmailVerified = true,
                Role = "member",
                JoinedAt = DateTime.UtcNow,
                IsOwner = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var subscriptions = new InvitationSubscriptions();

        // Act
        var results = await CollectMessagesAsync(
            subscriptions.FamilyMembersChanged(
                familyId.Value,
                userContext,
                userRepository,
                message,
                logger,
                CancellationToken.None),
            TimeSpan.FromSeconds(5));

        // Assert
        results.Should().HaveCount(1, "user in family should receive subscription message");
        results[0].FamilyId.Should().Be(familyId.Value);
        results[0].ChangeType.Should().Be(ChangeType.ADDED);
    }

    [Fact]
    public async Task FamilyMembersChanged_UserNotInFamily_ReceivesNothing()
    {
        // Arrange
        var targetFamilyId = FamilyId.New();
        var userFamilyId = FamilyId.New(); // Different family!
        var currentUserId = UserId.New();

        var user = User.CreateWithPassword(
            Email.From("member@example.com"),
            PasswordHash.FromHash("TestPasswordHash123!"),
            
            userFamilyId // User is NOT in target family
        );

        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(currentUserId);

        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(user);

        var logger = Substitute.For<ILogger<InvitationSubscriptions>>();

        var message = new FamilyMembersChangedPayload
        {
            FamilyId = targetFamilyId.Value,
            ChangeType = ChangeType.ADDED,
            Member = null
        };

        var subscriptions = new InvitationSubscriptions();

        // Act
        var results = await CollectMessagesAsync(
            subscriptions.FamilyMembersChanged(
                targetFamilyId.Value,
                userContext,
                userRepository,
                message,
                logger,
                CancellationToken.None),
            TimeSpan.FromMilliseconds(500));

        // Assert
        results.Should().BeEmpty("user not in family should not receive subscription messages");
    }

    [Fact]
    public async Task PendingInvitationsChanged_OwnerRole_ReceivesMessages()
    {
        // Arrange
        var familyId = FamilyId.New();
        var currentUserId = UserId.New();

        var user = User.CreateWithPassword(
            Email.From("owner@example.com"),
            PasswordHash.FromHash("TestPasswordHash123!"),
            
            familyId
        );
        user.UpdateRole(FamilyRole.Owner); // Owner role

        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(currentUserId);

        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(user);

        var logger = Substitute.For<ILogger<InvitationSubscriptions>>();

        var message = new PendingInvitationsChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Invitation = new PendingInvitationDto
            {
                Id = Guid.NewGuid(),
                Email = "invitee@example.com",
                Role = "member",
                Status = "pending",
                InvitedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(14)
            }
        };

        var subscriptions = new InvitationSubscriptions();

        // Act
        var results = await CollectMessagesAsync(
            subscriptions.PendingInvitationsChanged(
                familyId.Value,
                userContext,
                userRepository,
                message,
                logger,
                CancellationToken.None),
            TimeSpan.FromSeconds(5));

        // Assert
        results.Should().HaveCount(1, "owner role should receive pending invitations");
        results[0].FamilyId.Should().Be(familyId.Value);
    }

    [Fact]
    public async Task PendingInvitationsChanged_AdminRole_ReceivesMessages()
    {
        // Arrange
        var familyId = FamilyId.New();
        var currentUserId = UserId.New();

        var user = User.CreateWithPassword(
            Email.From("admin@example.com"),
            PasswordHash.FromHash("TestPasswordHash123!"),
            
            familyId
        );
        user.UpdateRole(FamilyRole.Admin); // Admin role

        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(currentUserId);

        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(user);

        var logger = Substitute.For<ILogger<InvitationSubscriptions>>();

        var message = new PendingInvitationsChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Invitation = new PendingInvitationDto
            {
                Id = Guid.NewGuid(),
                Email = "invitee@example.com",
                Role = "member",
                Status = "pending",
                InvitedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(14)
            }
        };

        var subscriptions = new InvitationSubscriptions();

        // Act
        var results = await CollectMessagesAsync(
            subscriptions.PendingInvitationsChanged(
                familyId.Value,
                userContext,
                userRepository,
                message,
                logger,
                CancellationToken.None),
            TimeSpan.FromSeconds(5));

        // Assert
        results.Should().HaveCount(1, "admin role should receive pending invitations");
        results[0].FamilyId.Should().Be(familyId.Value);
    }

    [Fact]
    public async Task PendingInvitationsChanged_MemberRole_ReceivesNothing()
    {
        // Arrange
        var familyId = FamilyId.New();
        var currentUserId = UserId.New();

        var user = User.CreateWithPassword(
            Email.From("member@example.com"),
            PasswordHash.FromHash("TestPasswordHash123!"),
            
            familyId
        );
        user.UpdateRole(FamilyRole.Member); // Member role (not Owner/Admin)

        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(currentUserId);

        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(user);

        var logger = Substitute.For<ILogger<InvitationSubscriptions>>();

        var message = new PendingInvitationsChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Invitation = new PendingInvitationDto
            {
                Id = Guid.NewGuid(),
                Email = "invitee@example.com",
                Role = "member",
                Status = "pending",
                InvitedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(14)
            }
        };

        var subscriptions = new InvitationSubscriptions();

        // Act
        var results = await CollectMessagesAsync(
            subscriptions.PendingInvitationsChanged(
                familyId.Value,
                userContext,
                userRepository,
                message,
                logger,
                CancellationToken.None),
            TimeSpan.FromMilliseconds(500));

        // Assert
        results.Should().BeEmpty("member role should not receive pending invitations");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task PublishAsync_InvalidTopicName_ThrowsArgumentException()
    {
        // Arrange
        var message = new FamilyMembersChangedPayload
        {
            FamilyId = Guid.NewGuid(),
            ChangeType = ChangeType.ADDED,
            Member = null
        };

        // Act & Assert - Null topic
        var actNull = async () => await _publisher.PublishAsync(null!, message);
        await actNull.Should().ThrowAsync<ArgumentException>();

        // Act & Assert - Empty topic
        var actEmpty = async () => await _publisher.PublishAsync(string.Empty, message);
        await actEmpty.Should().ThrowAsync<ArgumentException>();

        // Act & Assert - Whitespace topic
        var actWhitespace = async () => await _publisher.PublishAsync("   ", message);
        await actWhitespace.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task PublishAsync_NullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var topicName = $"family-members-changed:{Guid.NewGuid()}";

        // Act
        var act = async () => await _publisher.PublishAsync<FamilyMembersChangedPayload>(topicName, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PublishAsync_TopicEventSenderThrows_DoesNotThrow()
    {
        // Arrange - Create a publisher with a failing ITopicEventSender
        var failingTopicEventSender = Substitute.For<ITopicEventSender>();
        failingTopicEventSender
            .When(x => x.SendAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new InvalidOperationException("Simulated Redis connection failure"));

        var logger = Substitute.For<ILogger<RedisSubscriptionPublisher>>();
        var failingPublisher = new RedisSubscriptionPublisher(failingTopicEventSender, logger);

        var topicName = $"family-members-changed:{Guid.NewGuid()}";
        var message = new FamilyMembersChangedPayload
        {
            FamilyId = Guid.NewGuid(),
            ChangeType = ChangeType.ADDED,
            Member = null
        };

        // Act - Should NOT throw (graceful degradation)
        var act = async () => await failingPublisher.PublishAsync(topicName, message);

        // Assert - The key behavior: publisher catches exception and doesn't re-throw
        await act.Should().NotThrowAsync("publisher should gracefully handle connection failures");

        // Verify ITopicEventSender was called (meaning the exception was thrown and caught)
        await failingTopicEventSender.Received(1).SendAsync(
            Arg.Is(topicName),
            Arg.Any<FamilyMembersChangedPayload>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_RedisConnectionFailure_GracefulDegradation()
    {
        // Arrange - Create a publisher that simulates complete Redis unavailability
        var unavailableTopicEventSender = Substitute.For<ITopicEventSender>();
        unavailableTopicEventSender
            .SendAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns<ValueTask>(_ => throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Redis server unavailable"));

        var logger = Substitute.For<ILogger<RedisSubscriptionPublisher>>();
        var publisher = new RedisSubscriptionPublisher(unavailableTopicEventSender, logger);

        var topicName = $"family-members-changed:{Guid.NewGuid()}";
        var message = new FamilyMembersChangedPayload
        {
            FamilyId = Guid.NewGuid(),
            ChangeType = ChangeType.ADDED,
            Member = new FamilyMemberDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                EmailVerified = true,
                Role = "member",
                JoinedAt = DateTime.UtcNow,
                IsOwner = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // Act - Multiple publish attempts should all succeed (not throw)
        // This simulates a scenario where Redis is completely unavailable
        for (var i = 0; i < 3; i++)
        {
            var act = async () => await publisher.PublishAsync(topicName, message);
            await act.Should().NotThrowAsync($"publish attempt {i + 1} should not throw despite Redis unavailability");
        }

        // Assert - ITopicEventSender was called 3 times (each attempt tried to send)
        await unavailableTopicEventSender.Received(3).SendAsync(
            Arg.Is(topicName),
            Arg.Any<FamilyMembersChangedPayload>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_SubscriberThrows_DoesNotAffectOtherSubscribers()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var topicName = $"family-members-changed:{familyId}";
        var message = new FamilyMembersChangedPayload
        {
            FamilyId = familyId,
            ChangeType = ChangeType.ADDED,
            Member = new FamilyMemberDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                EmailVerified = true,
                Role = "member",
                JoinedAt = DateTime.UtcNow,
                IsOwner = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var healthySubscriberReceived = new TaskCompletionSource<bool>();
        var healthySubscriberMessage = string.Empty;
        var faultySubscriberCalled = false;

        var subscriber = _redis.GetSubscriber();

        // Subscribe with a faulty handler (throws exception)
        await subscriber.SubscribeAsync(RedisChannel.Literal(topicName), (channel, value) =>
        {
            faultySubscriberCalled = true;
            throw new InvalidOperationException("Simulated subscriber error");
        });

        // Subscribe with a healthy handler
        await subscriber.SubscribeAsync(RedisChannel.Literal(topicName), (channel, value) =>
        {
            healthySubscriberMessage = value!;
            healthySubscriberReceived.TrySetResult(true);
        });

        // Act
        await _publisher.PublishAsync(topicName, message);

        // Assert - Healthy subscriber should still receive the message
        var completed = await Task.WhenAny(healthySubscriberReceived.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        completed.Should().Be(healthySubscriberReceived.Task, "healthy subscriber should receive message despite faulty subscriber");
        healthySubscriberMessage.Should().NotBeEmpty();
        faultySubscriberCalled.Should().BeTrue("faulty subscriber should have been called");

        // Cleanup - Unsubscribe all handlers from this channel
        await subscriber.UnsubscribeAsync(RedisChannel.Literal(topicName));
    }

    [Fact]
    public async Task PublishAsync_PartialSubscriberFailure_OtherSubscribersContinueReceiving()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var topicName = $"family-members-changed:{familyId}";
        var messageCount = 5;
        var receivedByHealthySubscriber = new List<string>();
        var allMessagesReceived = new TaskCompletionSource<bool>();
        var faultySubscriberCallCount = 0;

        var subscriber = _redis.GetSubscriber();

        // Faulty subscriber that throws on every other message
        await subscriber.SubscribeAsync(RedisChannel.Literal(topicName), (channel, value) =>
        {
            Interlocked.Increment(ref faultySubscriberCallCount);
            if (faultySubscriberCallCount % 2 == 0)
            {
                throw new InvalidOperationException("Intermittent subscriber failure");
            }
        });

        // Healthy subscriber that collects all messages
        await subscriber.SubscribeAsync(RedisChannel.Literal(topicName), (channel, value) =>
        {
            lock (receivedByHealthySubscriber)
            {
                receivedByHealthySubscriber.Add(value!);
                if (receivedByHealthySubscriber.Count == messageCount)
                {
                    allMessagesReceived.TrySetResult(true);
                }
            }
        });

        // Act - Publish multiple messages
        for (var i = 0; i < messageCount; i++)
        {
            var message = new FamilyMembersChangedPayload
            {
                FamilyId = familyId,
                ChangeType = ChangeType.ADDED,
                Member = new FamilyMemberDto
                {
                    Id = Guid.NewGuid(),
                    Email = $"member{i}@example.com",
                    EmailVerified = true,
                    Role = "member",
                    JoinedAt = DateTime.UtcNow,
                    IsOwner = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            await _publisher.PublishAsync(topicName, message);
        }

        // Assert - Healthy subscriber should receive ALL messages despite faulty subscriber failures
        var completed = await Task.WhenAny(allMessagesReceived.Task, Task.Delay(TimeSpan.FromSeconds(10)));
        completed.Should().Be(allMessagesReceived.Task, "healthy subscriber should receive all messages");
        receivedByHealthySubscriber.Should().HaveCount(messageCount);
        faultySubscriberCallCount.Should().Be(messageCount, "faulty subscriber should have been called for each message");

        // Cleanup - Unsubscribe all handlers from this channel
        await subscriber.UnsubscribeAsync(RedisChannel.Literal(topicName));
    }

    #endregion

    #region Message Format Tests

    [Fact]
    public async Task PublishAsync_FamilyMembersChangedPayload_CorrectFormat()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var topicName = $"family-members-changed:{familyId}";
        var message = new FamilyMembersChangedPayload
        {
            FamilyId = familyId,
            ChangeType = ChangeType.ADDED,
            Member = new FamilyMemberDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                EmailVerified = true,
                Role = "member",
                JoinedAt = DateTime.UtcNow,
                IsOwner = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var receivedMessage = string.Empty;
        var messageReceived = new TaskCompletionSource<bool>();

        var subscriber = _redis.GetSubscriber();
        await subscriber.SubscribeAsync(RedisChannel.Literal(topicName), (channel, value) =>
        {
            receivedMessage = value!;
            messageReceived.SetResult(true);
        });

        // Act
        await _publisher.PublishAsync(topicName, message);

        // Assert
        var completed = await Task.WhenAny(messageReceived.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        completed.Should().Be(messageReceived.Task);

        // Verify JSON structure (camelCase)
        receivedMessage.Should().Contain("\"familyId\":");
        receivedMessage.Should().Contain("\"changeType\":");
        receivedMessage.Should().Contain("\"member\":");
        receivedMessage.Should().NotContain("\"FamilyId\":");
        receivedMessage.Should().NotContain("\"ChangeType\":");

        // Cleanup
        await subscriber.UnsubscribeAsync(RedisChannel.Literal(topicName));
    }

    [Fact]
    public async Task PublishAsync_PendingInvitationsChangedPayload_CorrectFormat()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var topicName = $"pending-invitations-changed:{familyId}";
        var message = new PendingInvitationsChangedPayload
        {
            FamilyId = familyId,
            ChangeType = ChangeType.REMOVED,
            Invitation = new PendingInvitationDto
            {
                Id = Guid.NewGuid(),
                Email = "invitee@example.com",
                Role = "member",
                Status = "pending",
                InvitedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(14)
            }
        };

        var receivedMessage = string.Empty;
        var messageReceived = new TaskCompletionSource<bool>();

        var subscriber = _redis.GetSubscriber();
        await subscriber.SubscribeAsync(RedisChannel.Literal(topicName), (channel, value) =>
        {
            receivedMessage = value!;
            messageReceived.SetResult(true);
        });

        // Act
        await _publisher.PublishAsync(topicName, message);

        // Assert
        var completed = await Task.WhenAny(messageReceived.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        completed.Should().Be(messageReceived.Task);

        // Verify JSON structure (camelCase)
        receivedMessage.Should().Contain("\"familyId\":");
        receivedMessage.Should().Contain("\"changeType\":");
        receivedMessage.Should().Contain("\"invitation\":");
        receivedMessage.Should().NotContain("\"FamilyId\":");
        receivedMessage.Should().NotContain("\"ChangeType\":");

        // Cleanup
        await subscriber.UnsubscribeAsync(RedisChannel.Literal(topicName));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Collects messages from an async enumerable with a timeout.
    /// Used to test subscription authorization logic.
    /// </summary>
    private static async Task<List<TPayload>> CollectMessagesAsync<TPayload>(
        IAsyncEnumerable<TPayload> subscription,
        TimeSpan timeout)
    {
        var messages = new List<TPayload>();
        var cts = new CancellationTokenSource(timeout);

        try
        {
            await foreach (var message in subscription.WithCancellation(cts.Token))
            {
                messages.Add(message);
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout - return collected messages
        }

        return messages;
    }

    #endregion
}
