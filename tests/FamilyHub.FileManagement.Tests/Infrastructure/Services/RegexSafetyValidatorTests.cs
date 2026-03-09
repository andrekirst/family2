using FamilyHub.Api.Common.Infrastructure.Validation;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Infrastructure.Services;

public class RegexSafetyValidatorTests
{
    [Theory]
    [InlineData(@".*\.pdf$")]
    [InlineData(@"^report_\d{4}")]
    [InlineData(@"invoice|receipt")]
    public void IsValid_WithValidPattern_ReturnsTrue(string pattern)
    {
        RegexSafetyValidator.IsValid(pattern).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WithEmptyOrWhitespace_ReturnsFalse(string pattern)
    {
        RegexSafetyValidator.IsValid(pattern).Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNull_ReturnsFalse()
    {
        RegexSafetyValidator.IsValid(null!).Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithOverLengthPattern_ReturnsFalse()
    {
        var longPattern = new string('a', 201);
        RegexSafetyValidator.IsValid(longPattern).Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithInvalidSyntax_ReturnsFalse()
    {
        RegexSafetyValidator.IsValid("[unclosed").Should().BeFalse();
    }

    [Fact]
    public void IsValid_AtExactMaxLength_ReturnsTrue()
    {
        var pattern = new string('a', 200);
        RegexSafetyValidator.IsValid(pattern).Should().BeTrue();
    }
}
