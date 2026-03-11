using FamilyHub.Common.Application;
using FamilyHub.Api.Features.BaseData.Models;

namespace FamilyHub.Api.Features.BaseData.Application.Queries.GetFederalStateByIso3166;

public sealed record GetFederalStateByIso3166Query(string Code) : IReadOnlyQuery<FederalStateDto?>, IAnonymousOperation;
