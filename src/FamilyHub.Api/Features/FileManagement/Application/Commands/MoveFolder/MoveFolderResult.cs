using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFolder;

public sealed record MoveFolderResult(FolderId FolderId, Folder MovedFolder);
