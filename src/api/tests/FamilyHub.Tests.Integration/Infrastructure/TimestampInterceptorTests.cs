using FamilyDomain = FamilyHub.Modules.Family.Domain;
using FamilyHub.Infrastructure.Persistence.Interceptors;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Unit.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Integration tests for TimestampInterceptor.
/// These tests verify that the interceptor correctly sets CreatedAt and UpdatedAt timestamps
/// when entities are saved to the database.
/// </summary>
[Collection("Database")]
public class TimestampInterceptorTests : IAsyncLifetime
{
    private AuthDbContext _context = null!;
    private FakeTimeProvider _timeProvider = null!;

    public async Task InitializeAsync()
    {
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));

        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(_timeProvider);
        var serviceProvider = services.BuildServiceProvider();

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql("Host=localhost;Database=familyhub_test;Username=familyhub;Password=Dev123!")
            .AddInterceptors(new TimestampInterceptor(_timeProvider))
            .Options;

        _context = new AuthDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task SaveChanges_NewFamily_ShouldSetCreatedAtAndUpdatedAt()
    {
        // Arrange
        var expectedTime = _timeProvider.GetUtcNow().UtcDateTime;
        var family = FamilyDomain.Family.Create(FamilyName.From("Test Family"), UserId.New());

        // Act
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        // Assert
        family.CreatedAt.Should().Be(expectedTime);
        family.UpdatedAt.Should().Be(expectedTime);
    }

    [Fact]
    public async Task SaveChanges_ModifiedFamily_ShouldUpdateOnlyUpdatedAt()
    {
        // Arrange
        var family = FamilyDomain.Family.Create(FamilyName.From("Original"), UserId.New());
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var createdAt = family.CreatedAt;

        // Advance time
        _timeProvider.Advance(TimeSpan.FromHours(1));
        var updateTime = _timeProvider.GetUtcNow().UtcDateTime;

        // Act
        family.UpdateName(FamilyName.From("Updated"));
        await _context.SaveChangesAsync();

        // Assert
        family.CreatedAt.Should().Be(createdAt); // Unchanged
        family.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public async Task SaveChanges_NewUser_ShouldSetCreatedAtAndUpdatedAt()
    {
        // Arrange
        var expectedTime = _timeProvider.GetUtcNow().UtcDateTime;

        // Create family first (required by foreign key constraint)
        var family = FamilyDomain.Family.Create(FamilyName.From("Test Family"), UserId.New());
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var user = User.CreateFromOAuth(Email.From("test@example.com"), "ext123", "zitadel", family.Id);

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert
        user.CreatedAt.Should().Be(expectedTime);
        user.UpdatedAt.Should().Be(expectedTime);
    }

    [Fact]
    public async Task SaveChanges_MultipleEntities_ShouldSetAllTimestamps()
    {
        // Arrange
        var expectedTime = _timeProvider.GetUtcNow().UtcDateTime;

        // Create user's family first (required by foreign key constraint)
        var userFamily = FamilyDomain.Family.Create(FamilyName.From("User Family"), UserId.New());
        _context.Families.Add(userFamily);
        await _context.SaveChangesAsync();

        var user = User.CreateFromOAuth(Email.From("test@example.com"), "ext123", "zitadel", userFamily.Id);
        var family1 = FamilyDomain.Family.Create(FamilyName.From("Family 1"), user.Id);
        var family2 = FamilyDomain.Family.Create(FamilyName.From("Family 2"), user.Id);

        // Act
        _context.Users.Add(user);
        _context.Families.Add(family1);
        _context.Families.Add(family2);
        await _context.SaveChangesAsync();

        // Assert
        user.CreatedAt.Should().Be(expectedTime);
        user.UpdatedAt.Should().Be(expectedTime);
        family1.CreatedAt.Should().Be(expectedTime);
        family1.UpdatedAt.Should().Be(expectedTime);
        family2.CreatedAt.Should().Be(expectedTime);
        family2.UpdatedAt.Should().Be(expectedTime);
    }
}
