// Global using directives

global using FamilyHub.Modules.Family.Domain.Constants;
global using FamilyHub.Modules.Family.Domain.Events;
global using FamilyHub.Modules.Family.Domain.Repositories;
global using FamilyHub.Modules.Family.Domain.ValueObjects;
global using Vogen;
// Family Domain Types (from Family module)
// Using type aliases to avoid namespace conflicts with FamilyHub.Modules.Family namespace
// ANTI-CORRUPTION LAYER STATE: Auth module accesses Family module through:
// - IFamilyService (Application Abstractions) for cross-module queries (ACL-compliant)
// - IFamilyMemberInvitationRepository for invitation management (owned by Auth domain)
// - GraphQL type extensions (FamilyTypeExtensions, UserTypeExtensions)
// Note: Direct IFamilyRepository usage has been removed to enforce ACL boundaries
global using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;
global using FamilyMemberInvitationAggregate = FamilyHub.Modules.Family.Domain.Aggregates.FamilyMemberInvitation;
