using FamilyHub.Api.Features.FileManagement.Models;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ProcessInboxFiles;

public sealed record ProcessInboxFilesResult(
    bool Success,
    int FilesProcessed,
    int RulesMatched,
    List<ProcessingLogEntryDto> LogEntries);
