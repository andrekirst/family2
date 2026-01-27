using FamilyHub.Infrastructure.Persistence.Interceptors;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Unit.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Integration tests for TimestampInterceptor.
/// These tests verify that the interceptor correctly sets CreatedAt and UpdatedAt timestamps
/// when entities are saved to the database.
///
/// PHASE 5 STATE: Family entities are now in FamilyDbContext (family schema),
/// while User entities remain in AuthDbContext (auth schema).
/// Tests use the appropriate DbContext for each entity type.
/// </summary>
/// <remarks>
/// Uses PostgreSqlContainerFixture for isolated, reproducible database testing.
/// The fixture provides both Auth and Family schemas via Testcontainers.
/// </remarks>
[Collection("Database")]
public class TimestampInterceptorTests(PostgreSqlContainerFixture containerFixture) : IAsyncLifetime
{
    private AuthDbContext _authContext = null!;
    private FamilyDbContext _familyContext = null!;
    private FakeTimeProvider _timeProvider = null!;

    public Task InitializeAsync()
    {
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));

        // Use connection string from Testcontainers fixture
        var connectionString = containerFixture.ConnectionString;

        var authOptions = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new TimestampInterceptor(_timeProvider))
            .Options;

        var familyOptions = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new TimestampInterceptor(_timeProvider))
            .Options;

        _authContext = new AuthDbContext(authOptions);
        _familyContext = new FamilyDbContext(familyOptions);

        // Schemas are created by the fixture - no need to call EnsureCreatedAsync here
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Clean up data but don't delete schemas (fixture manages database lifecycle)
        // Order matters due to foreign key constraints:
        // 1. Delete invitations first (FK to families)
        // 2. Delete families
        // 3. Delete users
        await _familyContext.FamilyMemberInvitations.ExecuteDeleteAsync();
        await _familyContext.Families.ExecuteDeleteAsync();
        await _authContext.Users.ExecuteDeleteAsync();

        await _familyContext.DisposeAsync();
        await _authContext.DisposeAsync();
    }

    [Fact]
    public async Task SaveChanges_NewFamily_ShouldSetCreatedAtAndUpdatedAt()
    {
        // Arrange
        var expectedTime = _timeProvider.GetUtcNow().UtcDateTime;
        var family = FamilyAggregate.Create(FamilyName.From("Test Family"), UserId.New());

        // Act - Family is now in FamilyDbContext
        _familyContext.Families.Add(family);
        await _familyContext.SaveChangesAsync();

        // Assert
        family.CreatedAt.Should().Be(expectedTime);
        family.UpdatedAt.Should().Be(expectedTime);
    }

    [Fact]
    public async Task SaveChanges_ModifiedFamily_ShouldUpdateOnlyUpdatedAt()
    {
        // Arrange
        var family = FamilyAggregate.Create(FamilyName.From("Original"), UserId.New());
        _familyContext.Families.Add(family);
        await _familyContext.SaveChangesAsync();

        var createdAt = family.CreatedAt;

        // Advance time
        _timeProvider.Advance(TimeSpan.FromHours(1));
        var updateTime = _timeProvider.GetUtcNow().UtcDateTime;

        // Act
        family.UpdateName(FamilyName.From("Updated"));
        await _familyContext.SaveChangesAsync();

        // Assert
        family.CreatedAt.Should().Be(createdAt); // Unchanged
        family.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public async Task SaveChanges_NewUser_ShouldSetCreatedAtAndUpdatedAt()
    {
        // Arrange
        var expectedTime = _timeProvider.GetUtcNow().UtcDateTime;

        // Create family first in FamilyDbContext (no FK constraint in Phase 5 architecture)
        var family = FamilyAggregate.Create(FamilyName.From("Test Family"), UserId.New());
        _familyContext.Families.Add(family);
        await _familyContext.SaveChangesAsync();

        var user = User.CreateWithPassword(Email.From("test@example.com"), PasswordHash.FromHash("TestPasswordHash123!"), family.Id);

        // Act - User is in AuthDbContext
        _authContext.Users.Add(user);
        await _authContext.SaveChangesAsync();

        // Assert
        user.CreatedAt.Should().Be(expectedTime);
        user.UpdatedAt.Should().Be(expectedTime);
    }

    [Fact]
    public async Task SaveChanges_MultipleEntities_ShouldSetAllTimestamps()
    {
        // Arrange
        var expectedTime = _timeProvider.GetUtcNow().UtcDateTime;
        var userId = UserId.New();

        // Create user's family first in FamilyDbContext
        var userFamily = FamilyAggregate.Create(FamilyName.From("User Family"), userId);
        _familyContext.Families.Add(userFamily);
        await _familyContext.SaveChangesAsync();

        var user = User.CreateWithPassword(Email.From("test@example.com"), PasswordHash.FromHash("TestPasswordHash123!"), userFamily.Id);
        var family1 = FamilyAggregate.Create(FamilyName.From("Family 1"), user.Id);
        var family2 = FamilyAggregate.Create(FamilyName.From("Family 2"), user.Id);

        // Act - Add families to FamilyDbContext
        _familyContext.Families.Add(family1);
        _familyContext.Families.Add(family2);
        await _familyContext.SaveChangesAsync();

        // Add user to AuthDbContext
        _authContext.Users.Add(user);
        await _authContext.SaveChangesAsync();

        // Assert
        user.CreatedAt.Should().Be(expectedTime);
        user.UpdatedAt.Should().Be(expectedTime);
        family1.CreatedAt.Should().Be(expectedTime);
        family1.UpdatedAt.Should().Be(expectedTime);
        family2.CreatedAt.Should().Be(expectedTime);
        family2.UpdatedAt.Should().Be(expectedTime);
    }
}
