namespace FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;

/// <summary>
/// Namespace type for calendar mutations nested under family.
/// Produces: mutation { family { calendar { create, update, cancel } } }
/// </summary>
public class FamilyCalendarMutation;
