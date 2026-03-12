using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.CreateSchool;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Application.Commands;

public class CreateSchoolCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateSchoolAndReturnResult()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var schoolRepo = new FakeSchoolRepository();
        var handler = new CreateSchoolCommandHandler(schoolRepo, TimeProvider.System);

        var command = new CreateSchoolCommand(
            SchoolName.From("Grundschule am Park"),
            FederalStateId.New(),
            "Dresden",
            "01069")
        {
            FamilyId = familyId,
            UserId = userId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SchoolId.Value.Should().NotBe(Guid.Empty);
        result.Value.CreatedSchool.Name.Value.Should().Be("Grundschule am Park");
        result.Value.CreatedSchool.FamilyId.Should().Be(familyId);
    }

    [Fact]
    public async Task Handle_ShouldAddSchoolToRepository()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var schoolRepo = new FakeSchoolRepository();
        var handler = new CreateSchoolCommandHandler(schoolRepo, TimeProvider.System);

        var command = new CreateSchoolCommand(
            SchoolName.From("Test School"),
            FederalStateId.New(),
            "Berlin",
            "10115")
        {
            FamilyId = familyId,
            UserId = userId
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        schoolRepo.AddedSchools.Should().HaveCount(1);
        schoolRepo.AddedSchools[0].FamilyId.Should().Be(familyId);
        schoolRepo.AddedSchools[0].City.Should().Be("Berlin");
    }

    [Fact]
    public async Task Handle_ShouldSetTimestamps()
    {
        // Arrange
        var schoolRepo = new FakeSchoolRepository();
        var handler = new CreateSchoolCommandHandler(schoolRepo, TimeProvider.System);

        var command = new CreateSchoolCommand(
            SchoolName.From("Time Test School"),
            FederalStateId.New(),
            "Munich",
            "80331")
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.CreatedSchool.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        result.Value.CreatedSchool.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
