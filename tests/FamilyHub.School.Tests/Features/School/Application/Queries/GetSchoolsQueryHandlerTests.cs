using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Queries.GetSchools;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using SchoolEntity = FamilyHub.Api.Features.School.Domain.Entities.School;

namespace FamilyHub.School.Tests.Features.School.Application.Queries;

public class GetSchoolsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSchoolsForFamily()
    {
        // Arrange
        var familyId = FamilyId.New();
        var school = SchoolEntity.Create(SchoolName.From("Grundschule am Park"), familyId, FederalStateId.New(), "Dresden", "01069", DateTimeOffset.UtcNow);
        var schoolRepo = new FakeSchoolRepository([school]);
        var handler = new GetSchoolsQueryHandler(schoolRepo);

        var query = new GetSchoolsQuery { FamilyId = familyId, UserId = UserId.New() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(school.Id.Value);
        result[0].Name.Should().Be("Grundschule am Park");
        result[0].City.Should().Be("Dresden");
        result[0].PostalCode.Should().Be("01069");
        result[0].FamilyId.Should().Be(familyId.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoSchools()
    {
        // Arrange
        var schoolRepo = new FakeSchoolRepository();
        var handler = new GetSchoolsQueryHandler(schoolRepo);

        var query = new GetSchoolsQuery { FamilyId = FamilyId.New(), UserId = UserId.New() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnSchoolsForRequestedFamily()
    {
        // Arrange
        var familyIdA = FamilyId.New();
        var familyIdB = FamilyId.New();
        var schoolA = SchoolEntity.Create(SchoolName.From("School A"), familyIdA, FederalStateId.New(), "City A", "11111", DateTimeOffset.UtcNow);
        var schoolB = SchoolEntity.Create(SchoolName.From("School B"), familyIdB, FederalStateId.New(), "City B", "22222", DateTimeOffset.UtcNow);
        var schoolRepo = new FakeSchoolRepository([schoolA, schoolB]);
        var handler = new GetSchoolsQueryHandler(schoolRepo);

        var query = new GetSchoolsQuery { FamilyId = familyIdA, UserId = UserId.New() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("School A");
    }
}
