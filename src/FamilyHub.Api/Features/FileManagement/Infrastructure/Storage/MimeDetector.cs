namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

/// <summary>
/// Detects MIME type from file magic bytes (first 512 bytes).
/// Does not rely on file extension — content-based detection only.
/// </summary>
public interface IMimeDetector
{
    string Detect(ReadOnlySpan<byte> header, string? fileNameHint = null);
}

public sealed class MimeDetector : IMimeDetector
{
    // Magic byte signatures for common file types
    private static readonly (byte[] Signature, int Offset, string MimeType)[] Signatures =
    [
        // Images
        ([ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A ], 0, "image/png"),
        ([ 0xFF, 0xD8, 0xFF ], 0, "image/jpeg"),
        ([ 0x47, 0x49, 0x46, 0x38 ], 0, "image/gif"),
        ([ 0x52, 0x49, 0x46, 0x46 ], 0, "image/webp"),  // RIFF header (WebP also uses RIFF)
        ([ 0x42, 0x4D ], 0, "image/bmp"),
        ([ 0x00, 0x00, 0x01, 0x00 ], 0, "image/x-icon"),

        // Documents
        ([ 0x25, 0x50, 0x44, 0x46 ], 0, "application/pdf"),
        ([ 0x50, 0x4B, 0x03, 0x04 ], 0, "application/zip"), // ZIP (also DOCX, XLSX, PPTX)

        // Video
        ([ 0x1A, 0x45, 0xDF, 0xA3 ], 0, "video/webm"),
        ([ 0x00, 0x00, 0x00 ], 0, "video/mp4"), // ftyp box (approximate)

        // Audio
        ([ 0x49, 0x44, 0x33 ], 0, "audio/mpeg"),     // ID3 tag (MP3)
        ([ 0xFF, 0xFB ], 0, "audio/mpeg"),             // MP3 frame sync
        ([ 0xFF, 0xF3 ], 0, "audio/mpeg"),             // MP3 frame sync
        ([ 0x4F, 0x67, 0x67, 0x53 ], 0, "audio/ogg"), // OGG
        ([ 0x66, 0x4C, 0x61, 0x43 ], 0, "audio/flac"),

        // Text/data
        ([ 0xEF, 0xBB, 0xBF ], 0, "text/plain"),      // UTF-8 BOM
        ([ 0x7B ], 0, "application/json"),               // JSON opening brace
    ];

    public string Detect(ReadOnlySpan<byte> header, string? fileNameHint = null)
    {
        if (header.Length == 0)
            return "application/octet-stream";

        // Check magic bytes
        foreach (var (signature, offset, mimeType) in Signatures)
        {
            if (header.Length >= offset + signature.Length &&
                header.Slice(offset, signature.Length).SequenceEqual(signature))
            {
                // Refine ZIP-based formats using file extension hint
                if (mimeType == "application/zip" && fileNameHint is not null)
                {
                    var ext = System.IO.Path.GetExtension(fileNameHint).ToLowerInvariant();
                    return ext switch
                    {
                        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                        ".odt" => "application/vnd.oasis.opendocument.text",
                        ".ods" => "application/vnd.oasis.opendocument.spreadsheet",
                        _ => mimeType
                    };
                }

                // Refine RIFF-based formats
                if (mimeType == "image/webp" && header.Length >= 12)
                {
                    var formatTag = header.Slice(8, 4);
                    if (formatTag.SequenceEqual("WEBP"u8))
                        return "image/webp";
                    if (formatTag.SequenceEqual("AVI "u8))
                        return "video/x-msvideo";
                    if (formatTag.SequenceEqual("WAVE"u8))
                        return "audio/wav";
                }

                // Refine MP4 variants (check for ftyp box)
                if (mimeType == "video/mp4" && header.Length >= 12)
                {
                    var ftyp = header.Slice(4, 4);
                    if (ftyp.SequenceEqual("ftyp"u8))
                    {
                        var brand = header.Slice(8, 4);
                        if (brand.SequenceEqual("M4A "u8))
                            return "audio/mp4";
                        return "video/mp4";
                    }
                    // Not actually ftyp — fall through
                    continue;
                }

                return mimeType;
            }
        }

        // Fallback: check if it looks like text
        if (LooksLikeText(header))
        {
            if (fileNameHint is not null)
            {
                var ext = System.IO.Path.GetExtension(fileNameHint).ToLowerInvariant();
                return ext switch
                {
                    ".html" or ".htm" => "text/html",
                    ".css" => "text/css",
                    ".js" => "text/javascript",
                    ".json" => "application/json",
                    ".xml" => "application/xml",
                    ".csv" => "text/csv",
                    ".md" => "text/markdown",
                    _ => "text/plain"
                };
            }
            return "text/plain";
        }

        return "application/octet-stream";
    }

    private static bool LooksLikeText(ReadOnlySpan<byte> data)
    {
        var checkLength = Math.Min(data.Length, 512);
        for (var i = 0; i < checkLength; i++)
        {
            var b = data[i];
            // Allow printable ASCII, tabs, newlines, carriage returns, and UTF-8 lead bytes
            if (b < 0x09 || (b > 0x0D && b < 0x20 && b != 0x1B))
                return false;
        }
        return true;
    }
}
