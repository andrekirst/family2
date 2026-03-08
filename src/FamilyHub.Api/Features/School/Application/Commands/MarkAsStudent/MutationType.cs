using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Mappers;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;

[ExtendObjectType(typeof(SchoolMutation))]
public class MutationType
{
    [Authorize]
    public async Task<StudentDto> MarkAsStudent(
        MarkAsStudentRequest input,
        [Service] ICommandBus commandBus,
        [Service] IStudentRepository studentRepository,
        CancellationToken cancellationToken)
    {
        var familyMemberId = FamilyMemberId.From(input.FamilyMemberId);
        var command = new MarkAsStudentCommand(familyMemberId);
        var result = await commandBus.SendAsync(command, cancellationToken);

        var student = await studentRepository.GetByIdAsync(result.StudentId, cancellationToken)
                      ?? throw new InvalidOperationException("Student creation failed");

        return StudentMapper.ToDto(student);
    }
}
