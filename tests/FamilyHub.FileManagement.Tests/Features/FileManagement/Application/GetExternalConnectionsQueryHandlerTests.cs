using FamilyHub.Api.Features.FileManagement.Application.Queries.GetExternalConnections;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetExternalConnectionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnFamilyConnections()
    {
        var repo = new FakeExternalConnectionRepository();
        var handler = new GetExternalConnectionsQueryHandler(repo);
        var familyId = FamilyId.New();

        repo.Connections.Add(ExternalConnection.Create(
            familyId, ExternalProviderType.OneDrive, "OneDrive",
            "token1", "refresh1", DateTime.UtcNow.AddHours(1), UserId.New()));
        repo.Connections.Add(ExternalConnection.Create(
            familyId, ExternalProviderType.GoogleDrive, "Google Drive",
            "token2", "refresh2", DateTime.UtcNow.AddHours(1), UserId.New()));

        var query = new GetExternalConnectionsQuery(familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldNotReturnOtherFamilyConnections()
    {
        var repo = new FakeExternalConnectionRepository();
        var handler = new GetExternalConnectionsQueryHandler(repo);

        repo.Connections.Add(ExternalConnection.Create(
            FamilyId.New(), ExternalProviderType.Dropbox, "Dropbox",
            "token", "refresh", DateTime.UtcNow.AddHours(1), UserId.New()));

        var query = new GetExternalConnectionsQuery(FamilyId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapDtoFields()
    {
        var repo = new FakeExternalConnectionRepository();
        var handler = new GetExternalConnectionsQueryHandler(repo);
        var familyId = FamilyId.New();

        repo.Connections.Add(ExternalConnection.Create(
            familyId, ExternalProviderType.PaperlessNgx, "My Paperless",
            "api-token", null, null, UserId.New()));

        var query = new GetExternalConnectionsQuery(familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        var dto = result.Single();
        dto.ProviderType.Should().Be("PaperlessNgx");
        dto.DisplayName.Should().Be("My Paperless");
        dto.Status.Should().Be("Connected");
        dto.IsTokenExpired.Should().BeFalse();
    }
}
