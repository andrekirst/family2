namespace FamilyHub.Common.Domain.ValueObjects;

/// <summary>
/// Shared address value object for entities that need a physical address.
/// Multi-field VO — not a Vogen type, mapped as EF Core owned type.
/// FederalStateId is stored as Guid? since Address lives in Common and cannot reference module-specific VOs.
/// </summary>
public sealed class Address
{
    // Private parameterless constructor for EF Core
    private Address() { }

    public static Address Create(
        string? street,
        string? houseNumber,
        string? postalCode,
        string? city,
        string? country,
        Guid? federalStateId)
    {
        return new Address
        {
            Street = street,
            HouseNumber = houseNumber,
            PostalCode = postalCode,
            City = city,
            Country = country,
            FederalStateId = federalStateId
        };
    }

    public string? Street { get; private set; }
    public string? HouseNumber { get; private set; }
    public string? PostalCode { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public Guid? FederalStateId { get; private set; }

    public void Update(
        string? street,
        string? houseNumber,
        string? postalCode,
        string? city,
        string? country,
        Guid? federalStateId)
    {
        Street = street;
        HouseNumber = houseNumber;
        PostalCode = postalCode;
        City = city;
        Country = country;
        FederalStateId = federalStateId;
    }
}
