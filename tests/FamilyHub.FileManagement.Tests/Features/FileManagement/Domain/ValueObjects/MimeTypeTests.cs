using FluentAssertions;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using Vogen;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain.ValueObjects;

public class MimeTypeTests
{
    [Fact]
    public void From_ValidMimeType_Succeeds()
    {
        var mimeType = MimeType.From("image/png");

        mimeType.Value.Should().Be("image/png");
    }

    [Fact]
    public void From_ApplicationPdf_Succeeds()
    {
        var mimeType = MimeType.From("application/pdf");

        mimeType.Value.Should().Be("application/pdf");
    }

    [Fact]
    public void From_EmptyString_ThrowsValidation()
    {
        var act = () => MimeType.From("");

        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void From_NoSlash_ThrowsValidation()
    {
        var act = () => MimeType.From("imagepng");

        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void OctetStream_StaticField_HasCorrectValue()
    {
        MimeType.OctetStream.Value.Should().Be("application/octet-stream");
    }
}
