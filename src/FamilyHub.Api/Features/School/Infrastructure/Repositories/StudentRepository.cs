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
    public async Task<Student?> GetByIdAsync(StudentId id, CancellationToken cancellationToken = default)
    {
        return await context.Students.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(StudentId id, CancellationToken cancellationToken = default)
    {
        return await context.Students.AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<Student>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.Students
            .Where(s => s.FamilyId == familyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByFamilyMemberIdAsync(FamilyMemberId familyMemberId, CancellationToken cancellationToken = default)
    {
        return await context.Students
            .AnyAsync(s => s.FamilyMemberId == familyMemberId, cancellationToken);
    }

    public async Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        await context.Students.AddAsync(student, cancellationToken);
    }
}
