using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.DownloadFile;

public sealed class DownloadFileQueryValidator : AbstractValidator<DownloadFileQuery>
{
    public DownloadFileQueryValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.StorageKey).NotEmpty();
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.UserId).NotNull();
    }
}
