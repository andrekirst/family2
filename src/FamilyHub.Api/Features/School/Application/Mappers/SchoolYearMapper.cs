using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Models;

namespace FamilyHub.Api.Features.School.Application.Mappers;

public static class SchoolYearMapper
{
    public static SchoolYearDto ToDto(SchoolYear schoolYear, DateOnly today)
    {
        return new SchoolYearDto
        {
            Id = schoolYear.Id.Value,
            FamilyId = schoolYear.FamilyId.Value,
            FederalStateId = schoolYear.FederalStateId.Value,
            StartYear = schoolYear.StartYear,
            EndYear = schoolYear.EndYear,
            StartDate = schoolYear.StartDate,
            EndDate = schoolYear.EndDate,
            IsCurrent = schoolYear.IsCurrent(today),
            CreatedAt = schoolYear.CreatedAt,
            UpdatedAt = schoolYear.UpdatedAt
        };
    }
}
