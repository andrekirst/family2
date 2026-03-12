using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Domain;

public class SchoolYearEntityTests
{
    [Fact]
    public void Create_ShouldCreateSchoolYearWithValidData()
    {
        // Arrange
        var familyId = FamilyId.New();
        var federalStateId = FederalStateId.New();
        var startYear = 2025;
        var endYear = 2026;
        var startDate = new DateOnly(2025, 8, 18);
        var endDate = new DateOnly(2026, 7, 24);

        // Act
        var schoolYear = SchoolYear.Create(familyId, federalStateId, startYear, endYear, startDate, endDate, DateTimeOffset.UtcNow);

        // Assert
        schoolYear.Should().NotBeNull();
        schoolYear.Id.Value.Should().NotBe(Guid.Empty);
        schoolYear.FamilyId.Should().Be(familyId);
        schoolYear.FederalStateId.Should().Be(federalStateId);
        schoolYear.StartYear.Should().Be(startYear);
        schoolYear.EndYear.Should().Be(endYear);
        schoolYear.StartDate.Should().Be(startDate);
        schoolYear.EndDate.Should().Be(endDate);
        schoolYear.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange
        var familyId = FamilyId.New();
        var federalStateId = FederalStateId.New();

        // Act
        var sy1 = SchoolYear.Create(familyId, federalStateId, 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);
        var sy2 = SchoolYear.Create(familyId, federalStateId, 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);

        // Assert
        sy1.Id.Should().NotBe(sy2.Id);
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_WhenTodayIsWithinDateRange()
    {
        // Arrange
        var startDate = new DateOnly(2025, 8, 1);
        var endDate = new DateOnly(2026, 7, 31);
        var schoolYear = SchoolYear.Create(FamilyId.New(), FederalStateId.New(), 2025, 2026, startDate, endDate, DateTimeOffset.UtcNow);
        var today = new DateOnly(2026, 1, 15);

        // Act
        var result = schoolYear.IsCurrent(today);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnFalse_WhenTodayIsBeforeStartDate()
    {
        // Arrange
        var startDate = new DateOnly(2025, 8, 1);
        var endDate = new DateOnly(2026, 7, 31);
        var schoolYear = SchoolYear.Create(FamilyId.New(), FederalStateId.New(), 2025, 2026, startDate, endDate, DateTimeOffset.UtcNow);
        var today = new DateOnly(2025, 7, 31);

        // Act
        var result = schoolYear.IsCurrent(today);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrent_ShouldReturnFalse_WhenTodayIsAfterEndDate()
    {
        // Arrange
        var startDate = new DateOnly(2025, 8, 1);
        var endDate = new DateOnly(2026, 7, 31);
        var schoolYear = SchoolYear.Create(FamilyId.New(), FederalStateId.New(), 2025, 2026, startDate, endDate, DateTimeOffset.UtcNow);
        var today = new DateOnly(2026, 8, 1);

        // Act
        var result = schoolYear.IsCurrent(today);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_WhenTodayIsExactlyStartDate()
    {
        // Arrange
        var startDate = new DateOnly(2025, 8, 1);
        var endDate = new DateOnly(2026, 7, 31);
        var schoolYear = SchoolYear.Create(FamilyId.New(), FederalStateId.New(), 2025, 2026, startDate, endDate, DateTimeOffset.UtcNow);

        // Act
        var result = schoolYear.IsCurrent(startDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_ShouldReturnTrue_WhenTodayIsExactlyEndDate()
    {
        // Arrange
        var startDate = new DateOnly(2025, 8, 1);
        var endDate = new DateOnly(2026, 7, 31);
        var schoolYear = SchoolYear.Create(FamilyId.New(), FederalStateId.New(), 2025, 2026, startDate, endDate, DateTimeOffset.UtcNow);

        // Act
        var result = schoolYear.IsCurrent(endDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Update_ShouldUpdateAllFields()
    {
        // Arrange
        var schoolYear = SchoolYear.Create(FamilyId.New(), FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);

        var newFederalStateId = FederalStateId.New();
        var newStartDate = new DateOnly(2025, 9, 1);
        var newEndDate = new DateOnly(2026, 8, 31);

        // Act
        schoolYear.Update(newFederalStateId, 2026, 2027, newStartDate, newEndDate, DateTimeOffset.UtcNow);

        // Assert
        schoolYear.FederalStateId.Should().Be(newFederalStateId);
        schoolYear.StartYear.Should().Be(2026);
        schoolYear.EndYear.Should().Be(2027);
        schoolYear.StartDate.Should().Be(newStartDate);
        schoolYear.EndDate.Should().Be(newEndDate);
    }
}
