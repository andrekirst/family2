using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Family.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Family.Services;

/// <summary>
/// Service for family management operations
/// </summary>
public class FamilyService
{
    private readonly AppDbContext _context;

    public FamilyService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Create a new family with the specified owner
    /// </summary>
    public async Task<FamilyDto> CreateFamilyAsync(CreateFamilyRequest request, Guid ownerId)
    {
        // Verify owner exists
        var owner = await _context.Users.FindAsync(ownerId);
        if (owner == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if user already has a family
        if (owner.FamilyId.HasValue)
        {
            throw new InvalidOperationException("User already belongs to a family");
        }

        // Validate family name
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 100)
        {
            throw new ArgumentException("Family name must be between 1 and 100 characters", nameof(request.Name));
        }

        // Create family
        var family = new Models.Family
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Families.Add(family);

        // Update owner's FamilyId
        owner.FamilyId = family.Id;
        owner.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new FamilyDto
        {
            Id = family.Id,
            Name = family.Name,
            OwnerId = family.OwnerId,
            CreatedAt = family.CreatedAt,
            MemberCount = 1
        };
    }

    /// <summary>
    /// Get family by ID
    /// </summary>
    public async Task<FamilyDto?> GetFamilyByIdAsync(Guid familyId)
    {
        var family = await _context.Families
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.Id == familyId);

        if (family == null)
        {
            return null;
        }

        return new FamilyDto
        {
            Id = family.Id,
            Name = family.Name,
            OwnerId = family.OwnerId,
            CreatedAt = family.CreatedAt,
            MemberCount = family.Members.Count
        };
    }

    /// <summary>
    /// Get all members of a family
    /// </summary>
    public async Task<List<Auth.Models.UserDto>> GetFamilyMembersAsync(Guid familyId)
    {
        var members = await _context.Users
            .Where(u => u.FamilyId == familyId && u.IsActive)
            .Select(u => new Auth.Models.UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                Username = u.Username,
                FamilyId = u.FamilyId,
                EmailVerified = u.EmailVerified,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return members;
    }
}
