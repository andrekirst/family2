using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.AccessShareLink;

public sealed record AccessShareLinkCommand(
    string Token,
    string? Password,
    string IpAddress,
    string? UserAgent,
    ShareAccessAction Action
) : ICommand<AccessShareLinkResult>;
