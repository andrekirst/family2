using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FluentAssertions;
using SchoolEntity = FamilyHub.Api.Features.School.Domain.Entities.School;

namespace FamilyHub.School.Tests.Features.School.Domain;

public class SchoolEntityTests
{
    [Fact]
    public void Create_ShouldCreateSchoolWithValidData()
    {
        // Arrange
        var name = SchoolName.From("Grundschule am Park");
        var familyId = FamilyId.New();
        var federalStateId = FederalStateId.New();
        var city = "Dresden";
        var postalCode = "01069";

        // Act
        var school = SchoolEntity.Create(name, familyId, federalStateId, city, postalCode, DateTimeOffset.UtcNow);

        // Assert
        school.Should().NotBeNull();
        school.Id.Value.Should().NotBe(Guid.Empty);
        school.Name.Should().Be(name);
        school.FamilyId.Should().Be(familyId);
        school.FederalStateId.Should().Be(federalStateId);
        school.City.Should().Be(city);
        school.PostalCode.Should().Be(postalCode);
        school.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        school.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange
        var name = SchoolName.From("Test School");
        var familyId = FamilyId.New();
        var federalStateId = FederalStateId.New();

        // Act
        var school1 = SchoolEntity.Create(name, familyId, federalStateId, "City", "12345", DateTimeOffset.UtcNow);
        var school2 = SchoolEntity.Create(name, familyId, federalStateId, "City", "12345", DateTimeOffset.UtcNow);

        // Assert
        school1.Id.Should().NotBe(school2.Id);
    }

    [Fact]
    public void Update_ShouldUpdateAllFields()
    {
        // Arrange
        var school = SchoolEntity.Create(
            SchoolName.From("Old Name"),
            FamilyId.New(),
            FederalStateId.New(),
            "Old City",
            "00000",
            DateTimeOffset.UtcNow);

        var newName = SchoolName.From("New Name");
        var newFederalStateId = FederalStateId.New();
        var newCity = "New City";
        var newPostalCode = "99999";

        // Act
        school.Update(newName, newFederalStateId, newCity, newPostalCode, DateTimeOffset.UtcNow);

        // Assert
        school.Name.Should().Be(newName);
        school.FederalStateId.Should().Be(newFederalStateId);
        school.City.Should().Be(newCity);
        school.PostalCode.Should().Be(newPostalCode);
        school.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
