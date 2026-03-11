using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;

namespace FamilyHub.Api.Features.BaseData.Domain.Entities;

/// <summary>
/// A German federal state (Bundesland) with ISO 3166-2 code.
/// Plain entity (not AggregateRoot) -- immutable reference data with no domain events.
/// </summary>
public sealed class FederalState
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private FederalState() { }
#pragma warning restore CS8618

    public static FederalState Create(FederalStateName name, Iso3166Code iso3166Code)
    {
        return new FederalState
        {
            Id = FederalStateId.New(),
            Name = name,
            Iso3166Code = iso3166Code
        };
    }

    public FederalStateId Id { get; private set; }

    public FederalStateName Name { get; private set; }

    public Iso3166Code Iso3166Code { get; private set; }
}
