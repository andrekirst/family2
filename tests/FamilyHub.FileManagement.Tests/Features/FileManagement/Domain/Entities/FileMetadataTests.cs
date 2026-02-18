using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain.Entities;

public class FileMetadataTests
{
    [Fact]
    public void Create_WithGpsData_ShouldSetHasGpsDataTrue()
    {
        var metadata = FileMetadata.Create(
            FileId.New(),
            Latitude.From(48.8566),
            Longitude.From(2.3522),
            "Paris, France",
            "Canon EOS R5",
            new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc),
            null);

        metadata.HasGpsData.Should().BeTrue();
        metadata.GpsLatitude!.Value.Value.Should().BeApproximately(48.8566, 0.0001);
        metadata.GpsLongitude!.Value.Value.Should().BeApproximately(2.3522, 0.0001);
        metadata.LocationName.Should().Be("Paris, France");
        metadata.CameraModel.Should().Be("Canon EOS R5");
    }

    [Fact]
    public void Create_WithoutGpsData_ShouldSetHasGpsDataFalse()
    {
        var metadata = FileMetadata.Create(
            FileId.New(),
            null,
            null,
            null,
            "iPhone 15 Pro",
            new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc),
            null);

        metadata.HasGpsData.Should().BeFalse();
        metadata.GpsLatitude.Should().BeNull();
        metadata.GpsLongitude.Should().BeNull();
        metadata.CameraModel.Should().Be("iPhone 15 Pro");
    }

    [Fact]
    public void UpdateLocationName_ShouldChangeLocationName()
    {
        var metadata = FileMetadata.Create(
            FileId.New(),
            Latitude.From(48.8566),
            Longitude.From(2.3522),
            null,
            null,
            null,
            null);

        metadata.UpdateLocationName("Paris, France");

        metadata.LocationName.Should().Be("Paris, France");
    }

    [Fact]
    public void Create_ShouldSetExtractedAtToUtcNow()
    {
        var metadata = FileMetadata.Create(
            FileId.New(), null, null, null, null, null, null);

        metadata.ExtractedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
