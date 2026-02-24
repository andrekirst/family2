using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ReorderOrganizationRules;

public sealed record ReorderOrganizationRulesCommand(
    List<Guid> RuleIdsInOrder,
    FamilyId FamilyId
) : ICommand<ReorderOrganizationRulesResult>;
