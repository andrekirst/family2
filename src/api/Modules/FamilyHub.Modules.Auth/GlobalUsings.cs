// Global using directives

global using FamilyHub.Modules.Family.Domain.Constants;
global using FamilyHub.Modules.Family.Domain.Events;
global using FamilyHub.Modules.Family.Domain.Repositories;
global using FamilyHub.Modules.Family.Domain.ValueObjects;
global using Vogen;
// Family Domain Types (from Family module)
// Using type aliases to avoid namespace conflicts with FamilyHub.Modules.Family namespace
// PHASE 5 STATE: Auth module still references Family domain for:
// - GraphQL type extensions (FamilyTypeExtensions, UserTypeExtensions)
// - Application commands/queries that query Family data (AcceptInvitation, GetPendingInvitations)
// - Repository interfaces (IFamilyRepository, IFamilyMemberInvitationRepository) for cross-module queries
// Repository implementations are in Family module; Auth injects them via DI
global using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;
global using FamilyMemberInvitationAggregate = FamilyHub.Modules.Family.Domain.Aggregates.FamilyMemberInvitation;
