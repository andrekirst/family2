using FamilyHub.Api.Application.Common;
using FamilyHub.Api.Application.Services;
using FamilyHub.Api.Domain.ValueObjects;
using FamilyHub.Api.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Application.Commands;

public sealed record RequestPasswordResetCommand(string Email) : IRequest<OperationResult>;

public sealed class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public sealed class RequestPasswordResetCommandHandler(
    AppDbContext db,
    IEmailService emailService)
    : IRequestHandler<RequestPasswordResetCommand, OperationResult>
{
    public async Task<OperationResult> Handle(RequestPasswordResetCommand request, CancellationToken ct)
    {
        // Validate email format
        if (!Email.TryFrom(request.Email, out var email))
            return OperationResult.Success(); // Don't reveal if email exists

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        // Always return success to prevent email enumeration
        if (user == null)
            return OperationResult.Success();

        user.GeneratePasswordResetToken();
        await db.SaveChangesAsync(ct);

        await emailService.SendPasswordResetEmailAsync(
            email.Value,
            user.PasswordResetToken!,
            ct);

        return OperationResult.Success();
    }
}
