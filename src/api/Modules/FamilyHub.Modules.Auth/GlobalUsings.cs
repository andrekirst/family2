// Global using directives

global using FamilyHub.Modules.Family.Domain.Constants;
global using FamilyHub.Modules.Family.Domain.Events;
global using FamilyHub.Modules.Family.Domain.Repositories;
global using FamilyHub.Modules.Family.Domain.ValueObjects;
global using Vogen;
// Family Domain Types (from Family module)
// Using type aliases to avoid namespace conflicts with FamilyHub.Modules.Family namespace
global using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;
global using FamilyMemberInvitationAggregate = FamilyHub.Modules.Family.Domain.Aggregates.FamilyMemberInvitation;
