# References — Shared Family Calendar

## Key Pattern Files

| File | Pattern |
|------|---------|
| `Features/Family/Domain/Entities/Family.cs` | Aggregate root with factory Create() |
| `Features/Family/Domain/ValueObjects/FamilyId.cs` | Vogen Guid value object |
| `Features/Family/Domain/ValueObjects/FamilyName.cs` | Vogen string value object |
| `Features/Family/Domain/Events/FamilyCreatedEvent.cs` | Domain event sealed record |
| `Features/Family/Data/FamilyConfiguration.cs` | EF Core config with Vogen conversions |
| `Features/Family/Domain/Repositories/IFamilyRepository.cs` | Repository interface |
| `Features/Family/Infrastructure/Repositories/FamilyRepository.cs` | Repository implementation |
| `Features/Family/Application/Commands/CreateFamilyCommand.cs` | ICommand record |
| `Features/Family/Application/Handlers/CreateFamilyCommandHandler.cs` | Wolverine static handler |
| `Features/Family/Application/Validators/CreateFamilyCommandValidator.cs` | FluentValidation |
| `Features/Family/Application/Mappers/FamilyMapper.cs` | Static mapper |
| `Features/Family/GraphQL/FamilyMutations.cs` | GraphQL mutation with Input→Command |
| `Features/Family/GraphQL/FamilyQueries.cs` | GraphQL query with IQueryBus |
| `Common/Domain/AggregateRoot.cs` | Base aggregate with domain events |
| `Common/Domain/DomainEvent.cs` | Base domain event record |
| `Migrations/AddRlsPolicies.cs` | RLS migration pattern |
| `Program.cs` | DI registration + GraphQL setup |
| `frontend/.../family.service.ts` | Angular service with Apollo |
| `frontend/.../family.operations.ts` | GraphQL gql operations |
| `frontend/.../dashboard.component.ts` | Smart component + dialog pattern |
| `frontend/.../create-family-dialog.component.ts` | Dialog with signals + FormsModule |
| `frontend/.../app.routes.ts` | Route registration with authGuard |
