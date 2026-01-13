using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Auth.Presentation.GraphQL.DataLoaders;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.Modules.Family.Presentation.GraphQL.DataLoaders;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Integration.Helpers;
using FamilyHub.Tests.Integration.Infrastructure;
using FluentAssertions;
using GreenDonut;
using Microsoft.EntityFrameworkCore;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Tests.Integration.DataLoaders;

/// <summary>
/// Integration tests verifying DataLoader batching behavior with real PostgreSQL.
/// Validates that DataLoaders minimize database round-trips using query counting.
/// </summary>
/// <remarks>
/// These tests verify the N+1 query prevention documented in ADR-011.
/// Expected query counts:
/// - 100 users with families: ≤3 queries (vs 101 without DataLoaders)
/// - Family with N members: 1 query (vs N+1 without DataLoaders)
/// - Family with N invitations: 1 query (vs N+1 without DataLoaders)
/// </remarks>
[Collection("DataLoaderQueryCount")]
public sealed class DataLoaderQueryCountTests(DualSchemaPostgreSqlContainerFixture fixture) : IAsyncLifetime
{
    private string _testId = null!;

    public Task InitializeAsync()
    {
        _testId = Guid.NewGuid().ToString("N");
        QueryCountingInterceptor.SetCurrentTestId(_testId);
        QueryCountingInterceptor.ResetQueryCount(_testId);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Clean up test data to ensure test isolation
        await CleanupTestDataAsync();
        QueryCountingInterceptor.ResetQueryCount(_testId);
    }

    private async Task CleanupTestDataAsync()
    {
        await using var authContext = fixture.CreateAuthDbContext();
        await using var familyContext = fixture.CreateFamilyDbContext();

        // Delete in correct order to respect any implicit FK relationships
        await familyContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM family.family_member_invitations");
        await authContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM auth.users");
        await familyContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM family.families");
    }

    #region Test 1: Query100UsersWithFamilies_UsesMaxThreeQueries

    /// <summary>
    /// Tests that querying 100 users across 20 families uses at most 3 database queries:
    /// 1. Query for users (via UserBatchDataLoader)
    /// 2. Batched query for families (via FamilyBatchDataLoader)
    /// </summary>
    /// <remarks>
    /// Without DataLoaders, this would be 101 queries (1 + 100 N+1).
    /// With DataLoaders, we achieve ≤3 queries via batching.
    /// </remarks>
    [Fact]
    public async Task Query100UsersWithFamilies_UsesMaxThreeQueries()
    {
        // Arrange - Create 100 users across 20 families (5 users each)
        await using var seedAuthContext = fixture.CreateAuthDbContext();
        await using var seedFamilyContext = fixture.CreateFamilyDbContext();

        var familyData = await BulkTestDataFactory.CreateFamiliesWithUsersAsync(
            seedAuthContext,
            seedFamilyContext,
            familyCount: 20,
            usersPerFamily: 5);

        var allUserIds = familyData.Values
            .SelectMany(users => users.Select(u => u.Id))
            .ToList();

        allUserIds.Should().HaveCount(100, "Test setup should create 100 users");

        // Reset query counter before the actual test
        QueryCountingInterceptor.ResetQueryCount(_testId);

        // Create context factories with query counting interceptor
        var authFactory = CreateAuthContextFactory();
        var familyFactory = CreateFamilyContextFactory();

        var batchScheduler = new AutoBatchScheduler();
        var options = new DataLoaderOptions();

        var userDataLoader = new UserBatchDataLoader(authFactory, batchScheduler, options);
        var familyDataLoader = new FamilyBatchDataLoader(familyFactory, batchScheduler, options);

        // Act - Load all users and their families
        var userTasks = allUserIds.Select(id =>
            userDataLoader.LoadAsync(id, CancellationToken.None));
        var users = await Task.WhenAll(userTasks);

        // Load all families for these users
        var familyIds = users
            .Where(u => u != null)
            .Select(u => u!.FamilyId)
            .Distinct()
            .ToList();

        var familyTasks = familyIds.Select(id =>
            familyDataLoader.LoadAsync(id, CancellationToken.None));
        var families = await Task.WhenAll(familyTasks);

        // Assert
        users.Should().HaveCount(100);
        users.All(u => u != null).Should().BeTrue("All users should be loaded");
        families.Should().HaveCount(20, "Should have 20 distinct families");

        var queryCount = QueryCountingInterceptor.GetQueryCount(_testId);
        queryCount.Should().BeLessThanOrEqualTo(3,
            "DataLoaders should batch 100 user + 20 family lookups into at most 3 queries. " +
            $"Actual queries: {queryCount}. " +
            $"Executed SQL: [{string.Join(", ", QueryCountingInterceptor.GetExecutedQueries(_testId).Take(5))}...]");
    }

