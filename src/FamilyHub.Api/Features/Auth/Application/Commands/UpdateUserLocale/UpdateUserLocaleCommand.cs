using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Auth.Application.Commands.UpdateUserLocale;

/// <summary>
/// Command to update a user's preferred locale for UI language.
/// </summary>
public sealed record UpdateUserLocaleCommand(
    ExternalUserId ExternalUserId,
    string Locale
) : ICommand<UpdateUserLocaleResult>;

public sealed record UpdateUserLocaleResult(bool Success);
