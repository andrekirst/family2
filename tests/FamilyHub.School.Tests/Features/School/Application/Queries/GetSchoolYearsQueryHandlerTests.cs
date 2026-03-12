using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Queries.GetSchoolYears;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Application.Queries;

public class GetSchoolYearsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSchoolYearsForFamily()
    {
        // Arrange
        var familyId = FamilyId.New();
        var schoolYear = SchoolYear.Create(familyId, FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 18), new DateOnly(2026, 7, 24), DateTimeOffset.UtcNow);
        var repo = new FakeSchoolYearRepository([schoolYear]);
        var handler = new GetSchoolYearsQueryHandler(repo);

        var query = new GetSchoolYearsQuery { FamilyId = familyId, UserId = UserId.New() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(schoolYear.Id.Value);
        result[0].StartYear.Should().Be(2025);
        result[0].EndYear.Should().Be(2026);
        result[0].StartDate.Should().Be(new DateOnly(2025, 8, 18));
        result[0].EndDate.Should().Be(new DateOnly(2026, 7, 24));
        result[0].FamilyId.Should().Be(familyId.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoSchoolYears()
    {
        // Arrange
        var repo = new FakeSchoolYearRepository();
        var handler = new GetSchoolYearsQueryHandler(repo);

        var query = new GetSchoolYearsQuery { FamilyId = FamilyId.New(), UserId = UserId.New() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnSchoolYearsForRequestedFamily()
    {
        // Arrange
        var familyIdA = FamilyId.New();
        var familyIdB = FamilyId.New();
        var syA = SchoolYear.Create(familyIdA, FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);
        var syB = SchoolYear.Create(familyIdB, FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);
        var repo = new FakeSchoolYearRepository([syA, syB]);
        var handler = new GetSchoolYearsQueryHandler(repo);

        var query = new GetSchoolYearsQuery { FamilyId = familyIdA, UserId = UserId.New() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
    }
}
