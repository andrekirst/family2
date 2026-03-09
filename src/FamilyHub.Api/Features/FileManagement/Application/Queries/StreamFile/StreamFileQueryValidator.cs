using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.StreamFile;

public sealed class StreamFileQueryValidator : AbstractValidator<StreamFileQuery>
{
    public StreamFileQueryValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.StorageKey).NotEmpty();
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.UserId).NotNull();

        RuleFor(x => x.RangeFrom)
            .GreaterThanOrEqualTo(0)
            .When(x => x.RangeFrom.HasValue);

        RuleFor(x => x.RangeTo)
            .GreaterThanOrEqualTo(x => x.RangeFrom!.Value)
            .When(x => x.RangeFrom.HasValue && x.RangeTo.HasValue);
    }
}
