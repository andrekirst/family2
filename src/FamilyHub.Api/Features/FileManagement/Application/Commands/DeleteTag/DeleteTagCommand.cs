using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteTag;

public sealed record DeleteTagCommand(
    TagId TagId,
    FamilyId FamilyId
) : ICommand<DeleteTagResult>;
