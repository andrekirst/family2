using System.Reflection;
using FamilyHub.Api.Common.Modules;
using FluentAssertions;

namespace FamilyHub.Architecture.Tests;

/// <summary>
/// Ensures all IModule implementations follow registration conventions.
/// </summary>
public class ModuleRegistrationTests
{
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void All_IModule_implementations_should_have_ModuleOrder_attribute()
    {
        var moduleTypes = ApiAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                && t.GetInterfaces().Any(i => i == typeof(IModule)));

        var missingAttribute = moduleTypes
            .Where(t => t.GetCustomAttribute<ModuleOrderAttribute>() is null)
            .Select(t => t.Name)
            .ToList();

        missingAttribute.Should().BeEmpty(
            "all IModule implementations must have [ModuleOrder(N)] for source-generated registration. " +
            "Missing: {0}", string.Join(", ", missingAttribute));
    }

    [Fact]
    public void ModuleOrder_values_should_be_unique()
    {
        var moduleTypes = ApiAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                && t.GetInterfaces().Any(i => i == typeof(IModule)));

        var orders = moduleTypes
            .Select(t => new
            {
                t.Name,
                Order = t.GetCustomAttribute<ModuleOrderAttribute>()?.Order ?? -1
            })
            .ToList();

        var duplicates = orders
            .GroupBy(x => x.Order)
            .Where(g => g.Count() > 1)
            .Select(g => $"Order {g.Key}: {string.Join(", ", g.Select(x => x.Name))}")
            .ToList();

        duplicates.Should().BeEmpty(
            "each module must have a unique [ModuleOrder] value to ensure deterministic registration order");
    }
}
