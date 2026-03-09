using FamilyHub.Api.Features.FileManagement.Application.Queries.GetExternalConnections;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetExternalConnectionsQueryHandlerTests
{
    private readonly IExternalConnectionRepository _repo = Substitute.For<IExternalConnectionRepository>();
    private readonly GetExternalConnectionsQueryHandler _handler;

    public GetExternalConnectionsQueryHandlerTests()
    {
        _handler = new GetExternalConnectionsQueryHandler(_repo, TimeProvider.System);
    }

    [Fact]
    public async Task Handle_ShouldReturnFamilyConnections()
    {
        var familyId = FamilyId.New();
        _repo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([
                ExternalConnection.Create(familyId, ExternalProviderType.OneDrive, "OneDrive", "token1", "refresh1", DateTime.UtcNow.AddHours(1), UserId.New(), DateTimeOffset.UtcNow),
                ExternalConnection.Create(familyId, ExternalProviderType.GoogleDrive, "Google Drive", "token2", "refresh2", DateTime.UtcNow.AddHours(1), UserId.New(), DateTimeOffset.UtcNow)
            ]);

        var query = new GetExternalConnectionsQuery()
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldNotReturnOtherFamilyConnections()
    {
        var familyId = FamilyId.New();
        _repo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(new List<ExternalConnection>());

        var query = new GetExternalConnectionsQuery()
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapDtoFields()
    {
        var familyId = FamilyId.New();
        _repo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([
                ExternalConnection.Create(familyId, ExternalProviderType.PaperlessNgx, "My Paperless", "api-token", null, null, UserId.New(), DateTimeOffset.UtcNow)
            ]);

        var query = new GetExternalConnectionsQuery()
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        var dto = result.Single();
        dto.ProviderType.Should().Be("PaperlessNgx");
        dto.DisplayName.Should().Be("My Paperless");
        dto.Status.Should().Be("Connected");
        dto.IsTokenExpired.Should().BeFalse();
    }
}
