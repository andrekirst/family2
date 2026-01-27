using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.RequestPasswordReset;

/// <summary>
/// Handler for RequestPasswordResetCommand.
/// </summary>
/// <param name="authService">Service handling password reset logic.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class RequestPasswordResetCommandHandler(
    IAuthService authService,
    ILogger<RequestPasswordResetCommandHandler> logger)
    : ICommandHandler<RequestPasswordResetCommand, FamilyHub.SharedKernel.Domain.Result<RequestPasswordResetResult>>
{
    /// <inheritdoc />
    public async Task<FamilyHub.SharedKernel.Domain.Result<RequestPasswordResetResult>> Handle(
        RequestPasswordResetCommand request,
        CancellationToken cancellationToken)
    {
        LogPasswordResetRequest(request.Email.Value, request.UseMobileCode);

        var serviceRequest = new RequestPasswordResetRequest
        {
            Email = request.Email.Value,
            UseMobileCode = request.UseMobileCode
        };

        // Always return success regardless of whether email exists (security)
        await authService.RequestPasswordResetAsync(serviceRequest, cancellationToken);

        LogPasswordResetRequestCompleted(request.Email.Value);

        return Result.Success(new RequestPasswordResetResult());
    }

    [LoggerMessage(LogLevel.Information, "Password reset requested for email {Email}, mobile code: {UseMobileCode}")]
    partial void LogPasswordResetRequest(string email, bool useMobileCode);

    [LoggerMessage(LogLevel.Information, "Password reset request completed for email {Email}")]
    partial void LogPasswordResetRequestCompleted(string email);
}
