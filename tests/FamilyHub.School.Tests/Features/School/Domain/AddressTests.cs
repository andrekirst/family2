using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Domain;

public class AddressTests
{
    [Fact]
    public void Create_ShouldCreateAddressWithAllFields()
    {
        // Arrange
        var federalStateId = Guid.NewGuid();

        // Act
        var address = Address.Create("Main Street", "42", "01069", "Dresden", "Germany", federalStateId);

        // Assert
        address.Should().NotBeNull();
        address.Street.Should().Be("Main Street");
        address.HouseNumber.Should().Be("42");
        address.PostalCode.Should().Be("01069");
        address.City.Should().Be("Dresden");
        address.Country.Should().Be("Germany");
        address.FederalStateId.Should().Be(federalStateId);
    }

    [Fact]
    public void Create_ShouldCreateAddressWithAllNullFields()
    {
        // Arrange & Act
        var address = Address.Create(null, null, null, null, null, null);

        // Assert
        address.Should().NotBeNull();
        address.Street.Should().BeNull();
        address.HouseNumber.Should().BeNull();
        address.PostalCode.Should().BeNull();
        address.City.Should().BeNull();
        address.Country.Should().BeNull();
        address.FederalStateId.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldUpdateAllFields()
    {
        // Arrange
        var address = Address.Create("Old Street", "1", "00000", "Old City", "Old Country", null);

        var newFederalStateId = Guid.NewGuid();

        // Act
        address.Update("New Street", "99", "99999", "New City", "New Country", newFederalStateId);

        // Assert
        address.Street.Should().Be("New Street");
        address.HouseNumber.Should().Be("99");
        address.PostalCode.Should().Be("99999");
        address.City.Should().Be("New City");
        address.Country.Should().Be("New Country");
        address.FederalStateId.Should().Be(newFederalStateId);
    }

    [Fact]
    public void Update_ShouldAllowSettingFieldsToNull()
    {
        // Arrange
        var address = Address.Create("Street", "1", "12345", "City", "Country", Guid.NewGuid());

        // Act
        address.Update(null, null, null, null, null, null);

        // Assert
        address.Street.Should().BeNull();
        address.HouseNumber.Should().BeNull();
        address.PostalCode.Should().BeNull();
        address.City.Should().BeNull();
        address.Country.Should().BeNull();
        address.FederalStateId.Should().BeNull();
    }
}
