using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Repositories;
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
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IStudentRepository studentRepository,
        [Service] IFamilyMemberRepository familyMemberRepository,
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
                                   ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var user = await userRepository.GetByExternalIdAsync(externalUserId, cancellationToken)
                   ?? throw new UnauthorizedAccessException("User not found");

        if (user.FamilyId is null)
        {
            throw new UnauthorizedAccessException("User is not assigned to a family");
        }

        // Check that user has permission to manage students
        var callerMember = await familyMemberRepository.GetByUserAndFamilyAsync(user.Id, user.FamilyId.Value, cancellationToken)
                           ?? throw new UnauthorizedAccessException("User is not a family member");

        if (!callerMember.Role.CanManageStudents())
        {
            throw new UnauthorizedAccessException("Insufficient permissions to manage students");
        }

        var familyMemberId = FamilyMemberId.From(input.FamilyMemberId);
        var command = new MarkAsStudentCommand(familyMemberId, user.FamilyId.Value, user.Id);
        var result = await commandBus.SendAsync(command, cancellationToken);

        var student = await studentRepository.GetByIdAsync(result.StudentId, cancellationToken)
                      ?? throw new InvalidOperationException("Student creation failed");

        return StudentMapper.ToDto(student);
    }
}
