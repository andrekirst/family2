using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for family operations.
/// </summary>
[ExtendObjectType("Mutation")]
public sealed class FamilyMutations
{
    /// <summary>
    /// Creates a new family with the authenticated user as owner.
    /// </summary>
    public async Task<CreateFamilyPayload> CreateFamily(
        CreateFamilyInput input,
        [Service] IMediator mediator,
        [Service] ICurrentUserService currentUserService,
        [Service] ILogger<FamilyMutations> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("GraphQL: createFamily mutation called");

            // Get authenticated user ID
            var userId = currentUserService.GetUserId();
            if (userId == null)
            {
                logger.LogWarning("Unauthenticated user attempted to create family");
                return CreateFamilyPayload.Failure(new UserError
                {
                    Message = "You must be authenticated to create a family.",
                    Code = "UNAUTHENTICATED",
                    Field = null
                });
            }

            // Create command from input
            var command = new CreateFamilyCommand(
                Name: input.Name,
                UserId: userId.Value);

            // Send command via MediatR (automatic validation via ValidationBehavior)
            var result = await mediator.Send(command, cancellationToken);

            // Map to GraphQL type
            var familyType = new FamilyType
            {
                Id = result.FamilyId.Value,
                Name = result.Name,
                OwnerId = result.OwnerId.Value,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.CreatedAt // Same as CreatedAt for new families
            };

            logger.LogInformation(
                "Family created successfully: {FamilyId} by user {UserId}",
                result.FamilyId.Value,
                userId.Value);

            return CreateFamilyPayload.Success(familyType);
        }
        catch (ValidationException ex)
        {
            // FluentValidation errors
            logger.LogWarning("Family creation failed: Validation errors");

            var errors = ex.Errors.Select(error => new UserError
            {
                Message = error.ErrorMessage,
                Code = "VALIDATION_ERROR",
                Field = error.PropertyName
            }).ToArray();

            return CreateFamilyPayload.Failure(errors);
        }
        catch (InvalidOperationException ex)
        {
            // Business rule violations (e.g., user already has a family)
            logger.LogWarning(ex, "Family creation failed: Business rule violation");

            return CreateFamilyPayload.Failure(new UserError
            {
                Message = ex.Message.Contains("already")
                    ? "You already belong to a family. You can only be the owner of one family at a time."
                    : "Failed to create family. Please try again.",
                Code = "FAMILY_ALREADY_EXISTS",
                Field = null
            });
        }
        catch (ArgumentException ex)
        {
            // Domain validation errors
            logger.LogWarning(ex, "Family creation failed: Invalid argument");

            return CreateFamilyPayload.Failure(new UserError
            {
                Message = ex.Message,
                Code = "VALIDATION_ERROR",
                Field = ex.ParamName
            });
        }
        catch (Exception ex)
        {
            // Unexpected errors
            logger.LogError(ex, "Unexpected error during family creation");

            return CreateFamilyPayload.Failure(new UserError
            {
                Message = "An unexpected error occurred while creating the family. Please try again.",
                Code = "INTERNAL_ERROR",
                Field = null
            });
        }
    }
}
