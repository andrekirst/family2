using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.School.Infrastructure.Repositories;

public sealed class StudentRepository(AppDbContext context) : IStudentRepository
{
    public async Task<Student?> GetByIdAsync(StudentId id, CancellationToken ct = default)
    {
        return await context.Students.FindAsync([id], cancellationToken: ct);
    }

    public async Task<bool> ExistsByIdAsync(StudentId id, CancellationToken ct = default)
    {
        return await context.Students.AnyAsync(s => s.Id == id, ct);
    }

    public async Task<List<Student>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
    {
        return await context.Students
            .Where(s => s.FamilyId == familyId)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByFamilyMemberIdAsync(FamilyMemberId familyMemberId, CancellationToken ct = default)
    {
        return await context.Students
            .AnyAsync(s => s.FamilyMemberId == familyMemberId, ct);
    }

    public async Task AddAsync(Student student, CancellationToken ct = default)
    {
        await context.Students.AddAsync(student, ct);
    }
}
