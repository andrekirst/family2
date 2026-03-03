using System.Collections.Concurrent;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Infrastructure;

/// <summary>
/// Mock file upload/download endpoints for message attachments.
/// Stores files in memory and serves them back for download.
/// Will be replaced by the File Management module.
/// </summary>
[Obsolete("Mock endpoint — will be replaced by File Management module")]
public static class MockFileUploadEndpoint
{
    private static readonly ConcurrentDictionary<Guid, MockFile> Store = new();

    private sealed record MockFile(byte[] Data, string FileName, string ContentType);

    public static void MapMockFileUploadEndpoint(this WebApplication app)
    {
        app.MapPost("/api/messaging/mock-upload", async (HttpContext httpContext) =>
        {
            var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
            var file = form.Files.FirstOrDefault();

            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { error = "No file provided" });
            }

            var fileId = FileId.New();

            // Store file in memory for mock download
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, httpContext.RequestAborted);
            Store[fileId.Value] = new MockFile(
                ms.ToArray(),
                file.FileName,
                file.ContentType ?? "application/octet-stream");

            return Results.Ok(new
            {
                fileId = fileId.Value,
                fileName = file.FileName,
                mimeType = file.ContentType ?? "application/octet-stream",
                fileSize = file.Length
            });
        })
            .RequireAuthorization()
            .DisableAntiforgery();

        // Download endpoint is unauthenticated — the random FileId GUID acts as
        // a capability token. This will be replaced by the File Management module
        // which uses proper authorization.
        app.MapGet("/api/messaging/mock-download/{fileId:guid}", (Guid fileId) =>
        {
            if (!Store.TryGetValue(fileId, out var file))
            {
                return Results.NotFound(new { error = "File not found or server was restarted" });
            }

            return Results.File(file.Data, file.ContentType, file.FileName);
        })
            .AllowAnonymous();
    }
}
