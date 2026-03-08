using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.ModuleName.Application.Commands.CommandName;

public sealed record CommandNameCommand(
    FamilyId FamilyId
) : ICommand<CommandNameResult>, IFamilyScoped;
