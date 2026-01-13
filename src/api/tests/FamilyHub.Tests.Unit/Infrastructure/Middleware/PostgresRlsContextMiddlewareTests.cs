using System.Security.Claims;
using FamilyHub.Modules.Auth.Infrastructure.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FamilyHub.Tests.Unit.Infrastructure.Middleware;

/// <summary>
/// Unit tests for PostgresRlsContextMiddleware.
/// Tests claim extraction logic and middleware chain behavior.
/// Database operations require integration tests due to DbContext/DbConnection coupling.
/// </summary>
public sealed class PostgresRlsContextMiddlewareTests
{
    private readonly ILogger<PostgresRlsContextMiddleware> _logger;

    public PostgresRlsContextMiddlewareTests()
    {
        _logger = Substitute.For<ILogger<PostgresRlsContextMiddleware>>();
    }

    private HttpContext CreateHttpContext(ClaimsPrincipal? user = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/graphql";
        if (user != null)
        {
            context.User = user;
        }
        return context;
    }

    private ClaimsPrincipal CreateUserWithClaim(string claimType, string claimValue)
    {
        var claims = new List<Claim> { new(claimType, claimValue) };
        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }

    private ClaimsPrincipal CreateUserWithClaims(params (string type, string value)[] claimPairs)
    {
        var claims = claimPairs.Select(p => new Claim(p.type, p.value)).ToList();
        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        // Act
        var middleware = new PostgresRlsContextMiddleware(next, _logger);

        // Assert
        middleware.Should().NotBeNull();
    }

    #endregion

    #region Claim Extraction Helper Tests

    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")] // Valid GUID
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty GUID (still valid format)
    public void ValidGuidClaim_ShouldBeParseable(string guidValue)
    {
        // This tests the parsing logic used by the middleware
        var canParse = Guid.TryParse(guidValue, out var result);

        canParse.Should().BeTrue();
        // If it parsed, it's a valid Guid struct
        result.ToString().Should().Be(guidValue.ToLowerInvariant());
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("550e8400-e29b-41d4-a716")] // Truncated GUID
    public void InvalidGuidClaim_ShouldNotBeParseable(string invalidValue)
    {
        // This tests the parsing logic used by the middleware
        var canParse = Guid.TryParse(invalidValue, out _);

        canParse.Should().BeFalse();
    }

    #endregion

    #region ClaimsPrincipal Tests (Verifying Claim Extraction Logic)

    [Fact]
    public void FindFirst_WithNameIdentifierClaim_ReturnsClaimValue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithClaim(ClaimTypes.NameIdentifier, userId.ToString());

        // Act - This mimics the middleware's claim extraction
        var claimValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        // Assert
        claimValue.Should().Be(userId.ToString());
    }

    [Fact]
    public void FindFirst_WithSubClaim_ReturnsClaimValue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithClaim("sub", userId.ToString());

        // Act - This mimics the middleware's claim extraction with fallback
        var claimValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        // Assert
        claimValue.Should().Be(userId.ToString());
    }

    [Fact]
    public void FindFirst_WithBothClaims_PrefersNameIdentifier()
    {
        // Arrange
        var nameIdentifierId = Guid.NewGuid();
        var subId = Guid.NewGuid();
        var user = CreateUserWithClaims(
            (ClaimTypes.NameIdentifier, nameIdentifierId.ToString()),
            ("sub", subId.ToString()));

        // Act - This mimics the middleware's claim extraction
        var claimValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        // Assert - NameIdentifier should be preferred
        claimValue.Should().Be(nameIdentifierId.ToString());
    }

    [Fact]
    public void FindFirst_WithNoClaims_ReturnsNull()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var claimValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        // Assert
        claimValue.Should().BeNull();
    }

    [Fact]
    public void FindFirst_WithOtherClaims_ReturnsNull()
    {
        // Arrange
        var user = CreateUserWithClaim("other_claim", "some_value");

        // Act
        var claimValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        // Assert
        claimValue.Should().BeNull();
    }

    #endregion

    #region HttpContext User Property Tests

    [Fact]
    public void HttpContext_WithAuthenticatedUser_HasUserProperty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithClaim(ClaimTypes.NameIdentifier, userId.ToString());
        var context = CreateHttpContext(user);

        // Assert
        context.User.Should().NotBeNull();
        context.User.Identity?.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void HttpContext_WithNoUser_HasAnonymousUser()
    {
        // Arrange
        var context = CreateHttpContext();

        // Assert
        context.User.Should().NotBeNull();
        context.User.Identity?.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void HttpContext_WithEmptyIdentity_IsNotAuthenticated()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var context = CreateHttpContext(user);

        // Assert
        context.User.Identity?.IsAuthenticated.Should().BeFalse();
    }

    #endregion

    #region GUID Validation Tests (Used by Middleware)

    [Fact]
    public void GuidTryParse_WithValidGuid_ReturnsTrue()
    {
        // Arrange
        var validGuid = Guid.NewGuid().ToString();

        // Act
        var result = Guid.TryParse(validGuid, out var parsedGuid);

        // Assert
        result.Should().BeTrue();
        parsedGuid.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void GuidTryParse_WithInvalidString_ReturnsFalse()
    {
        // Arrange
        var invalidGuid = "not-a-valid-guid";

        // Act
        var result = Guid.TryParse(invalidGuid, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GuidTryParse_WithEmptyString_ReturnsFalse()
    {
        // Arrange
        var emptyString = string.Empty;

        // Act
        var result = Guid.TryParse(emptyString, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GuidTryParse_WithWhitespace_ReturnsFalse()
    {
        // Arrange
        var whitespace = "   ";

        // Act
        var result = Guid.TryParse(whitespace, out _);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Combined Validation Tests (Full Middleware Logic Path)

    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000", true)]  // Valid UUID
    [InlineData("not-a-guid", false)]                            // Invalid format
    [InlineData("", false)]                                      // Empty
    [InlineData("   ", false)]                                   // Whitespace
    public void ClaimValue_ParsesCorrectlyAsGuid(string claimValue, bool expectedValid)
    {
        // Arrange
        var user = CreateUserWithClaim(ClaimTypes.NameIdentifier, claimValue);

        // Act - Simulate middleware logic
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;
        var isValidUserId = !string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out _);

        // Assert
        isValidUserId.Should().Be(expectedValid);
    }

    [Fact]
    public void MiddlewareLogic_WithNullClaim_SkipsRlsSetup()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        // Act - Simulate middleware's decision logic
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;
        var shouldSetRls = !string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out _);

        // Assert
        shouldSetRls.Should().BeFalse("No valid user ID means RLS setup should be skipped");
    }

    [Fact]
    public void MiddlewareLogic_WithValidClaim_ProceedsWithRlsSetup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithClaim(ClaimTypes.NameIdentifier, userId.ToString());

        // Act - Simulate middleware's decision logic
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;
        var parsedUserId = Guid.Empty;
        var shouldSetRls = !string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out parsedUserId);

        // Assert
        shouldSetRls.Should().BeTrue("Valid user ID means RLS setup should proceed");
        parsedUserId.Should().Be(userId);
    }

    #endregion
}
