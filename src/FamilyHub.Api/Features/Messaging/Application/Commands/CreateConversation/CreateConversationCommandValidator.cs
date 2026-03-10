using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.CreateConversation;

/// <summary>
/// Validator for CreateConversationCommand.
/// Direct requires exactly 2 members, Group requires 2+, Family enforced at handler level.
/// </summary>
public sealed class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>, IInputValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.FamilyId)
            .NotNull()
            .WithMessage(_ => localizer["FamilyIdRequired"]);

        RuleFor(x => x.UserId)
            .NotNull()
            .WithMessage(_ => localizer["SenderIdRequired"]);

        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage("Conversation name is required");

        RuleFor(x => x.MemberIds)
            .Must(ids => ids.Count == 2)
            .When(x => x.Type == ConversationType.Direct)
            .WithMessage("Direct conversations require exactly 2 members");

        RuleFor(x => x.MemberIds)
            .Must(ids => ids.Count >= 2)
            .When(x => x.Type == ConversationType.Group)
            .WithMessage("Group conversations require at least 2 members");
    }
}
