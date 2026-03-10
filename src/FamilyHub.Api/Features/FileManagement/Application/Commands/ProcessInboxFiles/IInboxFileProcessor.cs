using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ProcessInboxFiles;

/// <summary>
/// Processes a single inbox file by applying the matched rule action.
/// Does not catch exceptions — error handling is the caller's responsibility.
/// </summary>
public interface IInboxFileProcessor
{
    Task<ProcessingLogEntry> ProcessFileAsync(
        StoredFile file,
        RuleMatchPreviewDto match,
        UserId movedBy,
        FamilyId familyId,
        CancellationToken cancellationToken);
}
