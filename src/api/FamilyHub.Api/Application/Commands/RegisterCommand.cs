using FamilyHub.Api.Application.Common;
using FamilyHub.Api.Application.Services;
using FamilyHub.Api.Domain.Entities;
using FamilyHub.Api.Domain.ValueObjects;
using FamilyHub.Api.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Application.Commands;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword) : IRequest<OperationResult<RegisterResult>>;

public sealed record RegisterResult(Guid UserId, string Email, string Message);

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match");
    }
}

public sealed class RegisterCommandHandler(
    AppDbContext db,
    IPasswordService passwordService,
    IEmailService emailService)
    : IRequestHandler<RegisterCommand, OperationResult<RegisterResult>>
{
    public async Task<OperationResult<RegisterResult>> Handle(RegisterCommand request, CancellationToken ct)
    {
        // Validate email format
        if (!Email.TryFrom(request.Email, out var email))
            return OperationResult.Failure<RegisterResult>("Invalid email format");

        // Check password strength
        var (isValid, error) = passwordService.ValidatePasswordStrength(request.Password);
        if (!isValid)
            return OperationResult.Failure<RegisterResult>(error!);

        // Check if email already exists
        var existingUser = await db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (existingUser != null)
            return OperationResult.Failure<RegisterResult>("Email is already registered");

        // Create user
        var passwordHash = passwordService.HashPassword(request.Password);
        var user = User.Create(email, passwordHash);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        // Send verification email
        await emailService.SendVerificationEmailAsync(
            email.Value,
            user.EmailVerificationToken!,
            ct);

        return new RegisterResult(
            user.Id.Value,
            user.Email.Value,
            "Registration successful. Please check your email to verify your account.");
    }
}
