namespace FamilyHub.Tests.Architecture.Fixtures.Violations.CqrsPatterns.Application.Commands;

/// <summary>
/// INTENTIONAL VIOLATION: Command class that does NOT implement IRequest.
/// Used for negative testing of CqrsPatternTests.Commands_ShouldImplement_IRequest
/// </summary>
public sealed record BadCommand
{
    public string Data { get; init; } = string.Empty;
}
