using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Tests.Unit.Auth.Domain.ValueObjects;

/// <summary>
/// Unit tests for InvitationToken value object.
/// </summary>
public class InvitationTokenTests
{
    [Fact]
    public void Generate_ShouldCreate64CharacterToken()
    {
        // Act
        var token = InvitationToken.Generate();

        // Assert
        token.Value.Should().HaveLength(64);
    }

    [Fact]
    public void Generate_ShouldCreateUrlSafeBase64Token()
    {
        // Act
        var token = InvitationToken.Generate();

        // Assert - URL-safe base64: alphanumeric, -, _
        token.Value.Should().MatchRegex("^[A-Za-z0-9_-]+$");
        token.Value.Should().NotContain("+");
        token.Value.Should().NotContain("/");
        token.Value.Should().NotContain("=");
    }

    [Fact]
    public void Generate_ShouldCreateUniqueTokens()
    {
        // Act - Generate 100 tokens
        var tokens = Enumerable.Range(0, 100)
            .Select(_ => InvitationToken.Generate())
            .ToList();

        // Assert - All should be unique (cryptographically random)
        tokens.Distinct().Count().Should().Be(100);
    }

    [Fact]
    public void From_WithValidToken_ShouldSucceed()
    {
        // Arrange
        var validToken = new string('a', 64);

        // Act
        var token = InvitationToken.From(validToken);

        // Assert
        token.Value.Should().Be(validToken);
    }

    [Fact]
    public void From_WithInvalidLength_ShouldThrow()
    {
        // Arrange
        var shortToken = new string('a', 32);

        // Act
        var act = () => InvitationToken.From(shortToken);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*must be exactly 64 characters*");
    }

    [Fact]
    public void From_WithEmptyString_ShouldThrow()
    {
        // Act
        var act = () => InvitationToken.From("");

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void From_WithNull_ShouldThrow()
    {
        // Act
        var act = () => InvitationToken.From(null!);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>();
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")] // 62 chars - too short
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890123456789")] // 72 chars - too long
    public void From_WithWrongLength_ShouldThrow(string invalidToken)
    {
        // Act
        var act = () => InvitationToken.From(invalidToken);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*must be exactly 64 characters*");
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456*")] // Contains *
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ012345 ")] // Contains space
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123+6=")] // Contains + and =
    public void From_WithInvalidCharacters_ShouldThrow(string invalidToken)
    {
        // Arrange - Pad to 64 chars
        var paddedToken = invalidToken.PadRight(64, 'a');

        // Act
        var act = () => InvitationToken.From(paddedToken);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*must contain only URL-safe base64 characters*");
    }
}
