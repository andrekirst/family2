using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateTag;

public sealed record CreateTagCommand(
    TagName Name,
    TagColor Color,
    FamilyId FamilyId,
    UserId CreatedBy
) : ICommand<CreateTagResult>;
