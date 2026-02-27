namespace FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;

/// <summary>
/// Namespace type for photos mutations nested under family.
/// Produces: mutation { family { photos { upload, updateCaption, delete } } }
/// </summary>
public class FamilyPhotosMutation;
