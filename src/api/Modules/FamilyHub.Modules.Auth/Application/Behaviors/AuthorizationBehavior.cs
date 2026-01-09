using System.Security.Claims;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Constants;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enforces authorization requirements using marker interfaces.
/// Executes after UserContextEnrichmentBehavior and before ValidationBehavior in the pipeline.
/// Skips authorization for public queries (IPublicQuery marker).
/// Checks family context and role-based authorization for authenticated requests.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed partial class AuthorizationBehavior<TRequest, TResponse>(
    IUserContext userContext,
    IAuthorizationService authorizationService,
    ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Skip authorization for public queries
        if (request is IPublicQuery)
        {
            LogSkippingPublicQuery(typeof(TRequest).Name);
            return await next(cancellationToken);
        }

        // Family context check - verify user has a family
        if (request is IRequireFamilyContext && userContext.FamilyId == FamilyId.From(Guid.Empty))
        {
            LogFamilyContextCheckFailed(typeof(TRequest).Name);
            throw new UnauthorizedAccessException(
                "User does not belong to a family. " +
                "This operation requires the user to be a member of a family.");
        }

        // Role-based authorization using ASP.NET Core policies
        var policyName = request switch
        {
            IRequireOwnerRole => AuthorizationPolicyConstants.RequireOwner,
            IRequireAdminRole => AuthorizationPolicyConstants.RequireAdmin,
            IRequireOwnerOrAdminRole => AuthorizationPolicyConstants.RequireOwnerOrAdmin,
            _ => null
        };

        if (policyName != null)
        {
            // Create ClaimsPrincipal from UserContext for authorization service
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userContext.UserId.Value.ToString()),
                new Claim(ClaimTypes.Email, userContext.Email.Value),
                new Claim(ClaimTypes.Role, userContext.Role.Value)
            };
            var identity = new ClaimsIdentity(claims, "UserContext");
            var principal = new ClaimsPrincipal(identity);

            // Execute authorization policy
            var authResult = await authorizationService.AuthorizeAsync(
                principal,
                null,
                policyName);

            if (!authResult.Succeeded)
            {
                LogAuthorizationFailed(typeof(TRequest).Name, policyName);
                throw new UnauthorizedAccessException(
                    $"User does not have the required role for this operation. " +
                    $"Policy '{policyName}' failed. Current role: {userContext.Role.Value}");
            }

            LogAuthorizationSucceeded(typeof(TRequest).Name, policyName);
        }

        // Authorization checks passed, continue pipeline
        return await next(cancellationToken);
    }

    [LoggerMessage(LogLevel.Debug, "Skipping authorization for public query: {requestName}")]
    partial void LogSkippingPublicQuery(string requestName);

    [LoggerMessage(LogLevel.Debug, "Authorization succeeded for {requestName} (Policy: {policyName})")]
    partial void LogAuthorizationSucceeded(string requestName, string policyName);

    [LoggerMessage(LogLevel.Warning, "Authorization failed for {requestName} (Policy: {policyName})")]
    partial void LogAuthorizationFailed(string requestName, string policyName);

    [LoggerMessage(LogLevel.Warning, "Family context check failed for {requestName}: User does not belong to a family")]
    partial void LogFamilyContextCheckFailed(string requestName);
}
