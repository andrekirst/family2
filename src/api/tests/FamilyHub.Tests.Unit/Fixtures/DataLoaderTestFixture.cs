using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FamilyHub.Tests.Unit.Fixtures;

/// <summary>
/// Reusable test fixture for DataLoader unit tests.
/// Provides in-memory DbContext instances and mock factories for testing
/// batching and caching behavior of GreenDonut DataLoaders.
/// </summary>
/// <typeparam name="TContext">The DbContext type to use for testing.</typeparam>
public sealed class DataLoaderTestFixture<TContext> : IDisposable
    where TContext : DbContext
{
    private readonly DbContextOptions<TContext> _options;
    private readonly List<TContext> _contexts = [];
    private readonly string _databaseName;

    public DataLoaderTestFixture()
    {
        _databaseName = $"TestDb_{Guid.NewGuid()}";
        _options = new DbContextOptionsBuilder<TContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;
    }

    /// <summary>
    /// Creates a new DbContext instance with the shared in-memory database.
    /// All contexts share the same database for data persistence during tests.
    /// </summary>
    public TContext CreateDbContext()
    {
        var context = (TContext)Activator.CreateInstance(typeof(TContext), _options)!;
        _contexts.Add(context);
        return context;
    }

    /// <summary>
    /// Creates a mock IDbContextFactory that returns new DbContext instances.
    /// Use this for basic DataLoader tests without query counting.
    /// </summary>
    public IDbContextFactory<TContext> CreateMockFactory()
    {
        var factory = Substitute.For<IDbContextFactory<TContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(CreateDbContext()));
        return factory;
    }

    /// <summary>
    /// Creates a mock IDbContextFactory that tracks the number of CreateDbContextAsync calls.
    /// Use this for verifying batching behavior (should be 1 call for multiple keys).
    /// </summary>
    /// <param name="callCount">Output parameter that will be incremented each time the factory is called.</param>
    public IDbContextFactory<TContext> CreateMockFactoryWithCallTracking(out int[] callCount)
    {
        var count = new int[] { 0 };
        callCount = count;

        var factory = Substitute.For<IDbContextFactory<TContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                count[0]++;
                return Task.FromResult(CreateDbContext());
            });
        return factory;
    }

    /// <summary>
    /// Seeds the in-memory database with test data.
    /// Data is persisted across all DbContext instances created by this fixture.
    /// </summary>
    /// <param name="seedAction">Action to seed data using the DbContext.</param>
    public async Task SeedAsync(Func<TContext, Task> seedAction)
    {
        await using var context = CreateDbContext();
        await seedAction(context);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds the in-memory database with test data synchronously.
    /// </summary>
    /// <param name="seedAction">Action to seed data using the DbContext.</param>
    public void Seed(Action<TContext> seedAction)
    {
        using var context = CreateDbContext();
        seedAction(context);
        context.SaveChanges();
    }

    public void Dispose()
    {
        foreach (var context in _contexts)
        {
            context.Dispose();
        }
    }
}
