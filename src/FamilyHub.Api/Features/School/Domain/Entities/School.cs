using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Events;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Domain.Entities;

public sealed class School
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private School() { }
#pragma warning restore CS8618

    public static School Create(
        SchoolName name,
        FamilyId familyId,
        FederalStateId federalStateId,
        string city,
        string postalCode,
        DateTimeOffset utcNow)
    {
        var school = new School
        {
            Id = SchoolId.New(),
            Name = name,
            FamilyId = familyId,
            FederalStateId = federalStateId,
            City = city,
            PostalCode = postalCode,
            CreatedAt = utcNow.UtcDateTime,
            UpdatedAt = utcNow.UtcDateTime
        };

        return school;
    }

    public SchoolId Id { get; private set; }
    public SchoolName Name { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public FederalStateId FederalStateId { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Update(
        SchoolName name,
        FederalStateId federalStateId,
        string city,
        string postalCode,
        DateTimeOffset utcNow)
    {
        Name = name;
        FederalStateId = federalStateId;
        City = city;
        PostalCode = postalCode;
        UpdatedAt = utcNow.UtcDateTime;
    }
}
