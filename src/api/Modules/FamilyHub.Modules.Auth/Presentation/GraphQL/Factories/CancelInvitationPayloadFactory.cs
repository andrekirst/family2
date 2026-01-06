using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Factories;

/// <summary>
/// Factory for creating CancelInvitationPayload instances.
/// </summary>
public class CancelInvitationPayloadFactory
    : IPayloadFactory<Result, CancelInvitationPayload>
{
    public CancelInvitationPayload Success(Result result)
    {
        return new CancelInvitationPayload([]);
    }

    public CancelInvitationPayload Error(IReadOnlyList<UserError> errors)
    {
        return new CancelInvitationPayload(errors);
    }
}
