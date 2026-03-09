using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Queries.GetStudents;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.School.Tests.Features.School.Application;

public class GetStudentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnStudentsForFamily()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var targetMember = FamilyMember.Create(familyId, UserId.New(), FamilyRole.Member);
        var student = Student.Create(targetMember.Id, familyId, userId);

        var studentRepo = Substitute.For<IStudentRepository>();
        var memberRepo = Substitute.For<IFamilyMemberRepository>();

        studentRepo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([student]);
        memberRepo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([targetMember]);

        var handler = new GetStudentsQueryHandler(studentRepo, memberRepo);
        var query = new GetStudentsQuery { FamilyId = familyId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(student.Id.Value);
        result[0].FamilyMemberId.Should().Be(targetMember.Id.Value);
        result[0].FamilyId.Should().Be(familyId.Value);
        result[0].MarkedByUserId.Should().Be(userId.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoStudents()
    {
        // Arrange
        var familyId = FamilyId.New();
        var studentRepo = Substitute.For<IStudentRepository>();
        var memberRepo = Substitute.For<IFamilyMemberRepository>();

        studentRepo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(new List<Student>());

        var handler = new GetStudentsQueryHandler(studentRepo, memberRepo);
        var query = new GetStudentsQuery { FamilyId = familyId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
