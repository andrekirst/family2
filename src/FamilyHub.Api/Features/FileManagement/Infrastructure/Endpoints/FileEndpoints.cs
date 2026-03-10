using FamilyHub.Api.Common.Infrastructure.ErrorMapping;
using FamilyHub.Api.Features.FileManagement.Application.Commands.CompleteChunkedUpload;
using FamilyHub.Api.Features.FileManagement.Application.Commands.InitiateChunkedUpload;
using FamilyHub.Api.Features.FileManagement.Application.Commands.StoreUploadedFile;
using FamilyHub.Api.Features.FileManagement.Application.Commands.UploadChunk;
using FamilyHub.Api.Features.FileManagement.Application.Queries.DownloadFile;
using FamilyHub.Api.Features.FileManagement.Application.Queries.StreamFile;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Application;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Endpoints;

/// <summary>
/// REST endpoints for binary file upload, download, and streaming.
/// GraphQL is used for metadata operations; REST handles binary I/O.
/// All endpoints delegate to Mediator commands/queries for pipeline support
/// (validation, logging, user resolution, transactions, audit).
/// </summary>
public static class FileEndpoints
{
    public static void MapFileEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/files")
            .RequireAuthorization();

        group.MapPost("/upload", UploadFileAsync)
            .DisableAntiforgery();

        group.MapGet("/{storageKey}/download", DownloadFileAsync);

        group.MapGet("/{storageKey}/stream", StreamFileAsync);

        group.MapPost("/upload/initiate", InitiateChunkedUploadAsync);

        group.MapPost("/upload/{uploadId}/chunk", UploadChunkAsync)
            .DisableAntiforgery();

        group.MapPost("/upload/{uploadId}/complete", CompleteChunkedUploadAsync);
    }

    private static async Task<IResult> UploadFileAsync(
        HttpContext httpContext,
        [FromServices] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var form = await httpContext.Request.ReadFormAsync(cancellationToken);
        var file = form.Files.FirstOrDefault();
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        // Copy to MemoryStream so it survives past the form scope.
        // The handler will call storageService.StoreFileAsync() with FamilyId from the pipeline.
        var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        var command = new StoreUploadedFileCommand(
            FileStream: memoryStream,
            FileName: file.FileName);

        var result = await commandBus.SendAsync(command, cancellationToken);

        return result.ToHttpResult(success => Results.Ok(new StoreUploadedFileResponse(
            StorageKey: success.StorageKey.Value,
            MimeType: success.MimeType.Value,
            Size: success.FileSize.Value,
            Checksum: success.Checksum.Value)));
    }

    private static async Task<IResult> DownloadFileAsync(
        string storageKey,
        [FromServices] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new DownloadFileQuery(storageKey);
        var result = await queryBus.QueryAsync(query, cancellationToken);

        return result.ToHttpResult(success =>
            Results.File(success.Data, success.MimeType));
    }

    private static async Task<IResult> StreamFileAsync(
        string storageKey,
        HttpContext httpContext,
        [FromServices] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        // Parse Range header (HTTP transport concern — stays in endpoint)
        var rangeHeader = httpContext.Request.Headers.Range.FirstOrDefault();
        long? rangeFrom = null;
        long? rangeTo = null;

        if (rangeHeader is not null)
        {
            var (from, to) = ParseRangeHeader(rangeHeader);
            if (from is null)
            {
                return Results.BadRequest(new { error = "Invalid range header" });
            }
            rangeFrom = from;
            rangeTo = to;
        }

        var query = new StreamFileQuery(storageKey, rangeFrom, rangeTo);
        var result = await queryBus.QueryAsync(query, cancellationToken);

        return result.Match(
            success =>
            {
                httpContext.Response.Headers["Accept-Ranges"] = "bytes";

                if (success.IsPartialContent)
                {
                    // 206 Partial Content with range headers
                    httpContext.Response.StatusCode = 206;
                    httpContext.Response.Headers["Content-Range"] =
                        $"bytes {success.RangeStart}-{success.RangeEnd}/{success.TotalSize}";
                    httpContext.Response.ContentLength = success.RangeEnd - success.RangeStart + 1;

                    // Must write directly to response body for streaming
                    return Results.Stream(success.Data, success.MimeType);
                }

                return Results.File(success.Data, success.MimeType);
            },
            error => DomainErrorToProblemDetailsMapper.ToProblemDetails(error));
    }

    private static async Task<IResult> InitiateChunkedUploadAsync(
        [FromServices] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new InitiateChunkedUploadCommand();
        var result = await commandBus.SendAsync(command, cancellationToken);

        return result.ToHttpResult(success => Results.Ok(
            new InitiateChunkedUploadResponse(success.UploadId)));
    }

    private static async Task<IResult> UploadChunkAsync(
        string uploadId,
        [FromQuery] int chunkIndex,
        HttpContext httpContext,
        [FromServices] IFileManagementStorageService storageService,
        [FromServices] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var form = await httpContext.Request.ReadFormAsync(cancellationToken);
        var file = form.Files.FirstOrDefault();
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No chunk data provided" });
        }

        // Phase 1: Upload bytes via storage service (no FamilyId needed for chunk storage)
        using var stream = file.OpenReadStream();
        await storageService.UploadChunkAsync(uploadId, chunkIndex, stream, cancellationToken);

        // Phase 2: Record chunk metadata via command pipeline (validation, logging, user resolution, audit)
        var command = new UploadChunkCommand(
            UploadId: uploadId,
            ChunkIndex: chunkIndex,
            ChunkSize: file.Length);

        var result = await commandBus.SendAsync(command, cancellationToken);

        return result.ToHttpResult(success => Results.Ok(
            new UploadChunkResponse(success.UploadId, success.ChunkIndex, success.Size)));
    }

    private static async Task<IResult> CompleteChunkedUploadAsync(
        string uploadId,
        [FromBody] CompleteChunkedUploadRequest request,
        [FromServices] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new CompleteChunkedUploadCommand(
            UploadId: uploadId,
            FileName: request.FileName);

        var result = await commandBus.SendAsync(command, cancellationToken);

        return result.ToHttpResult(success => Results.Ok(
            new CompleteChunkedUploadResponse(
                StorageKey: success.StorageKey.Value,
                MimeType: success.MimeType.Value,
                Size: success.Size.Value,
                Checksum: success.Checksum.Value)));
    }

    private static (long? From, long? To) ParseRangeHeader(string rangeHeader)
    {
        // Format: "bytes=start-end" or "bytes=start-"
        if (!rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
        {
            return (null, null);
        }

        var range = rangeHeader["bytes=".Length..];
        var parts = range.Split('-');
        if (parts.Length != 2)
        {
            return (null, null);
        }

        if (!long.TryParse(parts[0], out var from))
        {
            return (null, null);
        }

        long? to = string.IsNullOrEmpty(parts[1]) ? null : long.Parse(parts[1]);
        return (from, to);
    }
}

/// <summary>
/// Request body for the complete chunked upload endpoint.
/// </summary>
public sealed record CompleteChunkedUploadRequest(string FileName);
