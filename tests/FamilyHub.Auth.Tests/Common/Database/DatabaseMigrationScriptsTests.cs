using System.Reflection;
using FluentAssertions;

namespace FamilyHub.Auth.Tests.Common.Database;

public class DatabaseMigrationScriptsTests
{
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    private static IEnumerable<string> GetEmbeddedSqlScripts()
    {
        return ApiAssembly
            .GetManifestResourceNames()
            .Where(r => r.Contains(".Database.Migrations.") && r.EndsWith(".sql"));
    }

    [Fact]
    public void Should_have_embedded_sql_migration_scripts()
    {
        var scripts = GetEmbeddedSqlScripts().ToList();

        scripts.Should().NotBeEmpty("DbUp requires embedded SQL scripts to run migrations");
    }

    [Theory]
    [InlineData("_bridge")]
    [InlineData("auth")]
    [InlineData("family")]
    [InlineData("calendar")]
    [InlineData("event_chain")]
    [InlineData("x_cross_schema")]
    [InlineData("rls")]
    [InlineData("avatar")]
    [InlineData("storage")]
    [InlineData("dashboard")]
    [InlineData("google_integration")]
    [InlineData("file_management")]
    [InlineData("photos")]
    [InlineData("messaging")]
    public void Should_have_scripts_for_module(string moduleName)
    {
        var scripts = GetEmbeddedSqlScripts()
            .Where(s => s.Contains($".{moduleName}."))
            .ToList();

        scripts.Should().NotBeEmpty($"module '{moduleName}' should have at least one SQL migration script");
    }

    [Fact]
    public void All_scripts_should_follow_naming_convention()
    {
        var scripts = GetEmbeddedSqlScripts().ToList();

        foreach (var script in scripts)
        {
            // Extract filename from resource name (last segment)
            var fileName = script.Split('.').TakeLast(2).First() + ".sql";

            // Filename should start with a timestamp (14 digits) followed by underscore
            fileName.Should().MatchRegex(@"^\d{14}_",
                $"script '{script}' should follow naming convention '{{YYYYMMDDHHMMSS}}_{{kebab-case}}.sql'");
        }
    }
}
