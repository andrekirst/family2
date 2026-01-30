using FamilyHub.Api.Application.Common;
using FamilyHub.Api.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Application.Commands;

public sealed record VerifyEmailCommand(string Token) : IRequest<OperationResult<VerifyEmailResult>>;

public sealed record VerifyEmailResult(bool Success, string Message);

public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}

public sealed class VerifyEmailCommandHandler(AppDbContext db)
    : IRequestHandler<VerifyEmailCommand, OperationResult<VerifyEmailResult>>
{
    public async Task<OperationResult<VerifyEmailResult>> Handle(VerifyEmailCommand request, CancellationToken ct)
    {
        // Find user by verification token
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == request.Token, ct);

        if (user == null)
            return OperationResult.Failure<VerifyEmailResult>("Invalid verification token");

        if (user.EmailVerified)
            return new VerifyEmailResult(true, "Email already verified");

        if (!user.VerifyEmailWithToken(request.Token))
            return OperationResult.Failure<VerifyEmailResult>("Verification token has expired");

        await db.SaveChangesAsync(ct);

        return new VerifyEmailResult(true, "Email verified successfully");
    }
}
