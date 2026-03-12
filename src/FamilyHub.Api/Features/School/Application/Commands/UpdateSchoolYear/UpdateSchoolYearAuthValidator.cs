using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateSchoolYear;

public sealed class UpdateSchoolYearAuthValidator : AbstractValidator<UpdateSchoolYearCommand>, IAuthValidator<UpdateSchoolYearCommand>
{
    public UpdateSchoolYearAuthValidator(
        IFamilyMemberRepository familyMemberRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
            {
                var callerMember = await familyMemberRepository.GetByUserAndFamilyAsync(
                    command.UserId, command.FamilyId, cancellationToken);
                return callerMember is not null && callerMember.Role.CanManageSchools();
            })
            .WithErrorCode(DomainErrorCodes.InsufficientPermissionToManageSchools)
            .WithMessage(_ => localizer[DomainErrorCodes.InsufficientPermissionToManageSchools].Value);
    }
}
