using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Domain.Entities;

public sealed class SchoolYear
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private SchoolYear() { }
#pragma warning restore CS8618

    public static SchoolYear Create(
        FamilyId familyId,
        FederalStateId federalStateId,
        int startYear,
        int endYear,
        DateOnly startDate,
        DateOnly endDate,
        DateTimeOffset utcNow)
    {
        return new SchoolYear
        {
            Id = SchoolYearId.New(),
            FamilyId = familyId,
            FederalStateId = federalStateId,
            StartYear = startYear,
            EndYear = endYear,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = utcNow.UtcDateTime,
            UpdatedAt = utcNow.UtcDateTime
        };
    }

    public SchoolYearId Id { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public FederalStateId FederalStateId { get; private set; }
    public int StartYear { get; private set; }
    public int EndYear { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public bool IsCurrent(DateOnly today) => StartDate <= today && today <= EndDate;

    public void Update(
        FederalStateId federalStateId,
        int startYear,
        int endYear,
        DateOnly startDate,
        DateOnly endDate,
        DateTimeOffset utcNow)
    {
        FederalStateId = federalStateId;
        StartYear = startYear;
        EndYear = endYear;
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = utcNow.UtcDateTime;
    }
}
