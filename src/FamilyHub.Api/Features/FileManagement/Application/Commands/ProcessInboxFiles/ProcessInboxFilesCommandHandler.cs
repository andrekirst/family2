using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ProcessInboxFiles;

public sealed class ProcessInboxFilesCommandHandler(
    IFolderRepository folderRepository,
    IStoredFileRepository fileRepository,
    IOrganizationRuleRepository ruleRepository,
    IProcessingLogRepository logRepository,
    IOrganizationRuleEngine ruleEngine,
    IInboxFileProcessor fileProcessor,
    TimeProvider timeProvider,
    ILogger<ProcessInboxFilesCommandHandler> logger)
    : ICommandHandler<ProcessInboxFilesCommand, ProcessInboxFilesResult>
{
    public async ValueTask<ProcessInboxFilesResult> Handle(
        ProcessInboxFilesCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var inboxFolder = await folderRepository.GetInboxFolderAsync(command.FamilyId, cancellationToken)
            ?? throw new DomainException("Inbox folder not found", DomainErrorCodes.InboxFolderNotFound);

        var files = await fileRepository.GetByFolderIdAsync(inboxFolder.Id, cancellationToken);
        var rules = await ruleRepository.GetEnabledByFamilyIdAsync(command.FamilyId, cancellationToken);

        var logEntries = new List<ProcessingLogEntry>();
        var rulesMatched = 0;

        foreach (var file in files)
        {
            var match = ruleEngine.EvaluateFile(file, rules);

            if (match is { Matched: true })
            {
                rulesMatched++;
                var logEntry = await ProcessFileSafely(file, match, command, cancellationToken);
                logEntries.Add(logEntry);
            }
            else
            {
                var logEntry = ProcessingLogEntry.Create(
                    file.Id, file.Name.Value,
                    null, null, null, null, null,
                    true, null, command.FamilyId, utcNow);
                logEntries.Add(logEntry);
            }
        }

        if (logEntries.Count > 0)
        {
            await logRepository.AddRangeAsync(logEntries, cancellationToken);
        }

        return new ProcessInboxFilesResult(
            true,
            files.Count,
            rulesMatched,
            logEntries.Select(FileManagementMapper.ToDto).ToList());
    }

    /// <summary>
    /// Orchestration-level error boundary: delegates to IInboxFileProcessor and
    /// catches per-file failures so the batch continues processing remaining files.
    /// </summary>
    private async Task<ProcessingLogEntry> ProcessFileSafely(
        StoredFile file,
        RuleMatchPreviewDto match,
        ProcessInboxFilesCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            return await fileProcessor.ProcessFileAsync(file, match, command.UserId, command.FamilyId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to process inbox file {FileId} with rule {RuleName}",
                file.Id, match.MatchedRuleName);

            return ProcessingLogEntry.Create(
                file.Id, file.Name.Value,
                match.MatchedRuleId.HasValue ? OrganizationRuleId.From(match.MatchedRuleId.Value) : null,
                match.MatchedRuleName,
                match.ActionType,
                null, null,
                false, ex.Message, command.FamilyId, timeProvider.GetUtcNow());
        }
    }
}
