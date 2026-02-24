using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateTag;

public sealed record UpdateTagCommand(
    TagId TagId,
    TagName? NewName,
    TagColor? NewColor,
    FamilyId FamilyId
) : ICommand<UpdateTagResult>;
