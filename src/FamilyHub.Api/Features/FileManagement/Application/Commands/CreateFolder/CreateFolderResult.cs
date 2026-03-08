using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFolder;

public sealed record CreateFolderResult(FolderId FolderId, Folder CreatedFolder);
