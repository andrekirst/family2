using FamilyHub.Api.Features.FileManagement.Application.Commands.UploadChunk;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Application.Commands;

public class UploadChunkCommandHandlerTests
{
    private readonly UploadChunkCommandHandler _handler = new();

    private readonly FamilyId _familyId = FamilyId.From(Guid.NewGuid());
    private readonly UserId _userId = UserId.From(Guid.NewGuid());

    [Fact]
    public async Task Handle_ShouldReturnChunkMetadata()
    {
        // Arrange
        var command = new UploadChunkCommand("upload-xyz-789", 3, 65536)
        {
            FamilyId = _familyId,
            UserId = _userId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UploadId.Should().Be("upload-xyz-789");
        result.Value.ChunkIndex.Should().Be(3);
        result.Value.Size.Should().Be(65536);
    }
}
