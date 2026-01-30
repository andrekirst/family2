using FamilyHub.Api.Application.Common;
using FamilyHub.Api.Application.Services;
using FamilyHub.Api.Domain.ValueObjects;
using FamilyHub.Api.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Application.Commands;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword) : IRequest<OperationResult>;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}

public sealed class ChangePasswordCommandHandler(
    AppDbContext db,
    IPasswordService passwordService)
    : IRequestHandler<ChangePasswordCommand, OperationResult>
{
    public async Task<OperationResult> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var userId = UserId.From(request.UserId);

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
            return OperationResult.Failure("User not found");

        // Verify current password
        if (!passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            return OperationResult.Failure("Current password is incorrect");

        // Validate new password strength
        var (isValid, error) = passwordService.ValidatePasswordStrength(request.NewPassword);
        if (!isValid)
            return OperationResult.Failure(error!);

        // Set new password
        var passwordHash = passwordService.HashPassword(request.NewPassword);
        user.SetPassword(passwordHash);

        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }
}
