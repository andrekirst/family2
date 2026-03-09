namespace FamilyHub.Common.Application;

/// <summary>
/// Marker interface for query results that contain streaming data (e.g., file downloads).
/// <para>
/// Contract: The handler that creates this result must NOT wrap the stream in a using statement.
/// The endpoint that receives this result owns disposal of the stream.
/// </para>
/// <para>
/// The <see cref="StreamingBehavior{TMessage,TResponse}"/> detects this interface and sets a
/// diagnostic flag so downstream <see cref="LoggingBehavior"/> skips serializing stream content.
/// </para>
/// </summary>
public interface IStreamableResult
{
    /// <summary>The binary data stream.</summary>
    Stream Data { get; }

    /// <summary>MIME type of the stream content (e.g., "image/png").</summary>
    string MimeType { get; }

    /// <summary>Content length in bytes, if known. Null for unbounded streams.</summary>
    long? ContentLength { get; }
}
