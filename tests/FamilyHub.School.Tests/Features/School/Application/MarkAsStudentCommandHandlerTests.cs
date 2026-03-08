using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Application;

public class MarkAsStudentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateStudentAndReturnResult()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, _, _, targetMember) = CreateHandler(familyId, userId);
        var command = new MarkAsStudentCommand(targetMember.Id) { FamilyId = familyId, UserId = userId };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StudentId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldAddStudentToRepository()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, studentRepo, _, targetMember) = CreateHandler(familyId, userId);
        var command = new MarkAsStudentCommand(targetMember.Id) { FamilyId = familyId, UserId = userId };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        studentRepo.AddedStudents.Should().HaveCount(1);
        studentRepo.AddedStudents[0].FamilyMemberId.Should().Be(targetMember.Id);
        studentRepo.AddedStudents[0].FamilyId.Should().Be(familyId);
        studentRepo.AddedStudents[0].MarkedByUserId.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenFamilyMemberBelongsToDifferentFamily()
    {
        // Arrange — target member belongs to Family B, but command says Family A
        var familyIdA = FamilyId.New();
        var familyIdB = FamilyId.New();
        var userId = UserId.New();

        var callerMember = FamilyMember.Create(familyIdA, userId, FamilyRole.Owner);
        var targetMember = FamilyMember.Create(familyIdB, UserId.New(), FamilyRole.Member); // Different family!

        var studentRepo = new FakeStudentRepository();
        var memberRepo = new FakeFamilyMemberRepository(callerMember, [callerMember, targetMember]);
        var handler = new MarkAsStudentCommandHandler(studentRepo, memberRepo);

        var command = new MarkAsStudentCommand(targetMember.Id) { FamilyId = familyIdA, UserId = userId };

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Family member does not belong to this family");
    }

    // --- Helpers ---

    private static (MarkAsStudentCommandHandler Handler, FakeStudentRepository StudentRepo, FakeFamilyMemberRepository MemberRepo, FamilyMember TargetMember) CreateHandler(
        FamilyId familyId, UserId callerUserId)
    {
        var callerMember = FamilyMember.Create(familyId, callerUserId, FamilyRole.Owner);
        var targetMember = FamilyMember.Create(familyId, UserId.New(), FamilyRole.Member);
        var studentRepo = new FakeStudentRepository();
        var memberRepo = new FakeFamilyMemberRepository(callerMember, [callerMember, targetMember]);
        var handler = new MarkAsStudentCommandHandler(studentRepo, memberRepo);
        return (handler, studentRepo, memberRepo, targetMember);
    }
}
