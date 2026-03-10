using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetAvatar;

public sealed class GetAvatarQueryValidator : AbstractValidator<GetAvatarQuery>
{
    public GetAvatarQueryValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.AvatarId).NotEmpty();
        RuleFor(x => x.Size).NotEmpty();
    }
}
