using FamilyHub.Api.Features.BaseData.Domain.Entities;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.BaseData.Tests.Domain;

public class FederalStateTests
{
    [Fact]
    public void Create_ShouldCreateFederalStateWithValidData()
    {
        // Arrange
        var name = FederalStateName.From("Sachsen");
        var iso3166Code = Iso3166Code.From("DE-SN");

        // Act
        var state = FederalState.Create(name, iso3166Code);

        // Assert
        state.Should().NotBeNull();
        state.Id.Value.Should().NotBe(Guid.Empty);
        state.Name.Should().Be(name);
        state.Iso3166Code.Should().Be(iso3166Code);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange
        var name = FederalStateName.From("Bayern");
        var iso3166Code = Iso3166Code.From("DE-BY");

        // Act
        var state1 = FederalState.Create(name, iso3166Code);
        var state2 = FederalState.Create(name, iso3166Code);

        // Assert
        state1.Id.Should().NotBe(state2.Id);
    }

    [Fact]
    public void Create_ShouldSetNameCorrectly()
    {
        // Arrange
        var name = FederalStateName.From("Baden-Württemberg");
        var iso3166Code = Iso3166Code.From("DE-BW");

        // Act
        var state = FederalState.Create(name, iso3166Code);

        // Assert
        state.Name.Value.Should().Be("Baden-Württemberg");
    }

    [Fact]
    public void Create_ShouldSetIso3166CodeCorrectly()
    {
        // Arrange
        var name = FederalStateName.From("Berlin");
        var iso3166Code = Iso3166Code.From("DE-BE");

        // Act
        var state = FederalState.Create(name, iso3166Code);

        // Assert
        state.Iso3166Code.Value.Should().Be("DE-BE");
    }
}
