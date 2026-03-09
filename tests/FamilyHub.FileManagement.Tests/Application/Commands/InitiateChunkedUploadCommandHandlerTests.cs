using FamilyHub.Api.Features.FileManagement.Application.Commands.InitiateChunkedUpload;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Application.Commands;

public class InitiateChunkedUploadCommandHandlerTests
{
    private readonly IFileManagementStorageService _storageService = Substitute.For<IFileManagementStorageService>();
    private readonly InitiateChunkedUploadCommandHandler _handler;

    private readonly FamilyId _familyId = FamilyId.From(Guid.NewGuid());
    private readonly UserId _userId = UserId.From(Guid.NewGuid());

    private const string ExpectedUploadId = "upload-abc-123";

    public InitiateChunkedUploadCommandHandlerTests()
    {
        _handler = new InitiateChunkedUploadCommandHandler(_storageService);
    }

    [Fact]
    public async Task Handle_ShouldReturnUploadId()
    {
        // Arrange
        _storageService
            .InitiateChunkedUploadAsync(Arg.Any<CancellationToken>())
            .Returns(ExpectedUploadId);

        var command = new InitiateChunkedUploadCommand
        {
            FamilyId = _familyId,
            UserId = _userId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UploadId.Should().Be(ExpectedUploadId);
    }

    [Fact]
    public async Task Handle_ShouldCallStorageService()
    {
        // Arrange
        _storageService
            .InitiateChunkedUploadAsync(Arg.Any<CancellationToken>())
            .Returns("any-upload-id");

        var command = new InitiateChunkedUploadCommand
        {
            FamilyId = _familyId,
            UserId = _userId
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _storageService.Received(1).InitiateChunkedUploadAsync(Arg.Any<CancellationToken>());
    }
}
