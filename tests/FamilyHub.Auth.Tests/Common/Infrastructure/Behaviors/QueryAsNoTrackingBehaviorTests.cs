using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Common.Infrastructure.Behaviors;
using FamilyHub.Common.Application;
using FluentAssertions;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Auth.Tests.Common.Infrastructure.Behaviors;

public class QueryAsNoTrackingBehaviorTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Should_set_no_tracking_for_read_only_query()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var behavior = new QueryAsNoTrackingBehavior<TestReadOnlyQuery, string>(context);
        var query = new TestReadOnlyQuery();

        // Act
        var result = await behavior.Handle(
            query,
            (_, _) => new ValueTask<string>("result"),
            CancellationToken.None);

        // Assert
        context.ChangeTracker.QueryTrackingBehavior.Should().Be(QueryTrackingBehavior.NoTracking);
        result.Should().Be("result");
    }

    [Fact]
    public async Task Should_not_change_tracking_for_regular_query()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var behavior = new QueryAsNoTrackingBehavior<TestRegularQuery, string>(context);
        var query = new TestRegularQuery();
        var originalBehavior = context.ChangeTracker.QueryTrackingBehavior;

        // Act
        await behavior.Handle(
            query,
            (_, _) => new ValueTask<string>("result"),
            CancellationToken.None);

        // Assert
        context.ChangeTracker.QueryTrackingBehavior.Should().Be(originalBehavior);
    }

    private sealed record TestReadOnlyQuery : IReadOnlyQuery<string>;
    private sealed record TestRegularQuery : FamilyHub.Common.Application.IQuery<string>;
}
