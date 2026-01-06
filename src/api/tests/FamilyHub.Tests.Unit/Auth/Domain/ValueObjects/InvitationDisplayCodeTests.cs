using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.Tests.Unit.Auth.Domain.ValueObjects;

/// <summary>
/// Unit tests for InvitationDisplayCode value object.
/// </summary>
public class InvitationDisplayCodeTests
{
    [Fact]
    public void Generate_ShouldCreate8CharacterCode()
    {
        // Act
        var code = InvitationDisplayCode.Generate();

        // Assert
        code.Value.Should().HaveLength(8);
    }

    [Fact]
    public void Generate_ShouldCreateUppercaseCode()
    {
        // Act
        var code = InvitationDisplayCode.Generate();

        // Assert
        code.Value.Should().MatchRegex("^[A-Z0-9]+$");
    }

    [Fact]
    public void Generate_ShouldCreateUniqueCodesOnMultipleCalls()
    {
        // Act
        var codes = Enumerable.Range(0, 100)
            .Select(_ => InvitationDisplayCode.Generate())
            .ToList();

        // Assert - at least 90% should be unique (allows for some collisions in 100 samples)
        codes.Distinct().Count().Should().BeGreaterThan(90);
    }

    [Fact]
    public void From_WithValidCode_ShouldSucceed()
    {
        // Note: Using valid characters only (excluding ambiguous: 0, O, I, 1)
        // Act
        var code = InvitationDisplayCode.From("ABCD2345");

        // Assert
        code.Value.Should().Be("ABCD2345");
    }

    [Fact]
    public void From_WithLowercaseCode_ShouldNormalizeToUppercase()
    {
        // Act
        var code = InvitationDisplayCode.From("abcd2345");

        // Assert
        code.Value.Should().Be("ABCD2345");
    }

    [Fact]
    public void From_WithWhitespace_ShouldTrim()
    {
        // Act
        var code = InvitationDisplayCode.From("  ABCD2345  ");

        // Assert
        code.Value.Should().Be("ABCD2345");
    }

    [Fact]
    public void From_WithInvalidLength_ShouldThrow()
    {
        // Act
        var act = () => InvitationDisplayCode.From("ABC23");

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*must be exactly 8 characters*");
    }

    [Fact]
    public void From_WithEmptyString_ShouldThrow()
    {
        // Act
        var act = () => InvitationDisplayCode.From("");

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void From_WithNull_ShouldThrow()
    {
        // Act
        var act = () => InvitationDisplayCode.From(null!);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>();
    }

    [Theory]
    [InlineData("ABCD234*")] // Special character
    [InlineData("ABCD 234")] // Space
    [InlineData("ABCD-234")] // Hyphen
    [InlineData("ABCD0234")] // Contains ambiguous character: 0
    [InlineData("ABCDO234")] // Contains ambiguous character: O
    [InlineData("ABCDI234")] // Contains ambiguous character: I
    [InlineData("ABCD1234")] // Contains ambiguous character: 1
    public void From_WithInvalidCharacters_ShouldThrow(string invalidCode)
    {
        // Act
        var act = () => InvitationDisplayCode.From(invalidCode);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*must contain only alphanumeric characters*");
    }
}
