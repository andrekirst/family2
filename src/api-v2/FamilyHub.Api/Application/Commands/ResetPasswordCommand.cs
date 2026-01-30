using FamilyHub.Api.Application.Common;
using FamilyHub.Api.Application.Services;
using FamilyHub.Api.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Application.Commands;

public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string ConfirmPassword) : IRequest<OperationResult>;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}

public sealed class ResetPasswordCommandHandler(
    AppDbContext db,
    IPasswordService passwordService)
    : IRequestHandler<ResetPasswordCommand, OperationResult>
{
    public async Task<OperationResult> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        // Validate password strength
        var (isValid, error) = passwordService.ValidatePasswordStrength(request.NewPassword);
        if (!isValid)
            return OperationResult.Failure(error!);

        // Find user by reset token
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token, ct);

        if (user == null)
            return OperationResult.Failure("Invalid or expired reset token");

        if (!user.ValidatePasswordResetToken(request.Token))
            return OperationResult.Failure("Invalid or expired reset token");

        // Set new password
        var passwordHash = passwordService.HashPassword(request.NewPassword);
        user.SetPassword(passwordHash);

        // Revoke all refresh tokens for security
        var tokens = await db.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.Revoke();

        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }
}
