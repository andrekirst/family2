using System.Security.Claims;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Endpoints;

/// <summary>
/// REST endpoints for binary file upload, download, and streaming.
/// GraphQL is used for metadata operations; REST handles binary I/O.
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
        [FromServices] IFileManagementStorageService storageService,
        [FromServices] IUserRepository userRepository,
        CancellationToken ct)
    {
        var familyId = await GetFamilyIdAsync(httpContext.User, userRepository, ct);
        if (familyId is null)
            return Results.Unauthorized();

        var form = await httpContext.Request.ReadFormAsync(ct);
        var file = form.Files.FirstOrDefault();
        if (file is null || file.Length == 0)
            return Results.BadRequest(new { error = "No file provided" });

        using var stream = file.OpenReadStream();
        var result = await storageService.StoreFileAsync(familyId.Value, stream, file.FileName, ct);

        return Results.Ok(new
        {
            storageKey = result.StorageKey,
            mimeType = result.DetectedMimeType.Value,
            size = result.Size.Value,
            checksum = result.Sha256Checksum.Value
        });
    }

    private static async Task<IResult> DownloadFileAsync(
        string storageKey,
        [FromServices] IFileManagementStorageService storageService,
        CancellationToken ct)
    {
        var result = await storageService.GetFileAsync(storageKey, ct);
        if (result is null)
            return Results.NotFound();

        return Results.File(result.Data, result.MimeType);
    }

    private static async Task<IResult> StreamFileAsync(
        string storageKey,
        HttpContext httpContext,
        [FromServices] IFileManagementStorageService storageService,
        CancellationToken ct)
    {
        // Parse Range header for partial content
        var rangeHeader = httpContext.Request.Headers.Range.FirstOrDefault();
        if (rangeHeader is null)
        {
            // No range requested — return full file
            var result = await storageService.GetFileAsync(storageKey, ct);
            if (result is null)
                return Results.NotFound();

            httpContext.Response.Headers["Accept-Ranges"] = "bytes";
            return Results.File(result.Data, result.MimeType);
        }

        // Parse "bytes=start-end"
        var (from, to) = ParseRangeHeader(rangeHeader);
        if (from is null)
            return Results.BadRequest(new { error = "Invalid range header" });

        var rangeResult = await storageService.GetFileRangeAsync(
            storageKey, from.Value, to ?? long.MaxValue, ct);
        if (rangeResult is null)
            return Results.NotFound();

        httpContext.Response.StatusCode = 206;
        httpContext.Response.Headers["Accept-Ranges"] = "bytes";
        httpContext.Response.Headers["Content-Range"] =
            $"bytes {rangeResult.RangeStart}-{rangeResult.RangeEnd}/{rangeResult.TotalSize}";
        httpContext.Response.ContentLength = rangeResult.RangeEnd - rangeResult.RangeStart + 1;

        await rangeResult.Data.CopyToAsync(httpContext.Response.Body, ct);
        return Results.Empty;
    }

    private static async Task<IResult> InitiateChunkedUploadAsync(
        HttpContext httpContext,
        [FromServices] IFileManagementStorageService storageService,
        [FromServices] IUserRepository userRepository,
        CancellationToken ct)
    {
        var familyId = await GetFamilyIdAsync(httpContext.User, userRepository, ct);
        if (familyId is null)
            return Results.Unauthorized();

        var uploadId = await storageService.InitiateChunkedUploadAsync(ct);
        return Results.Ok(new { uploadId });
    }

    private static async Task<IResult> UploadChunkAsync(
        string uploadId,
        [FromQuery] int chunkIndex,
        HttpContext httpContext,
        [FromServices] IFileManagementStorageService storageService,
        CancellationToken ct)
    {
        var form = await httpContext.Request.ReadFormAsync(ct);
        var file = form.Files.FirstOrDefault();
        if (file is null || file.Length == 0)
            return Results.BadRequest(new { error = "No chunk data provided" });

        using var stream = file.OpenReadStream();
        await storageService.UploadChunkAsync(uploadId, chunkIndex, stream, ct);

        return Results.Ok(new { uploadId, chunkIndex, size = file.Length });
    }

    private static async Task<IResult> CompleteChunkedUploadAsync(
        string uploadId,
        [FromBody] CompleteUploadRequest request,
        HttpContext httpContext,
        [FromServices] IFileManagementStorageService storageService,
        [FromServices] IUserRepository userRepository,
        CancellationToken ct)
    {
        var familyId = await GetFamilyIdAsync(httpContext.User, userRepository, ct);
        if (familyId is null)
            return Results.Unauthorized();

        var result = await storageService.CompleteChunkedUploadAsync(
            familyId.Value, uploadId, request.FileName, ct);

        return Results.Ok(new
        {
            storageKey = result.StorageKey,
            mimeType = result.DetectedMimeType.Value,
            size = result.Size.Value,
            checksum = result.Sha256Checksum.Value
        });
    }

    private static async Task<FamilyId?> GetFamilyIdAsync(
        ClaimsPrincipal user, IUserRepository userRepository, CancellationToken ct)
    {
        var externalUserId = user.FindFirst(ClaimNames.Sub)?.Value;
        if (externalUserId is null) return null;

        var userEntity = await userRepository.GetByExternalIdAsync(
            ExternalUserId.From(externalUserId), ct);
        return userEntity?.FamilyId;
    }

    private static (long? From, long? To) ParseRangeHeader(string rangeHeader)
    {
        // Format: "bytes=start-end" or "bytes=start-"
        if (!rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
            return (null, null);

        var range = rangeHeader["bytes=".Length..];
        var parts = range.Split('-');
        if (parts.Length != 2) return (null, null);

        if (!long.TryParse(parts[0], out var from))
            return (null, null);

        long? to = string.IsNullOrEmpty(parts[1]) ? null : long.Parse(parts[1]);
        return (from, to);
    }
}

public sealed record CompleteUploadRequest(string FileName);
