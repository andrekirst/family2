using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Auth.Presentation.GraphQL.DataLoaders;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Unit.Builders;
using FamilyHub.Tests.Unit.Fixtures;
using FluentAssertions;
using GreenDonut;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ManualBatchScheduler = FamilyHub.Tests.Unit.Fixtures.ManualBatchScheduler;

namespace FamilyHub.Tests.Unit.Auth.Presentation.GraphQL.DataLoaders;

/// <summary>
/// Unit tests for UserBatchDataLoader.
/// Verifies batching behavior, caching, and proper DbContext factory usage.
/// </summary>
public sealed class UserBatchDataLoaderTests : IDisposable
{
    private readonly DataLoaderTestFixture<AuthDbContext> _fixture = new();
    private readonly IBatchScheduler _batchScheduler = new AutoBatchScheduler();
    private readonly DataLoaderOptions _options = new();

    #region Happy Path Tests

    [Fact]
    public async Task LoadBatchAsync_WithSingleKey_ShouldReturnUser()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("john@example.com")
            .WithFamilyId(FamilyId.New())
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new UserBatchDataLoader(factory, _batchScheduler, _options);

        // Act
        var result = await sut.LoadAsync(user.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task LoadBatchAsync_WithMultipleKeys_ShouldReturnAllUsers()
    {
        // Arrange
        var familyId = FamilyId.New();
        var user1 = new UserBuilder()
            .WithEmail("user1@example.com")
            .WithFamilyId(familyId)
            .Build();
        var user2 = new UserBuilder()
            .WithEmail("user2@example.com")
            .WithFamilyId(familyId)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Users.AddRange(user1, user2);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new UserBatchDataLoader(factory, _batchScheduler, _options);

        // Act
        var results = await Task.WhenAll(
            sut.LoadAsync(user1.Id, CancellationToken.None),
            sut.LoadAsync(user2.Id, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results[0]!.Id.Should().Be(user1.Id);
        results[1]!.Id.Should().Be(user2.Id);
    }

    #endregion

    #region Batching Behavior Tests

    [Fact]
    public async Task LoadBatchAsync_WithMultipleKeys_ShouldQueryDatabaseOnce()
    {
        // Arrange
        var familyId = FamilyId.New();
        var user1 = new UserBuilder()
            .WithEmail("batch1@example.com")
            .WithFamilyId(familyId)
            .Build();
        var user2 = new UserBuilder()
            .WithEmail("batch2@example.com")
            .WithFamilyId(familyId)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Users.AddRange(user1, user2);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactoryWithCallTracking(out var callCount);

        // Use ManualBatchScheduler for deterministic batching control
        // AutoBatchScheduler timing is non-deterministic and causes flaky tests in CI
        var manualScheduler = new ManualBatchScheduler();
        var sut = new UserBatchDataLoader(factory, manualScheduler, _options);

        // Act - Queue multiple keys for batching
        var task1 = sut.LoadAsync(user1.Id, CancellationToken.None);
        var task2 = sut.LoadAsync(user2.Id, CancellationToken.None);

        // Explicitly dispatch the batch - this ensures all queued keys are processed together
        await manualScheduler.DispatchAsync();
        await Task.WhenAll(task1, task2);

        // Assert - Should be batched into a single database call
        callCount[0].Should().Be(1, "DataLoader should batch all keys into a single query");
    }

    #endregion

    #region Caching Behavior Tests

    [Fact]
    public async Task LoadAsync_WithSameKeyTwice_ShouldReturnCachedResult()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("cache@example.com")
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();

        // Use ManualBatchScheduler to ensure both requests are queued before dispatch
        // AutoBatchScheduler timing can cause first request to complete before second is queued
        var manualScheduler = new ManualBatchScheduler();
        var sut = new UserBatchDataLoader(factory, manualScheduler, _options);

        // Act - Queue both requests for the same key before dispatching
        var task1 = sut.LoadAsync(user.Id, CancellationToken.None);
        var task2 = sut.LoadAsync(user.Id, CancellationToken.None);

        // Dispatch the batch - both requests will be resolved from the same Promise
        await manualScheduler.DispatchAsync();

        var result1 = await task1;
        var result2 = await task2;

        // Assert - GreenDonut caches at the Promise level, so same key returns same result
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        ReferenceEquals(result1, result2).Should().BeTrue("Same key should return cached result");
    }

    [Fact]
    public async Task LoadAsync_WithDifferentKeys_ShouldNotShareCache()
    {
        // Arrange
        var familyId = FamilyId.New();
        var user1 = new UserBuilder()
            .WithEmail("user1@example.com")
            .WithFamilyId(familyId)
            .Build();
        var user2 = new UserBuilder()
            .WithEmail("user2@example.com")
            .WithFamilyId(familyId)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Users.AddRange(user1, user2);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new UserBatchDataLoader(factory, _batchScheduler, _options);

        // Act
        var result1 = await sut.LoadAsync(user1.Id, CancellationToken.None);
        var result2 = await sut.LoadAsync(user2.Id, CancellationToken.None);

        // Assert
        result1.Should().NotBe(result2);
        result1.Email.Should().Be(Email.From("user1@example.com"));
        result2!.Email.Should().Be(Email.From("user2@example.com"));
    }

    #endregion

    #region Partial Results Tests

    [Fact]
    public async Task LoadBatchAsync_WhenSomeKeysNotFound_ShouldReturnOnlyFoundUsers()
    {
        // Arrange
        var existingUser = new UserBuilder()
            .WithEmail("existing@example.com")
            .Build();
        var nonExistentId = UserId.New();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Users.Add(existingUser);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new UserBatchDataLoader(factory, _batchScheduler, _options);

        // Act
        var existingResult = await sut.LoadAsync(existingUser.Id, CancellationToken.None);
        var nonExistentResult = await sut.LoadAsync(nonExistentId, CancellationToken.None);

        // Assert
        existingResult.Should().NotBeNull();
        existingResult.Id.Should().Be(existingUser.Id);
        nonExistentResult.Should().BeNull();
    }

    #endregion

    #region Empty/Null Cases Tests

    [Fact]
    public async Task LoadBatchAsync_WhenNoUsersExist_ShouldReturnNull()
    {
        // Arrange
        var factory = _fixture.CreateMockFactory();
        var sut = new UserBatchDataLoader(factory, _batchScheduler, _options);
        var nonExistentId = UserId.New();

        // Act
        var result = await sut.LoadAsync(nonExistentId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task LoadBatchAsync_WhenDbContextFactoryFails_ShouldPropagateException()
    {
        // Arrange
        var factory = Substitute.For<IDbContextFactory<AuthDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var sut = new UserBatchDataLoader(factory, _batchScheduler, _options);

        // Act
        var act = () => sut.LoadAsync(UserId.New(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection failed");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task LoadBatchAsync_ShouldPassCancellationTokenToDbContextFactory()
    {
        // Arrange
        var user = new UserBuilder().Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
        });

        var factory = Substitute.For<IDbContextFactory<AuthDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(_fixture.CreateDbContext()));

        var sut = new UserBatchDataLoader(factory, _batchScheduler, _options);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await sut.LoadAsync(user.Id, token);

        // Assert
        await factory.Received(1).CreateDbContextAsync(token);
    }

    [Fact]
    public async Task LoadBatchAsync_WhenCanceled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var factory = Substitute.For<IDbContextFactory<AuthDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var token = call.Arg<CancellationToken>();
                token.ThrowIfCancellationRequested();
                return Task.FromResult(_fixture.CreateDbContext());
            });

        var sut = new UserBatchDataLoader(factory, _batchScheduler, _options);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = () => sut.LoadAsync(UserId.New(), cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    public void Dispose() => _fixture.Dispose();
}
