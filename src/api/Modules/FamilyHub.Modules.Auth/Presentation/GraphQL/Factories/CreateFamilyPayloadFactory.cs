using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Adapters;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Factories;

/// <summary>
/// Factory for creating CreateFamilyPayload instances.
/// Implements the IPayloadFactory pattern for type-safe, reflection-free payload construction.
/// </summary>
public class CreateFamilyPayloadFactory : IPayloadFactory<CreateFamilyResult, CreateFamilyPayload>
{
    /// <summary>
    /// Creates a success payload from the CreateFamilyResult.
    /// </summary>
    /// <param name="result">The successful CreateFamilyResult from the command handler</param>
    /// <returns>A CreateFamilyPayload containing the created family</returns>
    public CreateFamilyPayload Success(CreateFamilyResult result)
    {
        var familyType = FamilyOutputAdapter.ToGraphQLType(result);

        return new CreateFamilyPayload(familyType);
    }

    /// <summary>
    /// Creates an error payload from a list of errors.
    /// </summary>
    /// <param name="errors">List of errors that occurred during family creation</param>
    /// <returns>A CreateFamilyPayload containing the errors</returns>
    public CreateFamilyPayload Error(IReadOnlyList<UserError> errors)
    {
        return new CreateFamilyPayload(errors);
    }
}
