using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FluentAssertions;

namespace FamilyHub.Messaging.Tests.Features.Messaging.Domain;

public class MessageAttachmentTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange
        var fileId = FileId.New();
        var fileName = "family-photo.jpg";
        var mimeType = "image/jpeg";
        var fileSize = 1024L * 512;

        // Act
        var attachment = MessageAttachment.Create(fileId, fileName, mimeType, fileSize, "uploads/family-photo.jpg");

        // Assert
        attachment.Id.Should().NotBe(Guid.Empty);
        attachment.FileId.Should().Be(fileId);
        attachment.FileName.Should().Be(fileName);
        attachment.MimeType.Should().Be(mimeType);
        attachment.FileSize.Should().Be(fileSize);
        attachment.AttachedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange
        var fileId = FileId.New();

        // Act
        var a1 = MessageAttachment.Create(fileId, "file1.txt", "text/plain", 100, "uploads/file1.txt");
        var a2 = MessageAttachment.Create(fileId, "file2.txt", "text/plain", 200, "uploads/file2.txt");

        // Assert
        a1.Id.Should().NotBe(a2.Id);
    }
}
