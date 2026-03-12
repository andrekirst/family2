using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.CreateSchool;

public sealed class CreateSchoolAuthValidator : AbstractValidator<CreateSchoolCommand>, IAuthValidator<CreateSchoolCommand>
{
    public CreateSchoolAuthValidator(
        IFamilyMemberRepository familyMemberRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
            {
                var callerMember = await familyMemberRepository.GetByUserAndFamilyAsync(
                    command.UserId, command.FamilyId, cancellationToken);
                return callerMember is not null;
            })
            .WithErrorCode(DomainErrorCodes.UserNotInFamily)
            .WithMessage(_ => localizer[DomainErrorCodes.UserNotInFamily].Value);

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
