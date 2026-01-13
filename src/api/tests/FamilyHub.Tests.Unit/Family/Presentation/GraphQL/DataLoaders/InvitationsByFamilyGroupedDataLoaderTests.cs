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

namespace FamilyHub.Tests.Unit.Family.Presentation.GraphQL.DataLoaders;

/// <summary>
/// Unit tests for InvitationsByFamilyGroupedDataLoader.
/// Verifies 1:N relationship loading, batching, and proper grouping.
/// </summary>
public sealed class InvitationsByFamilyGroupedDataLoaderTests : IDisposable
{
    private readonly DataLoaderTestFixture<FamilyDbContext> _fixture = new();
    private readonly IBatchScheduler _batchScheduler = new AutoBatchScheduler();
    private readonly DataLoaderOptions _options = new();

    #region Happy Path Tests

    [Fact]
    public async Task LoadGroupedBatchAsync_WithSingleFamily_ShouldReturnInvitationsForFamily()
    {
        // Arrange
        var familyId = FamilyId.New();
        var invitedByUserId = UserId.New();

        var invitation1 = new InvitationBuilder()
            .WithFamilyId(familyId)
            .WithEmail("invite1@example.com")
            .WithRole(FamilyRole.Member)
            .WithInvitedByUserId(invitedByUserId)
            .Build();
        var invitation2 = new InvitationBuilder()
            .WithFamilyId(familyId)
            .WithEmail("invite2@example.com")
            .WithRole(FamilyRole.Admin)
            .WithInvitedByUserId(invitedByUserId)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.FamilyMemberInvitations.AddRange(invitation1, invitation2);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new InvitationsByFamilyGroupedDataLoader(factory, _batchScheduler, _options);

        // Act
        var result = await sut.LoadAsync(familyId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(i => i.Email == invitation1.Email);
        result.Should().Contain(i => i.Email == invitation2.Email);
    }

    [Fact]
    public async Task LoadGroupedBatchAsync_WithMultipleFamilies_ShouldGroupInvitationsCorrectly()
    {
        // Arrange
        var familyId1 = FamilyId.New();
        var familyId2 = FamilyId.New();
        var invitedByUserId = UserId.New();

        var invitation1 = new InvitationBuilder()
            .WithFamilyId(familyId1)
            .WithEmail("family1-invite@example.com")
            .WithInvitedByUserId(invitedByUserId)
            .Build();
        var invitation2 = new InvitationBuilder()
            .WithFamilyId(familyId2)
            .WithEmail("family2-invite@example.com")
            .WithInvitedByUserId(invitedByUserId)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.FamilyMemberInvitations.AddRange(invitation1, invitation2);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new InvitationsByFamilyGroupedDataLoader(factory, _batchScheduler, _options);

        // Act
        var results = await Task.WhenAll(
            sut.LoadAsync(familyId1, CancellationToken.None),
            sut.LoadAsync(familyId2, CancellationToken.None));

        // Assert
        results[0].Should().HaveCount(1);
        results[0]!.Single().Email.Should().Be(invitation1.Email);

        results[1].Should().HaveCount(1);
        results[1]!.Single().Email.Should().Be(invitation2.Email);
    }

    #endregion

    #region Batching Behavior Tests

    [Fact]
    public async Task LoadGroupedBatchAsync_WithMultipleKeys_ShouldQueryDatabaseOnce()
    {
        // Arrange
        var familyId1 = FamilyId.New();
        var familyId2 = FamilyId.New();
        var invitedByUserId = UserId.New();

        var invitation1 = new InvitationBuilder()
            .WithFamilyId(familyId1)
            .WithEmail("batch1@example.com")
            .WithInvitedByUserId(invitedByUserId)
            .Build();
        var invitation2 = new InvitationBuilder()
            .WithFamilyId(familyId2)
            .WithEmail("batch2@example.com")
            .WithInvitedByUserId(invitedByUserId)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.FamilyMemberInvitations.AddRange(invitation1, invitation2);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactoryWithCallTracking(out var callCount);
        var sut = new InvitationsByFamilyGroupedDataLoader(factory, _batchScheduler, _options);

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
        var invitedByUserId = UserId.New();

        var invitation = new InvitationBuilder()
            .WithFamilyId(familyId)
            .WithEmail("cache@example.com")
            .WithInvitedByUserId(invitedByUserId)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.FamilyMemberInvitations.Add(invitation);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new InvitationsByFamilyGroupedDataLoader(factory, _batchScheduler, _options);

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
    public async Task LoadGroupedBatchAsync_WithFamilyHavingNoInvitations_ShouldReturnEmptyForThatKey()
    {
        // Arrange
        var familyIdWithInvitations = FamilyId.New();
        var familyIdWithoutInvitations = FamilyId.New();
        var invitedByUserId = UserId.New();

        var invitation = new InvitationBuilder()
            .WithFamilyId(familyIdWithInvitations)
            .WithEmail("invite@example.com")
            .WithInvitedByUserId(invitedByUserId)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.FamilyMemberInvitations.Add(invitation);
            await ctx.SaveChangesAsync();
        });

        var factory = _fixture.CreateMockFactory();
        var sut = new InvitationsByFamilyGroupedDataLoader(factory, _batchScheduler, _options);

        // Act
        var results = await Task.WhenAll(
            sut.LoadAsync(familyIdWithInvitations, CancellationToken.None),
            sut.LoadAsync(familyIdWithoutInvitations, CancellationToken.None));

        // Assert
        results[0].Should().HaveCount(1);
        results[1].Should().BeEmpty();
    }

    [Fact]
    public async Task LoadGroupedBatchAsync_WhenNoInvitationsExist_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        var factory = _fixture.CreateMockFactory();
        var sut = new InvitationsByFamilyGroupedDataLoader(factory, _batchScheduler, _options);
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
        var factory = Substitute.For<IDbContextFactory<FamilyDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var sut = new InvitationsByFamilyGroupedDataLoader(factory, _batchScheduler, _options);

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
        var invitedByUserId = UserId.New();

        var invitation = new InvitationBuilder()
            .WithFamilyId(familyId)
            .WithEmail("token@example.com")
            .WithInvitedByUserId(invitedByUserId)
            .Build();

        await _fixture.SeedAsync(async ctx =>
        {
            ctx.FamilyMemberInvitations.Add(invitation);
            await ctx.SaveChangesAsync();
        });

        var factory = Substitute.For<IDbContextFactory<FamilyDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(_fixture.CreateDbContext()));

        var sut = new InvitationsByFamilyGroupedDataLoader(factory, _batchScheduler, _options);
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
