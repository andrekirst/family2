namespace FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

/// <summary>
/// Types of conditions that can be used in organization rules.
/// </summary>
public enum RuleConditionType
{
    FileExtension = 1,
    MimeType = 2,
    FileNameRegex = 3,
    SizeGreaterThan = 4,
    SizeLessThan = 5,
    UploadDateAfter = 6,
    UploadDateBefore = 7
}
