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

namespace FamilyHub.Tests.Unit.Auth.Presentation.GraphQL.DataLoaders;

/// <summary>
/// Unit tests for UsersByFamilyGroupedDataLoader.
/// Verifies 1:N relationship loading, batching, and proper grouping.
/// </summary>
public sealed class UsersByFamilyGroupedDataLoaderTests : IDisposable
{
    private readonly DataLoaderTestFixture<AuthDbContext> _fixture = new();
    private readonly IBatchScheduler _batchScheduler = new AutoBatchScheduler();
    private readonly DataLoaderOptions _options = new();

    #region Happy Path Tests

    [Fact]
    public async Task LoadGroupedBatchAsync_WithSingleFamily_ShouldReturnUsersForFamily()
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
        var sut = new UsersByFamilyGroupedDataLoader(factory, _batchScheduler, _options);

        // Act
        var result = await sut.LoadAsync(familyId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Email == user1.Email);
        result.Should().Contain(u => u.Email == user2.Email);
    }

    [Fact]
    public async Task LoadGroupedBatchAsync_WithMultipleFamilies_ShouldGroupUsersCorrectly()
    {
        // Arrange
        var familyId1 = FamilyId.New();
        var familyId2 = FamilyId.New();

        var user1 = new UserBuilder()
            .WithEmail("family1-user@example.com")
            .WithFamilyId(familyId1)
            .Build();
        var user2 = new UserBuilder()
            .WithEmail("family2-user@example.com")
            .WithFamilyId(familyId2)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Users.AddRange(user1, user2);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new UsersByFamilyGroupedDataLoader(factory, _batchScheduler, _options);

        // Act
        var results = await Task.WhenAll(
            sut.LoadAsync(familyId1, CancellationToken.None),
            sut.LoadAsync(familyId2, CancellationToken.None));

        // Assert
        results[0].Should().HaveCount(1);
        results[0]!.Single().Email.Should().Be(user1.Email);

        results[1].Should().HaveCount(1);
        results[1]!.Single().Email.Should().Be(user2.Email);
    }

    #endregion

    #region Batching Behavior Tests

    [Fact]
    public async Task LoadGroupedBatchAsync_WithMultipleKeys_ShouldQueryDatabaseOnce()
    {
        // Arrange
        var familyId1 = FamilyId.New();
        var familyId2 = FamilyId.New();

        var user1 = new UserBuilder()
            .WithEmail("batch1@example.com")
            .WithFamilyId(familyId1)
            .Build();
        var user2 = new UserBuilder()
            .WithEmail("batch2@example.com")
            .WithFamilyId(familyId2)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Users.AddRange(user1, user2);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactoryWithCallTracking(out var callCount);
        var sut = new UsersByFamilyGroupedDataLoader(factory, _batchScheduler, _options);

        // Act - Load multiple keys which should be batched
        var task1 = sut.LoadAsync(familyId1, CancellationToken.None);
        var task2 = sut.LoadAsync(familyId2, CancellationToken.None);
        await Task.WhenAll(task1, task2);

        // Assert - Should be batched into a single database call
        callCount[0].Should().Be(1, "GroupedDataLoader should batch all keys into a single query");
    }

    #endregion

    #region Caching Behavior Tests

    [Fact]
    public async Task LoadAsync_WithSameKeyTwice_ShouldReturnCachedResult()
    {
        // Arrange
        var familyId = FamilyId.New();
        var user = new UserBuilder()
            .WithEmail("cache@example.com")
            .WithFamilyId(familyId)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new UsersByFamilyGroupedDataLoader(factory, _batchScheduler, _options);

        // Act - Get two tasks for the same key
        var task1 = sut.LoadAsync(familyId, CancellationToken.None);
        var task2 = sut.LoadAsync(familyId, CancellationToken.None);

        var result1 = await task1;
        var result2 = await task2;

        // Assert - GreenDonut caches at the Promise level, so same key returns same result
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Should().BeEquivalentTo(result2, "Same key should return cached result");
    }

    #endregion

    #region Empty Results Tests

    [Fact]
    public async Task LoadGroupedBatchAsync_WithFamilyHavingNoUsers_ShouldReturnEmptyForThatKey()
    {
        // Arrange
        var familyIdWithUsers = FamilyId.New();
        var familyIdWithoutUsers = FamilyId.New();

        var user = new UserBuilder()
            .WithEmail("user@example.com")
            .WithFamilyId(familyIdWithUsers)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new UsersByFamilyGroupedDataLoader(factory, _batchScheduler, _options);

        // Act
        var results = await Task.WhenAll(
            sut.LoadAsync(familyIdWithUsers, CancellationToken.None),
            sut.LoadAsync(familyIdWithoutUsers, CancellationToken.None));

        // Assert
        results[0].Should().HaveCount(1);
        results[1].Should().BeEmpty();
    }

    [Fact]
    public async Task LoadGroupedBatchAsync_WhenNoUsersExist_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        var factory = _fixture.CreateMockFactory();
        var sut = new UsersByFamilyGroupedDataLoader(factory, _batchScheduler, _options);
        var nonExistentFamilyId = FamilyId.New();

        // Act
        var result = await sut.LoadAsync(nonExistentFamilyId, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task LoadGroupedBatchAsync_WhenDbContextFactoryFails_ShouldPropagateException()
    {
        // Arrange
        var factory = Substitute.For<IDbContextFactory<AuthDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var sut = new UsersByFamilyGroupedDataLoader(factory, _batchScheduler, _options);

        // Act
        var act = () => sut.LoadAsync(FamilyId.New(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection failed");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task LoadGroupedBatchAsync_ShouldPassCancellationTokenToDbContextFactory()
    {
        // Arrange
        var familyId = FamilyId.New();
        var user = new UserBuilder()
            .WithEmail("token@example.com")
            .WithFamilyId(familyId)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
        });

        var factory = Substitute.For<IDbContextFactory<AuthDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(_fixture.CreateDbContext()));

        var sut = new UsersByFamilyGroupedDataLoader(factory, _batchScheduler, _options);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await sut.LoadAsync(familyId, token);

        // Assert
        await factory.Received(1).CreateDbContextAsync(token);
    }

    #endregion

    public void Dispose() => _fixture.Dispose();
}
