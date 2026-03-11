using FamilyHub.Common.Application;
using FamilyHub.Api.Features.BaseData.Models;

namespace FamilyHub.Api.Features.BaseData.Application.Queries.GetFederalStates;

public sealed record GetFederalStatesQuery : IReadOnlyQuery<List<FederalStateDto>>, IAnonymousOperation;
