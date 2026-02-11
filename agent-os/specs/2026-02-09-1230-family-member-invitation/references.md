# References

## Domain Model

- `src/FamilyHub.Api/Features/Family/Domain/Entities/Family.cs` — Family aggregate root
- `src/FamilyHub.Api/Features/Auth/Domain/Entities/User.cs` — User aggregate with FamilyId
- `src/FamilyHub.Api/Common/Domain/AggregateRoot.cs` — Base class for aggregates
- `src/FamilyHub.Api/Common/Domain/DomainEvent.cs` — Base record for domain events
- `src/FamilyHub.Api/Common/Domain/DomainException.cs` — Domain invariant violations

## Value Objects

- `src/FamilyHub.Api/Features/Family/Domain/ValueObjects/FamilyId.cs` — Vogen Guid VO pattern
- `src/FamilyHub.Api/Features/Family/Domain/ValueObjects/FamilyName.cs` — Vogen string VO pattern
- `src/FamilyHub.Api/Common/Domain/ValueObjects/Email.cs` — Reuse for invitee email

## Command/Handler Pattern

- `src/FamilyHub.Api/Features/Family/Application/Commands/CreateFamilyCommand.cs`
- `src/FamilyHub.Api/Features/Family/Application/Handlers/CreateFamilyCommandHandler.cs`
- `src/FamilyHub.Api/Common/Application/ICommand.cs`

## GraphQL Pattern

- `src/FamilyHub.Api/Features/Family/GraphQL/FamilyMutations.cs`
- `src/FamilyHub.Api/Features/Family/GraphQL/FamilyQueries.cs`

## Repository Pattern

- `src/FamilyHub.Api/Features/Family/Domain/Repositories/IFamilyRepository.cs`
- `src/FamilyHub.Api/Features/Family/Infrastructure/Repositories/FamilyRepository.cs`

## EF Configuration Pattern

- `src/FamilyHub.Api/Features/Family/Data/FamilyConfiguration.cs`
- `src/FamilyHub.Api/Features/Auth/Data/UserConfiguration.cs`

## Frontend Pattern

- `src/frontend/family-hub-web/src/app/features/family/services/family.service.ts`
- `src/frontend/family-hub-web/src/app/features/family/graphql/family.operations.ts`
- `src/frontend/family-hub-web/src/app/features/auth/callback/callback.component.ts` — Post-login redirect

## Event Chains

- `docs/architecture/event-chains-reference.md` — FamilyInvitation → email → acceptance chain

## Testing Pattern

- `tests/FamilyHub.UnitTests/Features/Family/Application/CreateFamilyCommandHandlerTests.cs`
- `tests/FamilyHub.UnitTests/Features/Family/Domain/FamilyAggregateTests.cs`
