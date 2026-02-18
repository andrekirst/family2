using FluentAssertions;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Infrastructure.Storage;

public class MimeDetectorTests
{
    private readonly MimeDetector _detector = new();

    [Fact]
    public void Detect_PngMagicBytes_ReturnsImagePng()
    {
        byte[] header = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];

        var result = _detector.Detect(header);

        result.Should().Be("image/png");
    }

    [Fact]
    public void Detect_JpegMagicBytes_ReturnsImageJpeg()
    {
        byte[] header = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

        var result = _detector.Detect(header);

        result.Should().Be("image/jpeg");
    }

    [Fact]
    public void Detect_PdfMagicBytes_ReturnsApplicationPdf()
    {
        byte[] header = [0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E];

        var result = _detector.Detect(header);

        result.Should().Be("application/pdf");
    }

    [Fact]
    public void Detect_ZipMagicBytes_WithDocxHint_ReturnsWordDocument()
    {
        byte[] header = [0x50, 0x4B, 0x03, 0x04, 0x00, 0x00];

        var result = _detector.Detect(header, "report.docx");

        result.Should().Be("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
    }

    [Fact]
    public void Detect_ZipMagicBytes_WithXlsxHint_ReturnsSpreadsheet()
    {
        byte[] header = [0x50, 0x4B, 0x03, 0x04, 0x00, 0x00];

        var result = _detector.Detect(header, "data.xlsx");

        result.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [Fact]
    public void Detect_ZipMagicBytes_WithoutHint_ReturnsApplicationZip()
    {
        byte[] header = [0x50, 0x4B, 0x03, 0x04, 0x00, 0x00];

        var result = _detector.Detect(header);

        result.Should().Be("application/zip");
    }

    [Fact]
    public void Detect_GifMagicBytes_ReturnsImageGif()
    {
        byte[] header = [0x47, 0x49, 0x46, 0x38, 0x39, 0x61];

        var result = _detector.Detect(header);

        result.Should().Be("image/gif");
    }

    [Fact]
    public void Detect_EmptyData_ReturnsOctetStream()
    {
        var result = _detector.Detect(ReadOnlySpan<byte>.Empty);

        result.Should().Be("application/octet-stream");
    }

    [Fact]
    public void Detect_PlainTextContent_ReturnsTextPlain()
    {
        var header = "Hello, this is a plain text file."u8.ToArray();

        var result = _detector.Detect(header);

        result.Should().Be("text/plain");
    }

    [Fact]
    public void Detect_TextWithHtmlHint_ReturnsTextHtml()
    {
        var header = "<html><head></head><body></body></html>"u8.ToArray();

        var result = _detector.Detect(header, "page.html");

        result.Should().Be("text/html");
    }

    [Fact]
    public void Detect_TextWithJsonHint_ReturnsApplicationJson()
    {
        var header = "{ \"key\": \"value\" }"u8.ToArray();

        var result = _detector.Detect(header, "data.json");

        // Magic bytes check for '{' returns application/json before text fallback
        result.Should().Be("application/json");
    }

    [Fact]
    public void Detect_BinaryData_ReturnsOctetStream()
    {
        byte[] header = [0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07];

        var result = _detector.Detect(header);

        result.Should().Be("application/octet-stream");
    }

    [Fact]
    public void Detect_Mp3WithId3Tag_ReturnsAudioMpeg()
    {
        byte[] header = [0x49, 0x44, 0x33, 0x03, 0x00, 0x00];

        var result = _detector.Detect(header);

        result.Should().Be("audio/mpeg");
    }

    [Fact]
    public void Detect_OggMagicBytes_ReturnsAudioOgg()
    {
        byte[] header = [0x4F, 0x67, 0x67, 0x53, 0x00, 0x02];

        var result = _detector.Detect(header);

        result.Should().Be("audio/ogg");
    }
}
