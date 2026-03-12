using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.UpdateSchool;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using SchoolEntity = FamilyHub.Api.Features.School.Domain.Entities.School;

namespace FamilyHub.School.Tests.Features.School.Application.Commands;

public class UpdateSchoolCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateSchoolAndReturnResult()
    {
        // Arrange
        var familyId = FamilyId.New();
        var school = SchoolEntity.Create(SchoolName.From("Old School"), familyId, FederalStateId.New(), "Old City", "00000", DateTimeOffset.UtcNow);
        var schoolRepo = new FakeSchoolRepository([school]);
        var handler = new UpdateSchoolCommandHandler(schoolRepo, TimeProvider.System);

        var newName = SchoolName.From("New School Name");
        var newFederalStateId = FederalStateId.New();
        var command = new UpdateSchoolCommand(school.Id, newName, newFederalStateId, "New City", "99999")
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UpdatedSchool.Name.Should().Be(newName);
        result.Value.UpdatedSchool.City.Should().Be("New City");
        result.Value.UpdatedSchool.PostalCode.Should().Be("99999");
        result.Value.UpdatedSchool.FederalStateId.Should().Be(newFederalStateId);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenSchoolDoesNotExist()
    {
        // Arrange
        var schoolRepo = new FakeSchoolRepository();
        var handler = new UpdateSchoolCommandHandler(schoolRepo, TimeProvider.System);

        var command = new UpdateSchoolCommand(SchoolId.New(), SchoolName.From("Test"), FederalStateId.New(), "City", "12345")
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

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
        var handler = new UpdateSchoolCommandHandler(schoolRepo, TimeProvider.System);

        var differentFamilyId = FamilyId.New();
        var command = new UpdateSchoolCommand(school.Id, SchoolName.From("Updated"), FederalStateId.New(), "City", "12345")
        {
            FamilyId = differentFamilyId,
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
    }
}
