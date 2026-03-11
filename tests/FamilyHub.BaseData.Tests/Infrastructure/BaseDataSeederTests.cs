using FamilyHub.Api.Features.BaseData.Domain.Entities;
using FamilyHub.Api.Features.BaseData.Domain.Repositories;
using FamilyHub.Api.Features.BaseData.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FamilyHub.BaseData.Tests.Infrastructure;

public class BaseDataSeederTests
{
    private readonly IFederalStateRepository _repository;
    private readonly BaseDataSeeder _seeder;

    public BaseDataSeederTests()
    {
        _repository = Substitute.For<IFederalStateRepository>();

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IFederalStateRepository))
            .Returns(_repository);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        var logger = Substitute.For<ILogger<BaseDataSeeder>>();

        _seeder = new BaseDataSeeder(scopeFactory, logger);
    }

    [Fact]
    public async Task StartAsync_ShouldInsertData_WhenRepositoryIsEmpty()
    {
        // Arrange
        _repository.AnyAsync(Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _seeder.StartAsync(CancellationToken.None);

        // Assert
        await _repository.Received(1)
            .AddRangeAsync(
                Arg.Is<IEnumerable<FederalState>>(list => list.Count() == 16),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_ShouldNotInsertData_WhenRepositoryHasData()
    {
        // Arrange
        _repository.AnyAsync(Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _seeder.StartAsync(CancellationToken.None);

        // Assert
        await _repository.DidNotReceive()
            .AddRangeAsync(
                Arg.Any<IEnumerable<FederalState>>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_ShouldLoadAllGermanFederalStates()
    {
        // Arrange
        var capturedStates = new List<FederalState>();
        _repository.AnyAsync(Arg.Any<CancellationToken>())
            .Returns(false);
        _repository.AddRangeAsync(Arg.Any<IEnumerable<FederalState>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedStates.AddRange(callInfo.Arg<IEnumerable<FederalState>>());
                return Task.CompletedTask;
            });

        // Act
        await _seeder.StartAsync(CancellationToken.None);

        // Assert
        capturedStates.Should().HaveCount(16);
        capturedStates.Select(s => s.Iso3166Code.Value).Should().Contain("DE-SN");
        capturedStates.Select(s => s.Iso3166Code.Value).Should().Contain("DE-BW");
        capturedStates.Select(s => s.Iso3166Code.Value).Should().Contain("DE-BY");
        capturedStates.Select(s => s.Name.Value).Should().Contain("Sachsen");
        capturedStates.Select(s => s.Name.Value).Should().Contain("Bayern");
    }

    [Fact]
    public async Task StopAsync_ShouldCompleteWithoutError()
    {
        // Act
        var act = () => _seeder.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
