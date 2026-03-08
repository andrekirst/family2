using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Auth.Application.Commands.UpdateUserLocale;

public sealed class UpdateUserLocaleBusinessValidator : AbstractValidator<UpdateUserLocaleCommand>, IBusinessValidator<UpdateUserLocaleCommand>
{
    public UpdateUserLocaleBusinessValidator(
        IUserRepository userRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
                await userRepository.ExistsByIdAsync(command.UserId, ct))
            .WithErrorCode(DomainErrorCodes.UserNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.UserNotFound].Value);
    }
}
