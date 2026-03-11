using FamilyHub.Api.Features.BaseData.Application.Queries.GetFederalStates;
using FamilyHub.Api.Features.BaseData.Domain.Entities;
using FamilyHub.Api.Features.BaseData.Domain.Repositories;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.BaseData.Tests.Application;

public class GetFederalStatesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnDtosFromRepository()
    {
        // Arrange
        var state1 = FederalState.Create(
            FederalStateName.From("Sachsen"),
            Iso3166Code.From("DE-SN"));
        var state2 = FederalState.Create(
            FederalStateName.From("Bayern"),
            Iso3166Code.From("DE-BY"));

        var repository = Substitute.For<IFederalStateRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([state1, state2]);

        var handler = new GetFederalStatesQueryHandler(repository);
        var query = new GetFederalStatesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenRepositoryIsEmpty()
    {
        // Arrange
        var repository = Substitute.For<IFederalStateRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<FederalState>());

        var handler = new GetFederalStatesQueryHandler(repository);
        var query = new GetFederalStatesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapEntityPropertiesCorrectly()
    {
        // Arrange
        var state = FederalState.Create(
            FederalStateName.From("Sachsen"),
            Iso3166Code.From("DE-SN"));

        var repository = Substitute.For<IFederalStateRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([state]);

        var handler = new GetFederalStatesQueryHandler(repository);
        var query = new GetFederalStatesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Id.Should().Be(state.Id.Value);
        dto.Name.Should().Be("Sachsen");
        dto.Iso3166Code.Should().Be("DE-SN");
    }
}
