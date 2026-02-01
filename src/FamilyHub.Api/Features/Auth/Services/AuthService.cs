using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Auth.Services;

/// <summary>
/// Service for user authentication and management
/// </summary>
public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Register a new user from OAuth callback
    /// If user already exists (by ExternalUserId), update their information
    /// </summary>
    public async Task<UserDto> RegisterUserAsync(RegisterUserRequest request)
    {
        // Check if user already exists by external ID
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.ExternalUserId == request.ExternalUserId);

        if (existingUser != null)
        {
            // Update existing user information
            existingUser.Email = request.Email;
            existingUser.Name = request.Name;
            existingUser.EmailVerified = request.EmailVerified;
            existingUser.LastLoginAt = DateTime.UtcNow;
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(existingUser);
        }

        // Check for duplicate email
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
        {
            throw new InvalidOperationException($"User with email {request.Email} already exists");
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = request.Name,
            ExternalUserId = request.ExternalUserId,
            ExternalProvider = request.ExternalProvider,
            EmailVerified = request.EmailVerified,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    /// <summary>
    /// Get user by external OAuth provider ID
    /// </summary>
    public async Task<UserDto?> GetUserByExternalIdAsync(string externalUserId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.ExternalUserId == externalUserId);

        return user != null ? MapToDto(user) : null;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user != null ? MapToDto(user) : null;
    }

    /// <summary>
    /// Update user's family assignment
    /// </summary>
    public async Task<bool> UpdateFamilyIdAsync(Guid userId, Guid? familyId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.FamilyId = familyId;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Update user's last login timestamp
    /// </summary>
    public async Task UpdateLastLoginAsync(string externalUserId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.ExternalUserId == externalUserId);

        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Username = user.Username,
            FamilyId = user.FamilyId,
            EmailVerified = user.EmailVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
