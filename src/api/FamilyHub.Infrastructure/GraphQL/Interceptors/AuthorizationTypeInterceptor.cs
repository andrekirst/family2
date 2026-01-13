using System.Reflection;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using HotChocolate.Configuration;
using HotChocolate.Language;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors.Definitions;

namespace FamilyHub.Infrastructure.GraphQL.Interceptors;

/// <summary>
/// Type interceptor that automatically applies authorization policies to GraphQL mutation fields
/// based on marker interfaces implemented by the mutation class.
/// </summary>
/// <remarks>
/// <para>
/// This interceptor follows the Open-Closed Principle: authorization requirements are declared
/// via interface implementation on the mutation class, not via attributes on individual methods.
/// This provides consistency with MediatR command authorization via <see cref="IRequireAuthentication"/>
/// and related interfaces.
/// </para>
/// <para>
/// Interface to Policy Mapping:
/// <list type="bullet">
/// <item><see cref="IRequireOwnerRole"/> → "RequireOwner" policy</item>
/// <item><see cref="IRequireAdminRole"/> → "RequireAdmin" policy</item>
/// <item><see cref="IRequireOwnerOrAdminRole"/> → "RequireOwnerOrAdmin" policy</item>
/// <item><see cref="IRequireAuthentication"/> → Basic authentication required (no specific role)</item>
/// </list>
/// </para>
/// <para>
/// Individual methods can override class-level authorization using:
/// <list type="bullet">
/// <item><c>[Authorize]</c> or <c>[Authorize(Policy = "...")]</c> - explicit authorization</item>
/// <item><c>[AllowAnonymous]</c> - skip authorization entirely</item>
/// </list>
/// </para>
/// <para>
/// Usage:
/// <code>
/// [ExtendObjectType("Mutation")]
/// public sealed class InvitationMutations : IRequireOwnerOrAdminRole
/// {
///     // All methods automatically require Owner or Admin role
///     public async Task&lt;Result&gt; InviteFamilyMember(...) { ... }
///
///     // Override: This method only requires authentication
///     [Authorize]
///     public async Task&lt;Result&gt; AcceptInvitation(...) { ... }
/// }
/// </code>
/// </para>
/// </remarks>
public sealed class AuthorizationTypeInterceptor : TypeInterceptor
{
    // Policy constants - these match AuthorizationPolicyConstants in Auth module
    // Using string literals to avoid circular dependency (Infrastructure → Auth)
    private const string RequireOwnerPolicy = "RequireOwner";
    private const string RequireAdminPolicy = "RequireAdmin";
    private const string RequireOwnerOrAdminPolicy = "RequireOwnerOrAdmin";

    // Directive name for authorization
    private const string AuthorizeDirectiveName = "authorize";

    /// <inheritdoc />
    public override void OnBeforeCompleteType(
        ITypeCompletionContext completionContext,
        DefinitionBase definition)
    {
        if (definition is not ObjectTypeDefinition objectTypeDef)
        {
            return;
        }

        // Only process the Mutation type
        if (objectTypeDef.Name != "Mutation")
        {
            return;
        }

        // Process each field in the mutation type
        foreach (var field in objectTypeDef.Fields)
        {
            // Skip fields without a member (e.g., synthetic fields)
            if (field.Member is not MethodInfo method)
            {
                continue;
            }

            // Get the declaring type (the class that defines this method)
            var declaringType = method.DeclaringType;
            if (declaringType is null)
            {
                continue;
            }

            // Check if the declaring type is marked with [ExtendObjectType("Mutation")]
            var extendAttribute = declaringType.GetCustomAttribute<ExtendObjectTypeAttribute>();
            if (extendAttribute?.Name != "Mutation")
            {
                continue;
            }

            // Skip if field has explicit authorization attribute
            if (HasExplicitAuthorizationAttribute(field))
            {
                continue;
            }

            // Determine authorization based on the declaring type's interfaces
            var policy = DetermineAuthorizationPolicy(declaringType);
            var requiresAuthentication = typeof(IRequireAuthentication).IsAssignableFrom(declaringType);

            if (policy is null && !requiresAuthentication)
            {
                // No authorization interfaces implemented - skip
                continue;
            }

            // Apply authorization to this field
            ApplyAuthorizationToField(field, policy, requiresAuthentication);
        }
    }

    /// <summary>
    /// Determines the authorization policy based on implemented interfaces.
    /// Returns the most specific policy.
    /// </summary>
    /// <remarks>
    /// Priority order (most restrictive first):
    /// 1. IRequireOwnerRole → RequireOwner
    /// 2. IRequireAdminRole → RequireAdmin
    /// 3. IRequireOwnerOrAdminRole → RequireOwnerOrAdmin
    /// 4. IRequireAuthentication → null (basic auth, no role policy)
    /// </remarks>
    private static string? DetermineAuthorizationPolicy(Type declaringType)
    {
        // Check for specific role requirements (most restrictive first)
        if (typeof(IRequireOwnerRole).IsAssignableFrom(declaringType))
        {
            return RequireOwnerPolicy;
        }

        if (typeof(IRequireAdminRole).IsAssignableFrom(declaringType))
        {
            return RequireAdminPolicy;
        }

        if (typeof(IRequireOwnerOrAdminRole).IsAssignableFrom(declaringType))
        {
            return RequireOwnerOrAdminPolicy;
        }

        // No specific policy, but may still require authentication via IRequireAuthentication
        return null;
    }

    /// <summary>
    /// Checks if a field has explicit authorization attributes that should prevent
    /// automatic authorization application.
    /// </summary>
    private static bool HasExplicitAuthorizationAttribute(ObjectFieldDefinition field)
    {
        var member = field.Member;
        if (member is null)
        {
            return false;
        }

        // Check for Hot Chocolate [Authorize] attribute
        if (member.GetCustomAttribute<HotChocolate.Authorization.AuthorizeAttribute>() is not null)
        {
            return true;
        }

        // Check for ASP.NET Core [Authorize] attribute (fallback)
        if (member.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() is not null)
        {
            return true;
        }

        // Check for Hot Chocolate [AllowAnonymous] attribute
        if (member.GetCustomAttribute<HotChocolate.Authorization.AllowAnonymousAttribute>() is not null)
        {
            return true;
        }

        // Check for ASP.NET Core [AllowAnonymous] attribute (fallback)
        if (member.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() is not null)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Applies authorization directive to a field definition using GraphQL SDL directive node.
    /// </summary>
    private static void ApplyAuthorizationToField(
        ObjectFieldDefinition field,
        string? policy,
        bool requiresAuthentication)
    {
        DirectiveNode directiveNode;

        if (policy is not null)
        {
            // Create @authorize(policy: "PolicyName") directive
            directiveNode = new DirectiveNode(
                AuthorizeDirectiveName,
                new ArgumentNode("policy", policy));
        }
        else if (requiresAuthentication)
        {
            // Create @authorize directive (basic authentication)
            directiveNode = new DirectiveNode(AuthorizeDirectiveName);
        }
        else
        {
            return;
        }

        // Add the directive to the field definition
        field.Directives.Add(new DirectiveDefinition(directiveNode));
    }
}
