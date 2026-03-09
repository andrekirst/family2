using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.ModuleName.Application.Commands.CommandName;

public sealed record CommandNameCommand(
    UserId UserId) : ICommand<CommandNameResult>, IRequireUser;

public sealed record CommandNameResult(bool Success);
