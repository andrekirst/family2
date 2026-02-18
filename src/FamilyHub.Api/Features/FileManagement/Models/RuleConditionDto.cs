using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Models;

public sealed record RuleConditionDto(
    RuleConditionType Type,
    string Value);
