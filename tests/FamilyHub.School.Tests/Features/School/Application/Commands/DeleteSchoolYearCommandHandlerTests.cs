using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.DeleteSchoolYear;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Application.Commands;

public class DeleteSchoolYearCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteSchoolYearAndReturnTrue()
    {
        // Arrange
        var familyId = FamilyId.New();
        var schoolYear = SchoolYear.Create(familyId, FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);
        var schoolYearRepo = new FakeSchoolYearRepository([schoolYear]);
        var classAssignmentRepo = new FakeClassAssignmentRepository();
        var handler = new DeleteSchoolYearCommandHandler(schoolYearRepo, classAssignmentRepo);

        var command = new DeleteSchoolYearCommand(schoolYear.Id) { FamilyId = familyId, UserId = UserId.New() };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        schoolYearRepo.DeletedSchoolYears.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenSchoolYearDoesNotExist()
    {
        // Arrange
        var schoolYearRepo = new FakeSchoolYearRepository();
        var classAssignmentRepo = new FakeClassAssignmentRepository();
        var handler = new DeleteSchoolYearCommandHandler(schoolYearRepo, classAssignmentRepo);

        var command = new DeleteSchoolYearCommand(SchoolYearId.New()) { FamilyId = FamilyId.New(), UserId = UserId.New() };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.SchoolYearNotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnBusinessRule_WhenSchoolYearIsInUse()
    {
        // Arrange
        var familyId = FamilyId.New();
        var schoolYear = SchoolYear.Create(familyId, FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);
        var schoolYearRepo = new FakeSchoolYearRepository([schoolYear]);

        // Create a class assignment that references this school year
        var assignment = ClassAssignment.Create(
            StudentId.New(), SchoolId.New(), schoolYear.Id,
            ClassName.From("1a"), familyId, UserId.New(), DateTimeOffset.UtcNow);
        var classAssignmentRepo = new FakeClassAssignmentRepository([assignment]);
        var handler = new DeleteSchoolYearCommandHandler(schoolYearRepo, classAssignmentRepo);

        var command = new DeleteSchoolYearCommand(schoolYear.Id) { FamilyId = familyId, UserId = UserId.New() };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.BusinessRule);
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.SchoolYearInUse);
    }

    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenSchoolYearBelongsToDifferentFamily()
    {
        // Arrange
        var schoolYear = SchoolYear.Create(FamilyId.New(), FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);
        var schoolYearRepo = new FakeSchoolYearRepository([schoolYear]);
        var classAssignmentRepo = new FakeClassAssignmentRepository();
        var handler = new DeleteSchoolYearCommandHandler(schoolYearRepo, classAssignmentRepo);

        var command = new DeleteSchoolYearCommand(schoolYear.Id) { FamilyId = FamilyId.New(), UserId = UserId.New() };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.Forbidden);
    }
}
