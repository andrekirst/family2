using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FluentAssertions;
using Vogen;

namespace FamilyHub.BaseData.Tests.Domain;

public class Iso3166CodeTests
{
    [Theory]
    [InlineData("DE-BW")]
    [InlineData("DE-SN")]
    [InlineData("US-CA")]
    [InlineData("AT-W")]
    [InlineData("CH-ZH")]
    [InlineData("FR-IDF")]
    public void From_ShouldSucceed_WithValidCodes(string code)
    {
        // Act
        var iso3166Code = Iso3166Code.From(code);

        // Assert
        iso3166Code.Value.Should().Be(code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("de-bw")]
    [InlineData("DEBW")]
    [InlineData("D-BW")]
    [InlineData("DE-")]
    [InlineData("DE-ABCD")]
    [InlineData("-BW")]
    [InlineData("DE")]
    [InlineData("DEE-BW")]
    public void From_ShouldThrow_WithInvalidCodes(string code)
    {
        // Act
        var act = () => Iso3166Code.From(code);

        // Assert
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_ShouldThrow_WithNullCode()
    {
        // Act
        var act = () => Iso3166Code.From(null!);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Equality_ShouldWork_ForSameCode()
    {
        // Arrange
        var code1 = Iso3166Code.From("DE-SN");
        var code2 = Iso3166Code.From("DE-SN");

        // Assert
        code1.Should().Be(code2);
    }

    [Fact]
    public void Equality_ShouldFail_ForDifferentCodes()
    {
        // Arrange
        var code1 = Iso3166Code.From("DE-SN");
        var code2 = Iso3166Code.From("DE-BW");

        // Assert
        code1.Should().NotBe(code2);
    }
}
