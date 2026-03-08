using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;

public sealed class MarkAsStudentAuthValidator : AbstractValidator<MarkAsStudentCommand>, IAuthValidator<MarkAsStudentCommand>
{
    public MarkAsStudentAuthValidator(
        IFamilyMemberRepository familyMemberRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var callerMember = await familyMemberRepository.GetByUserAndFamilyAsync(
                    command.UserId, command.FamilyId, ct);
                return callerMember is not null;
            })
            .WithErrorCode(DomainErrorCodes.UserNotInFamily)
            .WithMessage(_ => localizer[DomainErrorCodes.UserNotInFamily].Value);

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var callerMember = await familyMemberRepository.GetByUserAndFamilyAsync(
                    command.UserId, command.FamilyId, ct);
                return callerMember is not null && callerMember.Role.CanManageStudents();
            })
            .WithErrorCode(DomainErrorCodes.Forbidden)
            .WithMessage(_ => localizer[DomainErrorCodes.Forbidden].Value);
    }
}