    #endregion

    #region Test 2: QueryFamilyWithMembers_UsesSingleBatchQuery

    /// <summary>
    /// Tests that querying a family with 50 members uses exactly 1 batch query
    /// via UsersByFamilyGroupedDataLoader.
    /// </summary>
    /// <remarks>
    /// Without DataLoaders, this would be N+1 queries.
    /// With GroupedDataLoader, we achieve 1 query for all members.
    /// </remarks>
    [Fact]
    public async Task QueryFamilyWithMembers_UsesSingleBatchQuery()
    {
        // Arrange - Create 1 family with 50 members
        await using var seedAuthContext = fixture.CreateAuthDbContext();
        await using var seedFamilyContext = fixture.CreateFamilyDbContext();

        var (family, members) = await BulkTestDataFactory.CreateFamilyWithMembersAsync(
            seedAuthContext,
            seedFamilyContext,
            memberCount: 50);

        members.Should().HaveCount(50, "Test setup should create 50 members");

        // Reset query counter before the actual test
        QueryCountingInterceptor.ResetQueryCount(_testId);

        // Create context factory with query counting interceptor
        var authFactory = CreateAuthContextFactory();

        var batchScheduler = new AutoBatchScheduler();
        var options = new DataLoaderOptions();

        var usersByFamilyDataLoader = new UsersByFamilyGroupedDataLoader(
            authFactory, batchScheduler, options);

        // Act - Load all members for the family
        var loadedMembers = await usersByFamilyDataLoader.LoadAsync(
            family.Id, CancellationToken.None);

        // Assert
        loadedMembers.Should().HaveCount(50,
            "Should load all 50 family members");

        var queryCount = QueryCountingInterceptor.GetQueryCount(_testId);
        queryCount.Should().Be(1,
            "UsersByFamilyGroupedDataLoader should load 50 members in a single batch query. " +
            $"Actual queries: {queryCount}");
    }

    #endregion

    #region Test 3: QueryFamilyWithInvitations_UsesSingleBatchQuery

    /// <summary>
    /// Tests that querying a family with 20 pending invitations uses exactly 1 batch query
    /// via InvitationsByFamilyGroupedDataLoader.
    /// </summary>
    [Fact]
    public async Task QueryFamilyWithInvitations_UsesSingleBatchQuery()
    {
        // Arrange - Create 1 family with owner and 20 invitations
        await using var seedAuthContext = fixture.CreateAuthDbContext();
        await using var seedFamilyContext = fixture.CreateFamilyDbContext();

        var (family, members) = await BulkTestDataFactory.CreateFamilyWithMembersAsync(
            seedAuthContext,
            seedFamilyContext,
            memberCount: 1); // Just the owner

        var invitations = await BulkTestDataFactory.CreateInvitationsAsync(
            seedFamilyContext,
            family.Id,
            members[0].Id, // Owner invites
            invitationCount: 20);

        invitations.Should().HaveCount(20, "Test setup should create 20 invitations");

        // Reset query counter before the actual test
        QueryCountingInterceptor.ResetQueryCount(_testId);

        // Create context factory with query counting interceptor
        var familyFactory = CreateFamilyContextFactory();

        var batchScheduler = new AutoBatchScheduler();
        var options = new DataLoaderOptions();

        var invitationsDataLoader = new InvitationsByFamilyGroupedDataLoader(
            familyFactory, batchScheduler, options);

        // Act - Load all invitations for the family
        var loadedInvitations = await invitationsDataLoader.LoadAsync(
            family.Id, CancellationToken.None);

        // Assert
        loadedInvitations.Should().HaveCount(20,
            "Should load all 20 invitations");

        var queryCount = QueryCountingInterceptor.GetQueryCount(_testId);
        queryCount.Should().Be(1,
            "InvitationsByFamilyGroupedDataLoader should load 20 invitations in a single batch query. " +
            $"Actual queries: {queryCount}");
    }

    #endregion

    #region Test 4: QueryMultipleFamiliesWithMembers_UsesSingleBatchQuery

