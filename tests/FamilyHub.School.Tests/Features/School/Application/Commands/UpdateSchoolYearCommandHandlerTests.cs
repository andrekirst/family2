using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.UpdateSchoolYear;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Application.Commands;

public class UpdateSchoolYearCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateSchoolYearAndReturnResult()
    {
        // Arrange
        var familyId = FamilyId.New();
        var schoolYear = SchoolYear.Create(familyId, FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);
        var repo = new FakeSchoolYearRepository([schoolYear]);
        var handler = new UpdateSchoolYearCommandHandler(repo, TimeProvider.System);

        var newFederalStateId = FederalStateId.New();
        var command = new UpdateSchoolYearCommand(schoolYear.Id, newFederalStateId, 2026, 2027, new DateOnly(2026, 9, 1), new DateOnly(2027, 8, 31))
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UpdatedSchoolYear.FederalStateId.Should().Be(newFederalStateId);
        result.Value.UpdatedSchoolYear.StartYear.Should().Be(2026);
        result.Value.UpdatedSchoolYear.EndYear.Should().Be(2027);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenSchoolYearDoesNotExist()
    {
        // Arrange
        var repo = new FakeSchoolYearRepository();
        var handler = new UpdateSchoolYearCommandHandler(repo, TimeProvider.System);

        var command = new UpdateSchoolYearCommand(SchoolYearId.New(), FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31))
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.SchoolYearNotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenSchoolYearBelongsToDifferentFamily()
    {
        // Arrange
        var schoolYear = SchoolYear.Create(FamilyId.New(), FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);
        var repo = new FakeSchoolYearRepository([schoolYear]);
        var handler = new UpdateSchoolYearCommandHandler(repo, TimeProvider.System);

        var command = new UpdateSchoolYearCommand(schoolYear.Id, FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31))
        {
            FamilyId = FamilyId.New(), // Different family
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
    }
}
