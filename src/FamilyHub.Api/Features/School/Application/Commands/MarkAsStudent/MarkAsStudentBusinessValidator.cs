using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;

public sealed class MarkAsStudentBusinessValidator : AbstractValidator<MarkAsStudentCommand>, IBusinessValidator<MarkAsStudentCommand>
{
    public MarkAsStudentBusinessValidator(
        IStudentRepository studentRepository,
        IFamilyMemberRepository familyMemberRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var alreadyExists = await studentRepository.ExistsByFamilyMemberIdAsync(command.FamilyMemberId, ct);
                return !alreadyExists;
            })
            .WithErrorCode(DomainErrorCodes.FamilyMemberAlreadyStudent)
            .WithMessage(_ => localizer[DomainErrorCodes.FamilyMemberAlreadyStudent].Value);

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var targetMember = await familyMemberRepository.GetByIdAsync(command.FamilyMemberId, ct);
                return targetMember is not null;
            })
            .WithErrorCode(DomainErrorCodes.FamilyMemberNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.FamilyMemberNotFound].Value);
    }
}
