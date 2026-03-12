using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.CreateSchoolYear;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Application.Commands;

public class CreateSchoolYearCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateSchoolYearAndReturnResult()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var schoolYearRepo = new FakeSchoolYearRepository();
        var handler = new CreateSchoolYearCommandHandler(schoolYearRepo, TimeProvider.System);

        var command = new CreateSchoolYearCommand(
            FederalStateId.New(),
            2025,
            2026,
            new DateOnly(2025, 8, 18),
            new DateOnly(2026, 7, 24))
        {
            FamilyId = familyId,
            UserId = userId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SchoolYearId.Value.Should().NotBe(Guid.Empty);
        result.Value.CreatedSchoolYear.StartYear.Should().Be(2025);
        result.Value.CreatedSchoolYear.EndYear.Should().Be(2026);
        result.Value.CreatedSchoolYear.FamilyId.Should().Be(familyId);
    }

    [Fact]
    public async Task Handle_ShouldAddSchoolYearToRepository()
    {
        // Arrange
        var schoolYearRepo = new FakeSchoolYearRepository();
        var handler = new CreateSchoolYearCommandHandler(schoolYearRepo, TimeProvider.System);

        var command = new CreateSchoolYearCommand(
            FederalStateId.New(),
            2025,
            2026,
            new DateOnly(2025, 8, 18),
            new DateOnly(2026, 7, 24))
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        schoolYearRepo.AddedSchoolYears.Should().HaveCount(1);
        schoolYearRepo.AddedSchoolYears[0].StartYear.Should().Be(2025);
        schoolYearRepo.AddedSchoolYears[0].EndYear.Should().Be(2026);
    }

    [Fact]
    public async Task Handle_ShouldSetStartAndEndDates()
    {
        // Arrange
        var schoolYearRepo = new FakeSchoolYearRepository();
        var handler = new CreateSchoolYearCommandHandler(schoolYearRepo, TimeProvider.System);

        var startDate = new DateOnly(2025, 8, 18);
        var endDate = new DateOnly(2026, 7, 24);

        var command = new CreateSchoolYearCommand(
            FederalStateId.New(),
            2025,
            2026,
            startDate,
            endDate)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.CreatedSchoolYear.StartDate.Should().Be(startDate);
        result.Value.CreatedSchoolYear.EndDate.Should().Be(endDate);
    }
}