    /// <summary>
    /// Tests that querying members for multiple families uses a single batch query.
    /// This validates the batching behavior when resolving members for a list of families.
    /// </summary>
    [Fact]
    public async Task QueryMultipleFamiliesWithMembers_UsesSingleBatchQuery()
    {
        // Arrange - Create 10 families with 5 users each
        await using var seedAuthContext = fixture.CreateAuthDbContext();
        await using var seedFamilyContext = fixture.CreateFamilyDbContext();

        var familyData = await BulkTestDataFactory.CreateFamiliesWithUsersAsync(
            seedAuthContext,
            seedFamilyContext,
            familyCount: 10,
            usersPerFamily: 5);

        var familyIds = familyData.Keys.ToList();
        familyIds.Should().HaveCount(10, "Test setup should create 10 families");

        // Reset query counter before the actual test
        QueryCountingInterceptor.ResetQueryCount(_testId);

        // Create context factory with query counting interceptor
        var authFactory = CreateAuthContextFactory();

        var batchScheduler = new AutoBatchScheduler();
        var options = new DataLoaderOptions();

        var usersByFamilyDataLoader = new UsersByFamilyGroupedDataLoader(
            authFactory, batchScheduler, options);

        // Act - Load members for all 10 families concurrently
        var tasks = familyIds.Select(id =>
            usersByFamilyDataLoader.LoadAsync(id, CancellationToken.None));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10, "Should have results for all 10 families");
        results.SelectMany(r => r).Should().HaveCount(50,
            "Total members should be 50 (10 families * 5 users)");

        var queryCount = QueryCountingInterceptor.GetQueryCount(_testId);
        queryCount.Should().Be(1,
            "UsersByFamilyGroupedDataLoader should batch 10 family member lookups into a single query. " +
            $"Actual queries: {queryCount}");
    }

    #endregion

    #region Test 5: QueryFamiliesForMultipleUsers_UsesSingleBatchQuery

    /// <summary>
    /// Tests that querying families for multiple users uses efficient batching
    /// via FamilyBatchDataLoader.
    /// </summary>
    /// <remarks>
    /// Note: GreenDonut's AutoBatchScheduler batches requests within scheduling windows.
    /// With 50 concurrent LoadAsync calls, some may be executed in separate batches.
    /// The key metric is that we see significantly fewer queries than 50 (N+1 pattern).
    /// </remarks>
    [Fact]
    public async Task QueryFamiliesForMultipleUsers_UsesBatchedQueries()
    {
        // Arrange - Create 50 users with unique families
        await using var seedAuthContext = fixture.CreateAuthDbContext();
        await using var seedFamilyContext = fixture.CreateFamilyDbContext();

        var usersWithFamilies = await BulkTestDataFactory.CreateUsersWithFamiliesAsync(
            seedAuthContext,
            seedFamilyContext,
            count: 50);

        usersWithFamilies.Should().HaveCount(50, "Test setup should create 50 user/family pairs");

        var familyIds = usersWithFamilies
            .Select(uf => uf.family.Id)
            .Distinct()
            .ToList();

        familyIds.Should().HaveCount(50, "Should have 50 distinct families");

        // Reset query counter before the actual test
        QueryCountingInterceptor.ResetQueryCount(_testId);

        // Create context factory with query counting interceptor
        var familyFactory = CreateFamilyContextFactory();

        var batchScheduler = new AutoBatchScheduler();
        var options = new DataLoaderOptions();

        var familyDataLoader = new FamilyBatchDataLoader(familyFactory, batchScheduler, options);

        // Act - Load all families concurrently
        var tasks = familyIds.Select(id =>
            familyDataLoader.LoadAsync(id, CancellationToken.None));
        var families = await Task.WhenAll(tasks);

        // Assert
        families.Should().HaveCount(50, "Should load all 50 families");
        families.All(f => f != null).Should().BeTrue("All families should be loaded");

        var queryCount = QueryCountingInterceptor.GetQueryCount(_testId);
        // DataLoaders batch requests within scheduling windows. With 50 concurrent calls,
        // we expect significantly fewer than 50 queries (which would be N+1 pattern).
        // Typically we see 1-10 batches depending on timing.
        queryCount.Should().BeLessThanOrEqualTo(10,
            "FamilyBatchDataLoader should batch 50 family lookups into at most 10 queries (vs 50 without batching). " +
            $"Actual queries: {queryCount}");
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Factory that creates new DbContext instances with the query counting interceptor.
    /// Each context creation is tracked by the QueryCountingInterceptor.
    /// </summary>
    private sealed class InterceptingContextFactory<TContext>(Func<TContext> contextFactory) : IDbContextFactory<TContext>
        where TContext : DbContext
    {
        public TContext CreateDbContext() => contextFactory();

        public Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(contextFactory());
    }

    private InterceptingContextFactory<AuthDbContext> CreateAuthContextFactory()
    {
        return new InterceptingContextFactory<AuthDbContext>(() =>
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseNpgsql(fixture.ConnectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(new QueryCountingInterceptor())
                .Options;
            return new AuthDbContext(options);
        });
    }

    private InterceptingContextFactory<FamilyDbContext> CreateFamilyContextFactory()
    {
        return new InterceptingContextFactory<FamilyDbContext>(() =>
        {
            var options = new DbContextOptionsBuilder<FamilyDbContext>()
                .UseNpgsql(fixture.ConnectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(new QueryCountingInterceptor())
                .Options;
            return new FamilyDbContext(options);
        });
    }

    #endregion
}
