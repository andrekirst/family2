namespace FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;

/// <summary>
/// Namespace type for invitation queries (admin/family view + public token lookup).
/// No [Authorize] at this level — byToken is public, pendings has its own auth.
/// Extended by Family module.
/// </summary>
public class InvitationsQuery;
