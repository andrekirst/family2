using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.CreateFamily;

/// <summary>
/// Business validator for CreateFamilyCommand.
/// Checks that the user exists and does not already own a family.
/// </summary>
public sealed class CreateFamilyBusinessValidator : AbstractValidator<CreateFamilyCommand>, IBusinessValidator<CreateFamilyCommand>
{
    public CreateFamilyBusinessValidator(
        IFamilyRepository familyRepository,
        IUserRepository userRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
            {
                var existingFamily = await familyRepository.GetByOwnerIdAsync(command.UserId, cancellationToken);
                return existingFamily is null;
            })
            .WithErrorCode(DomainErrorCodes.UserAlreadyOwnsFamily)
            .WithMessage(_ => localizer[DomainErrorCodes.UserAlreadyOwnsFamily].Value);

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
                await userRepository.ExistsByIdAsync(command.UserId, cancellationToken))
            .WithErrorCode(DomainErrorCodes.UserNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.UserNotFound].Value);
    }
}
