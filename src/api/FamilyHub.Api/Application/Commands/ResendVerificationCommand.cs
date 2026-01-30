using FamilyHub.Api.Application.Common;
using FamilyHub.Api.Application.Services;
using FamilyHub.Api.Domain.ValueObjects;
using FamilyHub.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Application.Commands;

public sealed record ResendVerificationCommand(Guid UserId) : IRequest<OperationResult>;

public sealed class ResendVerificationCommandHandler(
    AppDbContext db,
    IEmailService emailService)
    : IRequestHandler<ResendVerificationCommand, OperationResult>
{
    public async Task<OperationResult> Handle(ResendVerificationCommand request, CancellationToken ct)
    {
        var userId = UserId.From(request.UserId);

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
            return OperationResult.Failure("User not found");

        if (user.EmailVerified)
            return OperationResult.Failure("Email is already verified");

        // Generate new verification token
        user.GenerateEmailVerificationToken();
        await db.SaveChangesAsync(ct);

        // Send email
        await emailService.SendVerificationEmailAsync(
            user.Email.Value,
            user.EmailVerificationToken!,
            ct);

        return OperationResult.Success();
    }
}
