using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.ModuleName.Application.Queries.QueryName;

public sealed record QueryNameQuery(
    UserId UserId) : IQuery<QueryNameResult>, IRequireUser;

public sealed record QueryNameResult(bool Success);
