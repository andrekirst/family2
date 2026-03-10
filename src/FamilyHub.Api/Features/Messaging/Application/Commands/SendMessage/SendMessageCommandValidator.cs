using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.SendMessage;

/// <summary>
/// Validator for SendMessageCommand.
/// Vogen already enforces basic MessageContent validation;
/// this provides additional business rules.
/// </summary>
public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>, IInputValidator<SendMessageCommand>
{
    public SendMessageCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Content)
            .NotNull()
            .WithMessage(_ => localizer["MessageContentRequired"]);

        RuleFor(x => x.FamilyId)
            .NotNull()
            .WithMessage(_ => localizer["FamilyIdRequired"]);

        RuleFor(x => x.UserId)
            .NotNull()
            .WithMessage(_ => localizer["SenderIdRequired"]);

        RuleFor(x => x)
            .Must(cmd => cmd.Content.Value.Length > 0 || (cmd.Attachments is not null && cmd.Attachments.Count > 0))
            .WithMessage("Message must have content or at least one attachment");
    }
}
