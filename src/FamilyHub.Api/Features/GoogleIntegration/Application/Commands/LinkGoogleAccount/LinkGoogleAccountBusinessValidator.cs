using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Commands.LinkGoogleAccount;

public sealed class LinkGoogleAccountBusinessValidator : AbstractValidator<LinkGoogleAccountCommand>, IBusinessValidator<LinkGoogleAccountCommand>
{
    public LinkGoogleAccountBusinessValidator(
        IOAuthStateRepository stateRepository,
        IGoogleAccountLinkRepository linkRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x.State)
            .MustAsync(async (state, ct) =>
            {
                var oauthState = await stateRepository.GetByStateAsync(state, ct);
                return oauthState is not null;
            })
            .WithErrorCode(DomainErrorCodes.InvalidOAuthState)
            .WithMessage(_ => localizer[DomainErrorCodes.InvalidOAuthState].Value);

        RuleFor(x => x.State)
            .MustAsync(async (state, ct) =>
            {
                var oauthState = await stateRepository.GetByStateAsync(state, ct);
                if (oauthState is null) return true;

                var existingLink = await linkRepository.GetByUserIdAsync(oauthState.UserId, ct);
                return existingLink is null;
            })
            .WithErrorCode(DomainErrorCodes.GoogleAccountAlreadyLinked)
            .WithMessage(_ => localizer[DomainErrorCodes.GoogleAccountAlreadyLinked].Value);
    }
}
