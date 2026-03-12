using FamilyHub.Api.Features.School.Models;

namespace FamilyHub.Api.Features.School.Application.Mappers;

public static class SchoolMapper
{
    public static SchoolDto ToDto(Domain.Entities.School school)
    {
        return new SchoolDto
        {
            Id = school.Id.Value,
            Name = school.Name.Value,
            FamilyId = school.FamilyId.Value,
            FederalStateId = school.FederalStateId.Value,
            City = school.City,
            PostalCode = school.PostalCode,
            CreatedAt = school.CreatedAt,
            UpdatedAt = school.UpdatedAt
        };
    }
}
