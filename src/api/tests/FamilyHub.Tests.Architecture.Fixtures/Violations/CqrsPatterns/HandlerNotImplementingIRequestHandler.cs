namespace FamilyHub.Tests.Architecture.Fixtures.Violations.CqrsPatterns.Application.Handlers;

/// <summary>
/// INTENTIONAL VIOLATION: Handler class that does NOT implement IRequestHandler.
/// Used for negative testing of CqrsPatternTests.CommandHandlers_ShouldImplement_IRequestHandler
/// </summary>
public sealed class BadCommandHandler
{
    public void Handle(object command) { }
}
