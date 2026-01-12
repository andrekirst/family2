using MediatR;

namespace FamilyHub.Tests.Architecture.Fixtures.Violations.NamingConventions.Application.Commands;

/// <summary>
/// INTENTIONAL VIOLATION: Command class without 'Command' suffix.
/// Used for negative testing of NamingConventionTests.Commands_ShouldEndWith_Command
/// </summary>
public sealed record CreateUser : IRequest<bool>
{
    public string Name { get; init; } = string.Empty;
}
