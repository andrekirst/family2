using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetOrganizationRules;

public sealed record GetOrganizationRulesQuery(
    FamilyId FamilyId
) : IQuery<List<OrganizationRuleDto>>;
