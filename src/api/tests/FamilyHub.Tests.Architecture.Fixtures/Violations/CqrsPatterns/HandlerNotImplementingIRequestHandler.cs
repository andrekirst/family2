namespace FamilyHub.Tests.Architecture.Fixtures.Violations.CqrsPatterns.Application.Handlers;

/// <summary>
/// INTENTIONAL VIOLATION: Handler class that does NOT implement ICommandHandler.
/// Used for negative testing of CqrsPatternTests.CommandHandlers_ShouldImplement_ICommandHandler
/// </summary>
public sealed class BadCommandHandler
{
    public void Handle(object command) { }
}

/// <summary>
/// INTENTIONAL VIOLATION: Handler class that does NOT implement IQueryHandler.
/// Used for negative testing of CqrsPatternTests.QueryHandlers_ShouldImplement_IQueryHandler
/// </summary>
public sealed class BadQueryHandler
{
    public void Handle(object query) { }
}
