using FamilyHub.Api.Features.BaseData.Application.Queries.GetFederalStateByIso3166;
using FamilyHub.Api.Features.BaseData.Domain.Entities;
using FamilyHub.Api.Features.BaseData.Domain.Repositories;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.BaseData.Tests.Application;

public class GetFederalStateByIso3166QueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnDto_WhenCodeExists()
    {
        // Arrange
        var state = FederalState.Create(
            FederalStateName.From("Sachsen"),
            Iso3166Code.From("DE-SN"));

        var repository = Substitute.For<IFederalStateRepository>();
        var isoCode = Iso3166Code.From("DE-SN");
        repository.GetByIso3166CodeAsync(isoCode, Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(state);

        var handler = new GetFederalStateByIso3166QueryHandler(repository);
        var query = new GetFederalStateByIso3166Query("DE-SN");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(state.Id.Value);
        result.Name.Should().Be("Sachsen");
        result.Iso3166Code.Should().Be("DE-SN");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCodeDoesNotExist()
    {
        // Arrange
        var repository = Substitute.For<IFederalStateRepository>();
        var isoCode = Iso3166Code.From("DE-XX");
        repository.GetByIso3166CodeAsync(isoCode, Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((FederalState?)null);

        var handler = new GetFederalStateByIso3166QueryHandler(repository);
        var query = new GetFederalStateByIso3166Query("DE-XX");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldConvertStringCodeToValueObject()
    {
        // Arrange
        var state = FederalState.Create(
            FederalStateName.From("Bayern"),
            Iso3166Code.From("DE-BY"));

        var repository = Substitute.For<IFederalStateRepository>();
        repository.GetByIso3166CodeAsync(Iso3166Code.From("DE-BY"), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(state);

        var handler = new GetFederalStateByIso3166QueryHandler(repository);
        var query = new GetFederalStateByIso3166Query("DE-BY");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Bayern");
        result.Iso3166Code.Should().Be("DE-BY");
    }
}
