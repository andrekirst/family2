using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Commands.UnlinkGoogleAccount;

public sealed class UnlinkGoogleAccountBusinessValidator : AbstractValidator<UnlinkGoogleAccountCommand>, IBusinessValidator<UnlinkGoogleAccountCommand>
{
    public UnlinkGoogleAccountBusinessValidator(
        IGoogleAccountLinkRepository linkRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x.UserId)
            .MustAsync(async (userId, ct) =>
            {
                var link = await linkRepository.GetByUserIdAsync(userId, ct);
                return link is not null;
            })
            .WithErrorCode(DomainErrorCodes.NoGoogleAccountLinked)
            .WithMessage(_ => localizer[DomainErrorCodes.NoGoogleAccountLinked].Value);
    }
}
