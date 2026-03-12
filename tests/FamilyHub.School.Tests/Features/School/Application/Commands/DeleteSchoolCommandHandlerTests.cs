using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.DeleteSchool;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using SchoolEntity = FamilyHub.Api.Features.School.Domain.Entities.School;

namespace FamilyHub.School.Tests.Features.School.Application.Commands;

public class DeleteSchoolCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteSchoolAndReturnTrue()
    {
        // Arrange
        var familyId = FamilyId.New();
        var school = SchoolEntity.Create(SchoolName.From("Test School"), familyId, FederalStateId.New(), "City", "12345", DateTimeOffset.UtcNow);
        var schoolRepo = new FakeSchoolRepository([school]);
        var handler = new DeleteSchoolCommandHandler(schoolRepo);

        var command = new DeleteSchoolCommand(school.Id) { FamilyId = familyId, UserId = UserId.New() };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        schoolRepo.DeletedSchools.Should().HaveCount(1);
        schoolRepo.DeletedSchools[0].Id.Should().Be(school.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenSchoolDoesNotExist()
    {
        // Arrange
        var schoolRepo = new FakeSchoolRepository();
        var handler = new DeleteSchoolCommandHandler(schoolRepo);

        var command = new DeleteSchoolCommand(SchoolId.New()) { FamilyId = FamilyId.New(), UserId = UserId.New() };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.SchoolNotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenSchoolBelongsToDifferentFamily()
    {
        // Arrange
        var school = SchoolEntity.Create(SchoolName.From("Test School"), FamilyId.New(), FederalStateId.New(), "City", "12345", DateTimeOffset.UtcNow);
        var schoolRepo = new FakeSchoolRepository([school]);
        var handler = new DeleteSchoolCommandHandler(schoolRepo);

        var command = new DeleteSchoolCommand(school.Id) { FamilyId = FamilyId.New(), UserId = UserId.New() };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
    }
}
