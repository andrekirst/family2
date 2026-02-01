using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Auth.Models;
using FamilyHub.Api.Features.Auth.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FamilyHub.UnitTests.Auth;

/// <summary>
/// Unit tests for AuthService
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Setup in-memory database for testing
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _authService = new AuthService(_context);
    }

    [Fact]
    public async Task RegisterUserAsync_WithNewUser_CreatesUser()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            ExternalUserId = "keycloak-123",
            ExternalProvider = "KEYCLOAK",
            EmailVerified = true
        };

        // Act
        var result = await _authService.RegisterUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.Name);
        Assert.True(result.EmailVerified);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task RegisterUserAsync_WithDuplicateEmail_ThrowsException()
    {
        // Arrange
        var request1 = new RegisterUserRequest
        {
            Email = "duplicate@example.com",
            Name = "User 1",
            ExternalUserId = "keycloak-1",
            ExternalProvider = "KEYCLOAK",
            EmailVerified = true
        };

        var request2 = new RegisterUserRequest
        {
            Email = "duplicate@example.com",
            Name = "User 2",
            ExternalUserId = "keycloak-2",
            ExternalProvider = "KEYCLOAK",
            EmailVerified = true
        };

        // Act
        await _authService.RegisterUserAsync(request1);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.RegisterUserAsync(request2)
        );
    }

    [Fact]
    public async Task RegisterUserAsync_WithExistingExternalId_UpdatesUser()
    {
        // Arrange - Register user first time
        var request1 = new RegisterUserRequest
        {
            Email = "update@example.com",
            Name = "Original Name",
            ExternalUserId = "keycloak-update",
            ExternalProvider = "KEYCLOAK",
            EmailVerified = false
        };

        var firstResult = await _authService.RegisterUserAsync(request1);

        // Act - Register same external ID with updated info
        var request2 = new RegisterUserRequest
        {
            Email = "updated@example.com",
            Name = "Updated Name",
            ExternalUserId = "keycloak-update",
            ExternalProvider = "KEYCLOAK",
            EmailVerified = true
        };

        var secondResult = await _authService.RegisterUserAsync(request2);

        // Assert
        Assert.Equal(firstResult.Id, secondResult.Id); // Same user ID
        Assert.Equal("updated@example.com", secondResult.Email); // Updated email
        Assert.Equal("Updated Name", secondResult.Name); // Updated name
        Assert.True(secondResult.EmailVerified); // Updated verification status
    }

    [Fact]
    public async Task GetUserByExternalIdAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Email = "find@example.com",
            Name = "Find Me",
            ExternalUserId = "keycloak-find",
            ExternalProvider = "KEYCLOAK",
            EmailVerified = true
        };

        await _authService.RegisterUserAsync(request);

        // Act
        var result = await _authService.GetUserByExternalIdAsync("keycloak-find");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("find@example.com", result.Email);
        Assert.Equal("Find Me", result.Name);
    }

    [Fact]
    public async Task GetUserByExternalIdAsync_WithNonExistentUser_ReturnsNull()
    {
        // Act
        var result = await _authService.GetUserByExternalIdAsync("non-existent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateFamilyIdAsync_WithValidUser_UpdatesFamilyId()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Email = "family@example.com",
            Name = "Family User",
            ExternalUserId = "keycloak-family",
            ExternalProvider = "KEYCLOAK",
            EmailVerified = true
        };

        var user = await _authService.RegisterUserAsync(request);
        var familyId = Guid.NewGuid();

        // Act
        var result = await _authService.UpdateFamilyIdAsync(user.Id, familyId);

        // Assert
        Assert.True(result);

        var updatedUser = await _authService.GetUserByIdAsync(user.Id);
        Assert.Equal(familyId, updatedUser!.FamilyId);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
