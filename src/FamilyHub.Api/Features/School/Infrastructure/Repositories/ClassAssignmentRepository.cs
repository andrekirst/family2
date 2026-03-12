using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.School.Infrastructure.Repositories;

public sealed class ClassAssignmentRepository(AppDbContext context) : IClassAssignmentRepository
{
    public async Task<ClassAssignment?> GetByIdAsync(ClassAssignmentId id, CancellationToken cancellationToken = default)
    {
        return await context.ClassAssignments.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(ClassAssignmentId id, CancellationToken cancellationToken = default)
    {
        return await context.ClassAssignments.AnyAsync(ca => ca.Id == id, cancellationToken);
    }

    public async Task<List<ClassAssignment>> GetByStudentIdAsync(StudentId studentId, CancellationToken cancellationToken = default)
    {
        return await context.ClassAssignments
            .Where(ca => ca.StudentId == studentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ClassAssignment>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.ClassAssignments
            .Where(ca => ca.FamilyId == familyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySchoolIdAsync(SchoolId schoolId, CancellationToken cancellationToken = default)
    {
        return await context.ClassAssignments.AnyAsync(ca => ca.SchoolId == schoolId, cancellationToken);
    }

    public async Task<bool> ExistsBySchoolYearIdAsync(SchoolYearId schoolYearId, CancellationToken cancellationToken = default)
    {
        return await context.ClassAssignments.AnyAsync(ca => ca.SchoolYearId == schoolYearId, cancellationToken);
    }

    public async Task AddAsync(ClassAssignment classAssignment, CancellationToken cancellationToken = default)
    {
        await context.ClassAssignments.AddAsync(classAssignment, cancellationToken);
    }

    public Task UpdateAsync(ClassAssignment classAssignment, CancellationToken cancellationToken = default)
    {
        context.ClassAssignments.Update(classAssignment);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ClassAssignment classAssignment, CancellationToken cancellationToken = default)
    {
        context.ClassAssignments.Remove(classAssignment);
        return Task.CompletedTask;
    }
}
