using System.Reflection;
using DbUp;
using DbUp.Engine;

namespace FamilyHub.Api.Common.Database;

/// <summary>
/// Runs database migrations using DbUp with embedded SQL scripts.
/// Scripts are embedded as assembly resources and sorted alphabetically by
/// fully-qualified resource name. Folder naming ensures correct execution order:
/// _bridge → auth → avatar → calendar → ... → storage → x_cross_schema (after all modules).
/// All DDL uses IF NOT EXISTS for idempotency, so re-running on existing databases is safe.
/// </summary>
public static class DatabaseMigrationRunner
{

    public static DatabaseUpgradeResult Migrate(string connectionString, ILogger logger)
    {
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var assembly = Assembly.GetExecutingAssembly();

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(assembly, script => IsMigrationScript(script))
            .WithTransactionPerScript()
            .WithExecutionTimeout(TimeSpan.FromMinutes(5))
            .LogTo(new DbUpSerilogAdapter(logger))
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            logger.LogError(result.Error, "Database migration failed");
        }
        else
        {
            var scriptCount = result.Scripts.Count();
            logger.LogInformation("Database migration completed successfully. {ScriptCount} script(s) executed",
                scriptCount);
        }

        return result;
    }

    private static bool IsMigrationScript(string resourceName)
    {
        return resourceName.Contains(".Database.Migrations.") && resourceName.EndsWith(".sql");
    }

    /// <summary>
    /// Adapter to route DbUp log output through ILogger.
    /// </summary>
    private sealed class DbUpSerilogAdapter(ILogger logger) : DbUp.Engine.Output.IUpgradeLog
    {
        public void LogTrace(string format, params object[] args) =>
            logger.LogTrace("{Message}", string.Format(format, args));

        public void LogDebug(string format, params object[] args) =>
            logger.LogDebug("{Message}", string.Format(format, args));

        public void LogInformation(string format, params object[] args) =>
            logger.LogInformation("{Message}", string.Format(format, args));

        public void LogWarning(string format, params object[] args) =>
            logger.LogWarning("{Message}", string.Format(format, args));

        public void LogError(string format, params object[] args) =>
            logger.LogError("{Message}", string.Format(format, args));

        public void LogError(Exception ex, string format, params object[] args) =>
            logger.LogError(ex, "{Message}", string.Format(format, args));
    }
}
