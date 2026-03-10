using System.Reflection;
using FamilyHub.Common.Application;
using FluentAssertions;

namespace FamilyHub.Architecture.Tests;

/// <summary>
/// Ensures all command/query handlers reside in the correct nested folder structure:
///   Features/{Module}/Application/Commands/{Name}/ or
///   Features/{Module}/Application/Queries/{Name}/
/// </summary>
public class HandlerFolderStructureTests
{
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void All_command_handlers_should_reside_in_commands_namespace()
    {
        var handlerTypes = ApiAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>).GetGenericTypeDefinition()))
            .ToList();

        var violations = handlerTypes
            .Where(t => t.Namespace is not null
                && !t.Namespace.Contains(".Commands.")
                && !t.Namespace.EndsWith(".Commands"))
            .Select(t => $"{t.FullName} is not in a .Commands namespace")
            .ToList();

        violations.Should().BeEmpty(
            "all command handlers should be in a .Commands.{Name} namespace");
    }

    [Fact]
    public void All_query_handlers_should_reside_in_queries_namespace()
    {
        var handlerTypes = ApiAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>).GetGenericTypeDefinition()))
            .ToList();

        var violations = handlerTypes
            .Where(t => t.Namespace is not null
                && !t.Namespace.Contains(".Queries.")
                && !t.Namespace.EndsWith(".Queries"))
            .Select(t => $"{t.FullName} is not in a .Queries namespace")
            .ToList();

        violations.Should().BeEmpty(
            "all query handlers should be in a .Queries.{Name} namespace");
    }
}
