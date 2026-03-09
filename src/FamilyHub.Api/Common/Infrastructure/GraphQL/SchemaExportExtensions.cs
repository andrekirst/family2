using HotChocolate.Execution;

namespace FamilyHub.Api.Common.Infrastructure.GraphQL;

/// <summary>
/// Extension methods for exporting the Hot Chocolate GraphQL schema to SDL format.
/// Useful for documentation, client code generation, and schema versioning.
/// </summary>
public static class SchemaExportExtensions
{
    /// <summary>
    /// Exports the Hot Chocolate GraphQL schema to a <c>schema.graphql</c> file
    /// in the specified directory (defaults to the application's content root).
    /// </summary>
    /// <param name="app">The web application instance (must be built).</param>
    /// <param name="outputPath">
    /// Optional absolute path for the output file.
    /// Defaults to <c>{ContentRootPath}/schema.graphql</c>.
    /// </param>
    public static async Task ExportGraphQLSchemaAsync(
        this WebApplication app,
        string? outputPath = null)
    {
        var resolver = app.Services.GetRequiredService<IRequestExecutorResolver>();
        var executor = await resolver.GetRequestExecutorAsync(cancellationToken: CancellationToken.None);
        var schema = executor.Schema;

        var sdl = schema.ToString();

        var filePath = outputPath ?? System.IO.Path.Combine(app.Environment.ContentRootPath, "schema.graphql");

        await File.WriteAllTextAsync(filePath, sdl);

        app.Logger.LogInformation("GraphQL schema exported to {SchemaPath}", filePath);
    }
}
