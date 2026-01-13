using FamilyHub.Modules.Family.Persistence;
using FamilyHub.Modules.Family.Presentation.GraphQL.DataLoaders;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Unit.Builders;
using FamilyHub.Tests.Unit.Fixtures;
using FluentAssertions;
using GreenDonut;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;
using ManualBatchScheduler = FamilyHub.Tests.Unit.Fixtures.ManualBatchScheduler;

namespace FamilyHub.Tests.Unit.Family.Presentation.GraphQL.DataLoaders;

/// <summary>
/// Unit tests for FamilyBatchDataLoader.
/// Verifies batching behavior, caching, and proper DbContext factory usage.
/// </summary>
public sealed class FamilyBatchDataLoaderTests : IDisposable
{
    private readonly DataLoaderTestFixture<FamilyDbContext> _fixture;
    private readonly IBatchScheduler _batchScheduler;
    private readonly DataLoaderOptions _options;

    public FamilyBatchDataLoaderTests()
    {
        _fixture = new DataLoaderTestFixture<FamilyDbContext>();
        _batchScheduler = new AutoBatchScheduler();
        _options = new DataLoaderOptions();
    }

    #region Happy Path Tests

    [Fact]
    public async Task LoadBatchAsync_WithSingleKey_ShouldReturnFamily()
    {
        // Arrange
        var family = new FamilyBuilder()
            .WithName(FamilyName.From("Smith Family"))
            .WithOwnerId(UserId.New())
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Families.Add(family);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new FamilyBatchDataLoader(factory, _batchScheduler, _options);

        // Act
        var result = await sut.LoadAsync(family.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(family.Id);
        result.Name.Should().Be(family.Name);
    }

    [Fact]
    public async Task LoadBatchAsync_WithMultipleKeys_ShouldReturnAllFamilies()
    {
        // Arrange
        var family1 = new FamilyBuilder()
            .WithName(FamilyName.From("Smith Family"))
            .Build();
        var family2 = new FamilyBuilder()
            .WithName(FamilyName.From("Johnson Family"))
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Families.AddRange(family1, family2);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new FamilyBatchDataLoader(factory, _batchScheduler, _options);

        // Act
        var results = await Task.WhenAll(
            sut.LoadAsync(family1.Id, CancellationToken.None),
            sut.LoadAsync(family2.Id, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results[0]!.Id.Should().Be(family1.Id);
        results[1]!.Id.Should().Be(family2.Id);
    }

    #endregion

    #region Batching Behavior Tests

    [Fact]
    public async Task LoadBatchAsync_WithMultipleKeys_ShouldQueryDatabaseOnce()
    {
        // Arrange
        var family1 = new FamilyBuilder().Build();
        var family2 = new FamilyBuilder().Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Families.AddRange(family1, family2);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactoryWithCallTracking(out var callCount);

        // Use ManualBatchScheduler for deterministic batching control
        // AutoBatchScheduler timing is non-deterministic and causes flaky tests in CI
        var manualScheduler = new ManualBatchScheduler();
        var sut = new FamilyBatchDataLoader(factory, manualScheduler, _options);

        // Act - Queue multiple keys for batching
        var task1 = sut.LoadAsync(family1.Id, CancellationToken.None);
        var task2 = sut.LoadAsync(family2.Id, CancellationToken.None);

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
        var family = new FamilyBuilder().Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Families.Add(family);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();

        // Use ManualBatchScheduler to ensure both requests are queued before dispatch
        // AutoBatchScheduler timing can cause first request to complete before second is queued
        var manualScheduler = new ManualBatchScheduler();
        var sut = new FamilyBatchDataLoader(factory, manualScheduler, _options);

        // Act - Queue both requests for the same key before dispatching
        var task1 = sut.LoadAsync(family.Id, CancellationToken.None);
        var task2 = sut.LoadAsync(family.Id, CancellationToken.None);

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
        var family1 = new FamilyBuilder()
            .WithName(FamilyName.From("Family One"))
            .Build();
        var family2 = new FamilyBuilder()
            .WithName(FamilyName.From("Family Two"))
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Families.AddRange(family1, family2);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new FamilyBatchDataLoader(factory, _batchScheduler, _options);

        // Act
        var result1 = await sut.LoadAsync(family1.Id, CancellationToken.None);
        var result2 = await sut.LoadAsync(family2.Id, CancellationToken.None);

        // Assert
        result1.Should().NotBe(result2);
        result1!.Name.Should().Be(FamilyName.From("Family One"));
        result2!.Name.Should().Be(FamilyName.From("Family Two"));
    }

    #endregion

    #region Partial Results Tests

    [Fact]
    public async Task LoadBatchAsync_WhenSomeKeysNotFound_ShouldReturnOnlyFoundFamilies()
    {
        // Arrange
        var existingFamily = new FamilyBuilder().Build();
        var nonExistentId = FamilyId.New();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Families.Add(existingFamily);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new FamilyBatchDataLoader(factory, _batchScheduler, _options);

        // Act
        var existingResult = await sut.LoadAsync(existingFamily.Id, CancellationToken.None);
        var nonExistentResult = await sut.LoadAsync(nonExistentId, CancellationToken.None);

        // Assert
        existingResult.Should().NotBeNull();
        existingResult!.Id.Should().Be(existingFamily.Id);
        nonExistentResult.Should().BeNull();
    }

    #endregion

    #region Empty/Null Cases Tests

    [Fact]
    public async Task LoadBatchAsync_WhenNoFamiliesExist_ShouldReturnNull()
    {
        // Arrange
        var factory = _fixture.CreateMockFactory();
        var sut = new FamilyBatchDataLoader(factory, _batchScheduler, _options);
        var nonExistentId = FamilyId.New();

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
        var factory = Substitute.For<IDbContextFactory<FamilyDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var sut = new FamilyBatchDataLoader(factory, _batchScheduler, _options);

        // Act
        var act = () => sut.LoadAsync(FamilyId.New(), CancellationToken.None);

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
        var family = new FamilyBuilder().Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.Families.Add(family);
            await ctx.SaveChangesAsync();
        });

        var factory = Substitute.For<IDbContextFactory<FamilyDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(_fixture.CreateDbContext()));

        var sut = new FamilyBatchDataLoader(factory, _batchScheduler, _options);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await sut.LoadAsync(family.Id, token);

        // Assert
        await factory.Received(1).CreateDbContextAsync(token);
    }

    [Fact]
    public async Task LoadBatchAsync_WhenCanceled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var factory = Substitute.For<IDbContextFactory<FamilyDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var token = call.Arg<CancellationToken>();
                token.ThrowIfCancellationRequested();
                return Task.FromResult(_fixture.CreateDbContext());
            });

        var sut = new FamilyBatchDataLoader(factory, _batchScheduler, _options);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = () => sut.LoadAsync(FamilyId.New(), cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    public void Dispose() => _fixture.Dispose();
}
